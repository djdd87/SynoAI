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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SynoAI.Hubs;

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

        private static ConcurrentDictionary<string, DateTime> _lastCameraChecks = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

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

            // Fetch the camera
            Camera camera = Config.Cameras.FirstOrDefault(x => x.Name.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (camera == null)
            {
                _logger.LogError($"The camera with the name '{id}' was not found.");
            }
            else
            {
                // Create the stopwatches for reporting timings
                Stopwatch overallStopwatch = Stopwatch.StartNew();

                // Start loop for requesting snapshots until a valid prediction is found or MaxSnapshots is reached
                for (int snapshotCount = 1; snapshotCount <= Config.MaxSnapshots; snapshotCount++)
                {
                    // Take the snapshot from Surveillance Station
                    _logger.LogInformation($"Snapshot {snapshotCount} of {Config.MaxSnapshots} asked at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                    byte[] snapshot = await GetSnapshot(id);
                    _logger.LogInformation($"Snapshot {snapshotCount} of {Config.MaxSnapshots} received at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");

                    // See if the image needs to be rotated (or further processing in the future ?) before being analyzed by the AI
                    snapshot = PreProcessSnapshot(camera, snapshot);

                    // Use the AI to get the valid predictions and then get all the valid predictions, where the result from the AI is 
                    // in the list of types and where the size of the object is bigger than the defined value.
                    IEnumerable<AIPrediction> rawPredictions = await GetAIPredications(camera, snapshot);

                    _logger.LogInformation($"Snapshot {snapshotCount} of {Config.MaxSnapshots} contains {rawPredictions.Count()} objects at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                    
                    if (rawPredictions.Count() > 0)
                    {
                        IEnumerable<AIPrediction> predictions = rawPredictions.Where(x =>
                            x.SizeX >= camera.GetMinSizeX() && x.SizeY >= camera.GetMinSizeY() &&   // Is bigger than the minimum size
                            x.SizeX <= camera.GetMaxSizeX() && x.SizeY <= camera.GetMaxSizeY())     // Is smaller than the maximum size 
                            .ToList();

                         IEnumerable<AIPrediction> validPredictions = predictions.Where(x =>
                            camera.Types.Contains(x.Label, StringComparer.OrdinalIgnoreCase))       // Is a type we care about
                            .ToList();

                        if (validPredictions.Count() > 0)
                        {
                            // Save the original unprocessed image if required
                            if (Config.SaveOriginalSnapshot)
                            {
                                _logger.LogInformation($"{id}: Saving original image");
                                SnapshotManager.SaveOriginalImage(_logger, camera, snapshot);
                            }

                            // Generate text for notifications                  
                            List<String> labels = new List<String>();
                            if (Config.AlternativeLabelling && Config.DrawMode == DrawMode.Matches)
                            {
                                if (validPredictions.Count() == 1) 
                                {
                                    // If there is only a single object, then don't add a correlating number and instead just
                                    // write out the label.
                                    decimal confidence = Math.Round(validPredictions.First().Confidence, 0, MidpointRounding.AwayFromZero);
                                    labels.Add($"{validPredictions.First().Label.FirstCharToUpper()} {confidence}%");
                                }
                                else 
                                {
                                    //Since there is more than one object detected, include correlating number
                                    int counter = 1;
                                    foreach (AIPrediction prediction in validPredictions) 
                                    {
                                        decimal confidence = Math.Round(prediction.Confidence, 0, MidpointRounding.AwayFromZero);
                                        labels.Add($"{counter}. {prediction.Label.FirstCharToUpper()} {confidence}%");
                                        counter++;
                                    }
                                }
                            }
                            else
                            {
                                labels = validPredictions.Select(x => x.Label.FirstCharToUpper()).ToList();
                            }

                            // Process and save the snapshot
                            ProcessedImage processedImage = SnapshotManager.DressImage(camera, snapshot, predictions, validPredictions, _logger);

                            // Inform eventual web users about this new Snapshot, for the "realtime" option thru Web
                            await _hubContext.Clients.All.SendAsync("ReceiveSnapshot", camera.Name, processedImage.FileName);

                            // Send Notifications                  
                            await SendNotifications(camera, processedImage, labels);
                            _logger.LogInformation($"{id}: Valid object found in snapshot {snapshotCount} of {Config.MaxSnapshots} at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                            break;
                        }
                        else if (predictions.Count() > 0)
                        {
                            // We got predictions back from the AI, but nothing that should trigger an alert
                            _logger.LogInformation($"{id}: No valid objects at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                        }
                        else
                        {
                            // We didn't get any predictions whatsoever from the AI
                            _logger.LogInformation($"{id}: Nothing detected by the AI at EVENT TIME {overallStopwatch.ElapsedMilliseconds}ms.");
                        }
                    }
                }
                snapshotCount++;
                else if (predictions.Count() > 0)
                {
                    // We got predictions back from the AI, but nothing that should trigger an alert
                    _logger.LogInformation($"{id}: Nothing detected by the AI exceeding the defined confidence level and/or minimum size");
                    _logger.LogDebug($"{id}: No objects in the specified list ({string.Join(", ", camera.Types)}) were detected by the AI exceeding the confidence level ({camera.Threshold}%) and/or minimum size ({minX}x{minY})");
                }
                else
                {
                    // We didn't get any predictions whatsoever from the AI
                    _logger.LogInformation($"{id}: Nothing detected by the AI");
                }

                _logger.LogInformation($"{id}: Finished ({overallStopwatch.ElapsedMilliseconds}ms).");
            }
        }

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
                //SKBitmap bitmap = SKBitmap.Decode(new MemoryStream(snapshot));
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

            SKBitmap rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);
            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
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
        /// <param name="processedImage">The path information for the snapshot.</param>
        /// <param name="labels">The text metadata for each existing valid object.</param>
        private async Task SendNotifications(Camera camera, ProcessedImage processedImage, IList<String> labels)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            IEnumerable<INotifier> notifiers = Config.Notifiers.Where(x=> x.Cameras == null || x.Cameras.Count() == 0 ||
                x.Cameras.Any(c=> c.Equals(camera.Name, StringComparison.OrdinalIgnoreCase))).ToList();

            List<Task> tasks = new List<Task>();
            foreach (INotifier notifier in notifiers)
            {
                tasks.Add(notifier.SendAsync(camera, processedImage, labels, _logger));
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
            }
            else if (_logger.IsEnabled(LogLevel.Information))
            {
                foreach (AIPrediction prediction in predictions)
                {
                    _logger.LogInformation($"{camera}: {prediction.Label} ({prediction.Confidence}%) [Size: {prediction.SizeX}x{prediction.SizeY}] [Start: {prediction.MinX},{prediction.MinY} | End: {prediction.MaxX},{prediction.MaxY}]");
                }
            }
            return predictions;
        }
    }
}
