using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SynoAI.Models;
using SynoAI.Notifiers;
using SynoAI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using SynoAI.Extensions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SynoAI.Hubs;
using System.Drawing;
using System.Text;
using SynoAI.Models.DTOs;

namespace SynoAI.Controllers
{
    /// <summary>
    /// Controller triggered on a motion alert from synology, which will act as a bridge between the Synology camera and DeepStack AI.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CameraController : ControllerBase
    {
        // euquiq: Needed for connecting into the SignalR hub and send valid Snapshot for rt web monitoring
        private readonly IHubContext<SynoAIHub> _hubContext;

        private readonly IAIService _aiService;
        private readonly ISynologyService _synologyService;
        private readonly ILogger<CameraController> _logger;

        private static ConcurrentDictionary<string, bool> _runningCameraChecks = new(StringComparer.OrdinalIgnoreCase);
        private static ConcurrentDictionary<string, DateTime> _delayedCameraChecks = new(StringComparer.OrdinalIgnoreCase);

        private static ConcurrentDictionary<string, bool> _enabledCameras = new(StringComparer.OrdinalIgnoreCase);

        public CameraController(IAIService aiService, ISynologyService synologyService, ILogger<CameraController> logger, IHubContext<SynoAIHub> hubContext)
        {
            _hubContext = hubContext;
            _aiService = aiService;
            _synologyService = synologyService;
            _logger = logger;
        }

        /// <summary>
        /// Called by the Synology motion alert hook.
        /// </summary>
        /// <param name="id">The name of the camera.</param>
        [HttpGet]
        [Route("{id}")]
        public async void Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (_enabledCameras.TryGetValue(id, out bool enabled))
            {
                if (!enabled)
                {
                    // The camera has been disabled, so don't process any requests
                    _logger.LogInformation($"{id}: Requests for this camera will not be processed as it is currently disabled.");
                    return;
                }
            }

            // Fetch the camera
            Camera camera = Config.Cameras.FirstOrDefault(x => x.Name.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (camera == null)
            {
                _logger.LogError($"{id}: The camera was not found.");
                return;
            }

            // Ensure the camera isn't under a delay
            lock (_delayedCameraChecks)
            {
                if (_delayedCameraChecks.TryGetValue(id, out DateTime ignoreUntil) && ignoreUntil >= DateTime.UtcNow)
                {
                    // The camera is under a detection delay for the period specified, so ignore this request
                    _logger.LogInformation($"{id}: Requests for this camera will not be processed until {ignoreUntil}.");
                    return;
                }
            }

            // Ensure the camera isn't currently processing
            lock (_runningCameraChecks)
            {
                if (_runningCameraChecks.TryGetValue(id, out bool running) && running)
                {
                    // The camera is already running, so ignore this request
                    _logger.LogInformation($"{id}: The request for this camera is already running and was ignored.");
                    return;
                }
                else
                {
                    // The camera isn't running, so mark it as running
                    _runningCameraChecks.AddOrUpdate(id, true, (key, oldValue) => true);
                    _logger.LogDebug($"{id}: The camera is currently running; no other camera requests will be processed while this request is ongoing.");
                }
            }

            try
            {
                // Kick off the autocleanup
                CleanupOldImages();

                // Wait if the camera has a wait
                if (camera.Wait > 0)
                {
                    _logger.LogInformation($"{id}: Waiting for {camera.Wait}ms before fetching snapshot.");
                    await Task.Delay(camera.Wait);
                }

                // Create the stopwatches for reporting timings
                Stopwatch overallStopwatch = Stopwatch.StartNew();

                // Start loop for requesting snapshots until a valid prediction is found or MaxSnapshots is reached
                for (int snapshotCount = 1; snapshotCount <= Config.MaxSnapshots; snapshotCount++)
                {
                    // Take the snapshot from Surveillance Station
                    _logger.LogInformation($"{id}: Snapshot {snapshotCount} of {Config.MaxSnapshots} requested at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                    byte[] snapshot = await GetSnapshot(id);
                    if (snapshot == null)
                    {
                        // Failed to get any result, so skip over this snapshot
                        continue;
                    }

                    _logger.LogInformation($"{id}: Snapshot {snapshotCount} of {Config.MaxSnapshots} received at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");

                    // See if the image needs to be rotated (or further processing in the future ?) before being analyzed by the AI
                    snapshot = PreProcessSnapshot(camera, snapshot);

                    // Use the AI to get the valid predictions and then get all the valid predictions, where the result from the AI is 
                    // in the list of types and where the size of the object is bigger than the defined value.
                    IEnumerable<AIPrediction> predictions = await GetAIPredications(camera, snapshot);
                    if (predictions == null)
                    {
                        // An error occured fetching predictions, so bail-out early.
                        return;
                    }

                    _logger.LogInformation($"{id}: Snapshot {snapshotCount} of {Config.MaxSnapshots} contains {predictions.Count()} objects at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");

                    int minSizeX = camera.GetMinSizeX();
                    int minSizeY = camera.GetMinSizeY();
                    int maxSizeX = camera.GetMaxSizeX();
                    int maxSizeY = camera.GetMaxSizeY();

                    List<AIPrediction> validPredictions = new();
                    foreach (AIPrediction prediction in predictions)
                    {
                        // Check if the prediction label is in the list of types the camera is looking for
                        if (camera.Types != null && !camera.Types.Contains(prediction.Label, StringComparer.OrdinalIgnoreCase))
                        {
                            _logger.LogDebug($"{id}: Ignored '{prediction.Label}' ([{prediction.MinX},{prediction.MinY}],[{prediction.MaxX},{prediction.MaxY}]) as it's not in the valid type list ({string.Join(",", camera.Types)}) at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                        }
                        else
                        {
                            // Ensure that the prediction is bigger than the minimum size
                            if (prediction.SizeX < minSizeX || prediction.SizeY < minSizeY)
                            {
                                // The prediction is under the minimum specified size
                                _logger.LogDebug($"{id}: Ignored '{prediction.Label}' ([{prediction.MinX},{prediction.MinY}],[{prediction.MaxX},{prediction.MaxY}]) as it's under the minimum specified size ({minSizeX}x{minSizeY}) at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                            }
                            else if (prediction.SizeX > maxSizeX || prediction.SizeY > maxSizeY)
                            {
                                // The prediction has exceeded the maximum specified size
                                _logger.LogDebug($"{id}: Ignored '{prediction.Label}' ([{prediction.MinX},{prediction.MinY}],[{prediction.MaxX},{prediction.MaxY}]) as it exceeds the maximum specified size ({maxSizeX}x{maxSizeY}) at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                            }
                            else
                            {
                                bool include = ShouldIncludePrediction(id, camera, overallStopwatch, prediction);
                                if (include)
                                {
                                    validPredictions.Add(prediction);
                                    _logger.LogDebug($"{id}: Found valid prediction '{prediction.Label}' ([{prediction.MinX},{prediction.MinY}],[{prediction.MaxX},{prediction.MaxY}]) at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                                }
                            }
                        }
                    }

                    // Save the original unprocessed image if required
                    if (Config.SaveOriginalSnapshot == SaveSnapshotMode.Always ||
                        (Config.SaveOriginalSnapshot == SaveSnapshotMode.WithPredictions && predictions.Count() > 0) ||
                        (Config.SaveOriginalSnapshot == SaveSnapshotMode.WithValidPredictions && validPredictions.Count() > 0))
                    {
                        _logger.LogInformation($"{id}: Saving original image");
                        SnapshotManager.SaveOriginalImage(_logger, camera, snapshot);
                    }

                    if (validPredictions.Count > 0)
                    {
                        // Process and save the snapshot
                        ProcessedImage processedImage = SnapshotManager.DressImage(camera, snapshot, predictions, validPredictions, _logger);

                        // Send Notifications                  
                        Notification notification = new()
                        {
                            ProcessedImage = processedImage,
                            ValidPredictions = validPredictions
                        };

                        await SendNotifications(camera, notification);

                        // Inform eventual web users about this new Snapshot, for the "realtime" option thru Web
                        await _hubContext.Clients.All.SendAsync("ReceiveSnapshot", camera.Name, processedImage.FileName);
                        _logger.LogInformation($"{id}: Valid object found in snapshot {snapshotCount} of {Config.MaxSnapshots} at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");

                        // Extend the delay until the next motion detection will be run if a delay after success is specified
                        int successDelay = camera.GetDelayAfterSuccess();
                        AddCameraDelay(id, successDelay);
                        return;
                    }
                    else if (predictions.Any())
                    {
                        // We got predictions back from the AI, but nothing that should trigger an alert
                        _logger.LogInformation($"{id}: No valid objects at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                    }
                    else
                    {
                        // We didn't get any predictions whatsoever from the AI
                        _logger.LogInformation($"{id}: Nothing detected by the AI at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");

                        StringBuilder nothingFoundOutput = new StringBuilder($"{id}: No objects ");
                        if (camera.Types != null && camera.Types.Any())
                        {
                            nothingFoundOutput.Append($"in the specified list ({string.Join(", ", camera.Types)}) ");
                        }
                        nothingFoundOutput.Append($"were detected by the AI exceeding the confidence level ({camera.Threshold}%) and/or minimum size ({minSizeX}x{minSizeY} and/or maximum size ({maxSizeX},{maxSizeY}))");

                        _logger.LogDebug(nothingFoundOutput.ToString());
                    }

                    _logger.LogInformation($"{id}: Finished ({overallStopwatch.ElapsedMilliseconds}ms).");
                }

                // Add the delay (if any)
                int delay = camera.GetDelay();
                AddCameraDelay(id, delay);
            }
            finally
            {
                // Ensure the camera is unflagged as running
                lock (_runningCameraChecks)
                {
                    _logger.LogDebug($"{id}: Removing running camera block.");
                    _runningCameraChecks.Remove(id, out _);
                }
            }
        }
        
        [HttpPost]
        [Route("{id}")]
        public void Post(string id, [FromBody]CameraOptionsDto options)
        {
            if (options.HasChanged(x=> x.Enabled))
            {
                _enabledCameras.AddOrUpdate(id, options.Enabled, (key, oldValue) => options.Enabled);
            }
        }

        /// <summary>
        /// Checks to ensure the prediction falls within the boundaries to be considered a valid prediction.
        /// </summary>
        /// <param name="id">The ID of the camera.</param>
        /// <param name="camera">The camera object.</param>
        /// <param name="overallStopwatch">The current stopwatch for logging.</param>
        /// <param name="prediction">The prediction to validate.</param>
        /// <returns>True if the prediction is valid.</returns>
        private bool ShouldIncludePrediction(string id, Camera camera, Stopwatch overallStopwatch, AIPrediction prediction)
        {
            // Check if the prediction falls within the exclusion zones
            if (camera.Exclusions != null && camera.Exclusions.Count() > 0)
            {
                Rectangle boundary = new(prediction.MinX, prediction.MinY, prediction.SizeX, prediction.SizeY);
                foreach (Zone exclusion in camera.Exclusions)
                {
                    int startX = Math.Min(exclusion.Start.X, exclusion.End.X);
                    int startY = Math.Min(exclusion.Start.Y, exclusion.End.Y);
                    int endX = Math.Max(exclusion.Start.X, exclusion.End.X);
                    int endY = Math.Max(exclusion.Start.Y, exclusion.End.Y);
                    Rectangle exclusionZoneBoundary = new(startX, startY, endX - startX, endY - startY);
                    bool exclude = exclusion.Mode == OverlapMode.Contains ? exclusionZoneBoundary.Contains(boundary) : exclusionZoneBoundary.IntersectsWith(boundary);
                    if (exclude)
                    {
                        // The prediction boundary is contained within or intersects and exclusion zone, so ignore it    ;
                        _logger.LogDebug($"{id}: Ignored matching '{prediction.Label}' ([{prediction.MinX},{prediction.MinY}],[{prediction.MaxX},{prediction.MaxY}]) as it fell within the exclusion zone ([{exclusion.Start.X},{exclusion.Start.Y}],[{exclusion.End.X},{exclusion.End.Y}]) with exclusion mode '{exclusion.Mode}' at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a delay for the specified camera.
        /// </summary>
        /// <param name="id">The ID of the camera to add a delay for.</param>
        /// <param name="delay">The delay to add.</param>
        private void AddCameraDelay(string id, int delay)
        {
            if (delay == 0)
            {
                return;
            }

            lock (_delayedCameraChecks)
            {
                DateTime ignoreUntil = DateTime.UtcNow.AddMilliseconds(delay);
                _delayedCameraChecks.AddOrUpdate(id, ignoreUntil, (key, oldValue) => ignoreUntil);
                _logger.LogDebug($"{id}: Added delay of {delay} until the next request will be processed.");
            }
        }

        /// <summary>
        /// Fires off an Async process to clean up any old records.
        /// </summary>
        private void CleanupOldImages()
        {
            if (!Directory.Exists(Constants.DIRECTORY_CAPTURES))
            {
                return;
            }

            if (Config.DaysToKeepCaptures > 0 && !_cleanupOldImagesRunning)
            {
                _logger.LogInformation($"Captures Clean Up: Cleaning up images older than {Config.DaysToKeepCaptures} day(s).");
                Task.Run(() =>
                {
                    try
                    {
                        lock (_cleanUpOldImagesLock)
                        {
                            _cleanupOldImagesRunning = true;

                            DirectoryInfo directory = new(Constants.DIRECTORY_CAPTURES);
                            IEnumerable<FileInfo> files = directory.GetFiles("*", new EnumerationOptions() { RecurseSubdirectories = true });
                            foreach (FileInfo file in files)
                            {
                                double age = (DateTime.Now - file.CreationTime).TotalDays;
                                if (age > Config.DaysToKeepCaptures)
                                {
                                    _logger.LogInformation($"Captures Clean Up: {file.FullName} is {age} day(s) old and will be deleted.");
                                    System.IO.File.Delete(file.FullName);
                                    _logger.LogInformation($"Captures Clean Up: {file.FullName} deleted.");
                                }
                            }
                            _cleanupOldImagesRunning = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Captures Clean Up Failed");
                    }
                });
            }
        }
        private bool _cleanupOldImagesRunning;
        private object _cleanUpOldImagesLock = new();

        /// <summary>
        /// Handles any required preprocessing of the captured image.
        /// </summary>
        /// <param name="camera">The camera that the snapshot is from.</param>
        /// <param name="snapshot">The image data.</param>
        /// <returns>A byte array of the image.</returns>
        private byte[] PreProcessSnapshot(Camera camera, byte[] snapshot)
        {
            if (camera.Rotate != 0)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                // Load the bitmap & rotate the image
                SKBitmap bitmap = SKBitmap.Decode(snapshot);

                _logger.LogInformation($"{camera.Name}: Rotating image {camera.Rotate} degrees.");
                bitmap = Rotate(bitmap, camera.Rotate);

                using (SKPixmap pixmap = bitmap.PeekPixels())
                using (SKData data = pixmap.Encode(SKEncodedImageFormat.Jpeg, 100))
                {
                    _logger.LogInformation($"{camera.Name}: Image preprocessing complete ({stopwatch.ElapsedMilliseconds}ms).");
                    return data.ToArray();
                }
            }
            else
            {
                return snapshot;
            }
        }

        /// <summary>
        /// Rotates the image to the specified angle.
        /// </summary>
        /// <param name="bitmap">The bitmap to rotate.</param>
        /// <param name="angle">The angle to rotate to.</param>
        /// <returns>The rotated bitmap.</returns>
        private SKBitmap Rotate(SKBitmap bitmap, double angle)
        {
            double radians = Math.PI * angle / 180;
            float sine = (float)Math.Abs(Math.Sin(radians));
            float cosine = (float)Math.Abs(Math.Cos(radians));
            int originalWidth = bitmap.Width;
            int originalHeight = bitmap.Height;
            int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            SKBitmap rotatedBitmap = new(rotatedWidth, rotatedHeight);
            using (SKCanvas canvas = new(rotatedBitmap))
            {
                canvas.Clear();
                canvas.Translate(rotatedWidth / 2, rotatedHeight / 2);
                canvas.RotateDegrees((float)angle);
                canvas.Translate(-originalWidth / 2, -originalHeight / 2);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }

            return rotatedBitmap;
        }


        /// <summary>
        /// Sends notifications, if there is any configured
        /// </summary>
        /// <param name="camera">The camera responsible for this snapshot.</param>
        /// <param name="notification">The notification data to process.</param>
        private async Task SendNotifications(Camera camera, Notification notification)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            IEnumerable<string> labels = notification.ValidPredictions.Select(x => x.Label).Distinct().ToList();

            IEnumerable<INotifier> notifiers = Config.Notifiers
                .Where(x =>
                    (x.Cameras == null || !x.Cameras.Any() || x.Cameras.Any(c => c.Equals(camera.Name, StringComparison.OrdinalIgnoreCase))) &&
                    (x.Types == null || !x.Types.Any() || x.Types.Any(t => labels.Contains(t, StringComparer.OrdinalIgnoreCase)))
                ).ToList();

            List<Task> tasks = new();
            foreach (INotifier notifier in notifiers)
            {
                tasks.Add(notifier.SendAsync(camera, notification, _logger));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();
            _logger.LogInformation($"{camera.Name}: Notifications sent ({stopwatch.ElapsedMilliseconds}ms).");
        }

        /// <summary>
        /// Gets an image snapshot (in memory) from Surveillation Station.
        /// </summary>
        /// <param name="cameraName">The name of the camera to get the snapshot for.</param>
        /// <returns>A byte array for the image, or null on failure.</returns>
        private async Task<byte[]> GetSnapshot(string cameraName)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            byte[] imageBytes = await _synologyService.TakeSnapshotAsync(cameraName);
            stopwatch.Stop();

            if (imageBytes == null)
            {
                _logger.LogError($"{cameraName}: Failed to get snapshot.");
            }
            else
            {
                _logger.LogInformation($"{cameraName}: Snapshot received in {stopwatch.ElapsedMilliseconds}ms.");
            }
            return imageBytes;
        }

        /// <summary>
        /// Passes the provided image to the AI and gets the predictions back.
        /// </summary>
        /// <param name="camera">The camera that the image is from.</param>
        /// <param name="imageBytes">The in-memory image for processing.</param>
        /// <returns>A list of predictions, or null on failure.</returns>
        private async Task<IEnumerable<AIPrediction>> GetAIPredications(Camera camera, byte[] imageBytes)
        {
            IEnumerable<AIPrediction> predictions = await _aiService.ProcessAsync(camera, imageBytes);
            if (predictions == null)
            {
                _logger.LogError($"{camera}: Failed to get get predictions.");
                return null;
            }

            foreach (AIPrediction prediction in predictions)
            {
                _logger.LogInformation($"AI Detected '{camera}': {prediction.Label} ({prediction.Confidence}%) [Size: {prediction.SizeX}x{prediction.SizeY}] [Start: {prediction.MinX},{prediction.MinY} | End: {prediction.MaxX},{prediction.MaxY}]");
            }

            return predictions;
        }
    }
}
