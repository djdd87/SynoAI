using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SynoAI.Models;

namespace SynoAI.Services
{
    /// <summary>
    /// A thread safe object for sharing file access between multiple notifications.
    /// </summary>
    public class SnapshotManager : ISnapshotManager
    {
        /// <summary>
        /// The bytes of the received snapshot.
        /// </summary>
        private readonly byte[] _snapshot;

        /// <summary>
        /// All the predictions for the AI; used when the draw mode is "All".
        /// </summary>
        private readonly IEnumerable<AIPrediction> _predictions;
        /// <summary>
        /// The valid predictions according to the camera configuration; used when the draw mode is "Match".
        /// </summary>
        private readonly IEnumerable<AIPrediction> _validPredictions;

        private ProcessedImage _processedImage;
        private object _processLock = new object();

        private readonly ILogger _logger;

        public SnapshotManager(byte[] snapshot, IEnumerable<AIPrediction> predictions, IEnumerable<AIPrediction> validPredictions, ILogger logger)
        {
            _snapshot = snapshot;
            _predictions = predictions;
            _validPredictions = validPredictions;
            _logger = logger;
        }

        /// <summary>
        /// Thread-safely processes the image by drawing image boundaries.
        /// </summary>
        /// <param name="camera">The camera the image came from.</param>
        public ProcessedImage GetImage(Camera camera)
        {
            if (_processedImage == null)
            {
                lock (_processLock)
                {
                    if (_processedImage == null)
                    {
                        // Save the image
                        string filePath;
                        using (SKBitmap image = ProcessImage(camera))
                        {
                            filePath = SaveImage(camera, image);
                        }

                        // Create the helper object
                        _processedImage = new ProcessedImage(filePath);
                    }
                }
            }
            return _processedImage;
        }

        /// <summary>
        /// Processes the source image by adding the boundary boxes and saves the file locally.
        /// </summary>
        /// <param name="camera">The camera the image came from.</param>
        /// <param name="imageBytes">The image data.</param>
        /// <param name="predictions">The list of predictions to add to the image.</param>
        private SKBitmap ProcessImage(Camera camera)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            _logger.LogInformation($"{camera.Name}: Processing image boundaries.");

            // Load the bitmap
            SKBitmap image = SKBitmap.Decode(new MemoryStream(_snapshot));
            if (Config.DrawMode == DrawMode.Off)
            {
                _logger.LogInformation($"{camera.Name}: Draw mode is Off. Skipping image boundaries.");
                return image;
            }

            // Draw the predictions
            using (SKCanvas canvas = new SKCanvas(image))
            {
                foreach (AIPrediction prediction in Config.DrawMode == DrawMode.All ? _predictions : _validPredictions)
                {
                    // Write out anything detected that was above the minimum size
                    if (prediction.SizeX >= Config.AIMinSizeX && prediction.SizeY >= Config.AIMinSizeY)
                    {
                        decimal confidence = Math.Round(prediction.Confidence, 0, MidpointRounding.AwayFromZero);
                        string label = $"{prediction.Label} ({confidence}%)";

                        // Draw the box
                        SKRect rectangle = SKRect.Create(prediction.MinX, prediction.MinY, prediction.SizeX, prediction.SizeY);
                        canvas.DrawRect(rectangle, new SKPaint 
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = GetColour(Config.BoxColor)
                        });
                        
                        int x = prediction.MinX + Config.TextOffsetX;
                        int y = prediction.MinY + Config.FontSize + Config.TextOffsetY;

                        // Draw the text
                        SKFont font = new SKFont(SKTypeface.FromFamilyName(Config.Font), Config.FontSize);
                        canvas.DrawText(label, x, y, font, new SKPaint 
                        {
                            Color = GetColour(Config.FontColor)
                        });
                    }
                }
            }

            stopwatch.Stop();
            _logger.LogInformation($"{camera.Name}: Finished processing image boundaries ({stopwatch.ElapsedMilliseconds}ms).");

            return image;
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

            string fileName = $"{camera.Name}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_FFF}.jpeg";
            string filePath = Path.Combine(directory, fileName);
            _logger.LogInformation($"{camera}: Saving image to '{filePath}'.");

            using (FileStream saveStream = new FileStream(filePath, FileMode.CreateNew))
            {
                bool saved = image.Encode(saveStream, SKEncodedImageFormat.Jpeg, 100);
                stopwatch.Stop();

                if (saved)
                {    
                    _logger.LogInformation($"{camera}: Imaged saved to '{filePath}' ({stopwatch.ElapsedMilliseconds}ms).");
                }
                else
                {
                    _logger.LogInformation($"{camera}: Failed to save image to '{filePath}' ({stopwatch.ElapsedMilliseconds}ms).");
                }
            }
            
            return filePath;
        }

        /// <summary>
        /// Parses the provided colour name into an SKColor.
        /// </summary>
        /// <param name="colour">The string to parse.</param>
        private SKColor GetColour(string hex)
        {
            if (!SKColor.TryParse(hex, out SKColor colour))
            {
                return SKColors.Red;
            }
            return colour;  
        }
    }
}