using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SynoAI.AIs;
using SynoAI.Models;
using SynoAI.Notifiers;
using SynoAI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SynoAI.Controllers
{
    /// <summary>
    /// Controller triggered on a motion alert from synology, which will act as a bridge between the Synology camera and DeepStack AI.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ISynologyService _synologyService;
        private readonly ILogger<CameraController> _logger;

        private static ConcurrentDictionary<string, DateTime> _lastCameraChecks = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public CameraController(IAIService aiService, ISynologyService synologyService, ILogger<CameraController> logger)
        {
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
                return;
            }

            // Enforce a delay between checks
            if (!HasSufficientDelay(id))
            {
                return;
            }

            // Create the stopwatches for reporting timings
            Stopwatch overallStopwatch = Stopwatch.StartNew();

            // Take the snapshot from Surveillance Station
            byte[] imageBytes = await GetSnapshot(id);

            // Use the AI to get the valid predictions
            IEnumerable<AIPrediction> predictions = await GetAIPredications(camera, imageBytes);

            // Remove any predictions that aren't in the list of types our camera should be reporting
            if (predictions.Count() > 0)
            {
                // Process the image
                using (SKBitmap image = ProcessImage(camera, imageBytes, predictions))
                {
                    if (image == null)
                    {
                        // If we get a null image back, then we didn't find anything
                        _logger.LogInformation($"{id}: Nothing detected by the AI exceeding the defined confidence level");
                    }
                    else
                    {
                        // Save the image and send the notifications
                        string filePath = SaveImage(camera, image);
                        
                        // Limit the predictions to just those defined by the camera
                        predictions = predictions.Where(x => camera.Types.Contains(x.Label, StringComparer.OrdinalIgnoreCase)).ToList();
                        await SendNotifications(camera, filePath, predictions.Select(x=> x.Label).Distinct().ToList());
                    }
                }
            }
            else
            {
                _logger.LogInformation($"{id}: Nothing detected by the AI");
            }

            _logger.LogInformation($"{id}: Finished ({overallStopwatch.ElapsedMilliseconds}ms).");
        }

        private async Task SendNotifications(Camera camera, string filePath, IEnumerable<string> labels)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Task> tasks = new List<Task>();
            foreach (INotifier notifier in Config.Notifiers)
            {
                tasks.Add(notifier.Send(camera, filePath, labels, _logger));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation($"{camera.Name}: Notifications sent ({stopwatch.ElapsedMilliseconds}ms).");
        }

        /// <summary>
        /// Processes the source image by adding the boundary boxes and saves the file locally.
        /// </summary>
        /// <param name="camera">The camera the image came from.</param>
        /// <param name="imageBytes">The image data.</param>
        /// <param name="predictions">The list of predictions to add to the image.</param>
        private SKBitmap ProcessImage(Camera camera, byte[] imageBytes, IEnumerable<AIPrediction> predictions)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Get all the valid predictions, which are all the AI predictions where the result from the AI is 
            //  in the list of types and where the size of the object is bigger than the defined value.
            IEnumerable<AIPrediction> validPredictions = predictions.Where(x =>
                camera.Types.Contains(x.Label, StringComparer.OrdinalIgnoreCase) &&     // Is a type we care about
                x.SizeX >= Config.AIMinSizeX && x.SizeY >= Config.AIMinSizeY)           // Is bigger than the minimum size
                .ToList();

            if (validPredictions.Count() == 0)
            {
                // There's nothing to process
                return null;
            }

            _logger.LogInformation($"{camera.Name}: Processing image boundaries.");

            // Load the bitmap
            SKBitmap image = SKBitmap.Decode(new MemoryStream(imageBytes));
            if (Config.DrawMode == DrawMode.Off)
            {
                _logger.LogInformation($"{camera.Name}: Draw mode is Off. Skipping image boundaries.");
                return image;
            }

            // Draw the predictions
            using (SKCanvas canvas = new SKCanvas(image))
            {
                foreach (AIPrediction prediction in (Config.DrawMode == DrawMode.All ? predictions : validPredictions))
                {
                    // Write out anything detected that was above the minimum size
                    if (prediction.SizeX >= Config.AIMinSizeX && prediction.SizeY >= Config.AIMinSizeY)
                    {
                        decimal confidence = Math.Round(prediction.Confidence, 0, MidpointRounding.AwayFromZero);
                        string label = $"{prediction.Label} ({confidence}%)";

                        SKRect rectangle = SKRect.Create(prediction.MinX, prediction.MinY, prediction.SizeX, prediction.SizeY);
                        SKPaint paint = new SKPaint 
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = SKColors.Red
                        };

                        // draw fill
                        canvas.DrawRect(rectangle, paint);
                        
                        int x = prediction.MinX + Config.TextOffsetX;
                        int y = prediction.MinY + Config.FontSize + Config.TextOffsetY;

                        SKFont font = new SKFont(SKTypeface.FromFamilyName(Config.Font), Config.FontSize);
                        canvas.DrawText(label, x, y, font, paint);
                    }
                }
            }

            stopwatch.Stop();
            _logger.LogInformation($"{camera.Name}: Finished processing image boundaries ({stopwatch.ElapsedMilliseconds}ms).");

            return image;
        }

        /// <summary>
        /// Gets an image snapshot (in memory) from Surveillation Station.
        /// </summary>
        /// <param name="cameraName">The name of the camera to get the snapshot for.</param>
        /// <returns>A byte array for the image, or null on failure.</returns>
        private async Task<byte[]> GetSnapshot(string cameraName)
        {
            _logger.LogInformation($"{cameraName}: Motion detected, fetching snapshot.");

            Stopwatch stopwatch = Stopwatch.StartNew();

            byte[] imageBytes = await _synologyService.TakeSnapshotAsync(cameraName);
            if (imageBytes == null)
            {
                _logger.LogError($"{cameraName}: Failed to get snapshot.");
                return null;
            }
            else
            {
                stopwatch.Stop();
                _logger.LogInformation($"{cameraName}: Snapshot received ({stopwatch.ElapsedMilliseconds}ms).");
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
            _logger.LogInformation($"{camera}: Processing.");

            IEnumerable<AIPrediction> predictions = await _aiService.ProcessAsync(camera, imageBytes);
            if (predictions == null)
            {
                _logger.LogError($"{camera}: Failed to get get predictions.");
                return null;
            }
            else
            {
                foreach (AIPrediction prediction in predictions)
                {
                    _logger.LogInformation($"{camera}: {prediction.Label} ({prediction.Confidence}%)");
                }
            }

            return predictions;
        }

        /// <summary>
        /// Saves the image to the camera's capture directory.
        /// </summary>
        /// <param name="camera">The camera to save the image for.</param>
        /// <param name="image">The image to save.</param>
        private string SaveImage(Camera camera, SKBitmap image)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string directory = $"Captures";
            directory = Path.Combine(directory, camera.Name);

            if (!Directory.Exists(directory))
            {
                _logger.LogInformation($"{camera}: Creating directory '{directory}'.");
                Directory.CreateDirectory(directory);
            }

            string fileName = $"{camera.Name}_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss_FFF}.jpeg";
            string filePath = Path.Combine(directory, fileName);
            _logger.LogInformation($"{camera}: Saving image to '{filePath}'.");

            using (FileStream saveStream = new FileStream(filePath, FileMode.CreateNew))
            {
                bool saved = image.Encode(saveStream, SKEncodedImageFormat.Jpeg, 100);
                if (saved)
                {    
                    _logger.LogInformation($"{camera}: Imaged saved to '{filePath}' ({stopwatch.ElapsedMilliseconds}ms).");
                }
                else
                {
                    _logger.LogInformation($"{camera}: Failed to save image to '{filePath}' ({stopwatch.ElapsedMilliseconds}ms).");
                }
            }
            
            stopwatch.Stop();
            return filePath;
        }

        /// <summary>
        /// Ensures that the camera doesn't get called too often.
        /// </summary>
        /// <param name="id">The ID of the camera to check.</param>
        /// <returns>True if enough time has passed.</returns>
        private bool HasSufficientDelay(string id)
        {
            if (_lastCameraChecks.TryGetValue(id, out DateTime lastCheck))
            {
                TimeSpan timeSpan = DateTime.UtcNow - lastCheck;
                _logger.LogInformation($"{id}: Camera last checked {timeSpan.Milliseconds}ms ago");

                if (timeSpan.TotalMilliseconds < Config.Delay)
                {
                    _logger.LogInformation($"{id}: Ignoring request due to last check being under {Config.Delay}ms.");
                    return false;
                }

                if (!_lastCameraChecks.TryUpdate(id, DateTime.UtcNow, lastCheck))
                {
                    _logger.LogInformation($"{id}: Ignoring request due multiple concurrent calls.");
                    return false;
                }
            }
            else
            {
                if (!_lastCameraChecks.TryAdd(id, DateTime.UtcNow))
                {
                    _logger.LogInformation($"{id}: Ignoring request due multiple concurrent calls.");
                    return false;
                }
            }

            return true;
        }
    }
}
