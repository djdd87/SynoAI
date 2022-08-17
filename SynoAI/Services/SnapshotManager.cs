using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SynoAI.Models;
using SynoAI.Extensions;

namespace SynoAI.Services
{

    public class SnapshotManager
    {
 
        /// <summary>
        /// Dresses the source image by adding the boundary boxes and saves the file locally.
        /// </summary>
        /// <param name="camera">The camera the image came from.</param>
        /// <param name="snapshot">The image data.</param>
        /// <param name="predictions">The list of predictions with the right size (but may or may not be the types configured as interest for this camera).</param>
         /// <param name="validPredictions">The list of predictions with the right size and matching the type of objects of interest for this camera.</param>
        public static ProcessedImage DressImage(Camera camera, byte[] snapshot, IEnumerable<AIPrediction> predictions, IEnumerable<AIPrediction> validPredictions, ILogger logger) 
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Load the bitmap 
            SKBitmap image = SKBitmap.Decode(snapshot);

            // Draw the exclusion zones if enabled
            if (Config.DrawExclusions && camera.Exclusions != null)
            {
                logger.LogInformation($"{camera.Name}: Drawing exclusion zones.");
                
                using (SKCanvas canvas = new SKCanvas(image))
                {
                    // Draw the zone
                    foreach (Zone zone in camera.Exclusions)
                    {
                        SKRect rectangle = SKRect.Create(zone.Start.X, zone.Start.Y, zone.End.X - zone.Start.X, zone.End.Y - zone.Start.Y);
                        canvas.DrawRect(rectangle, new SKPaint 
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = GetColour(Config.ExclusionBoxColor),
                            StrokeWidth = Config.StrokeWidth
                        });
                    }
                }
            }

            // Don't process the drawing if the drawing mode is off
            if (Config.DrawMode == DrawMode.Off)
            {
                logger.LogInformation($"{camera.Name}: Draw mode is Off. Skipping image boundaries.");
            }
            else 
            {
                // Draw the predictions
                logger.LogInformation($"{camera.Name}: Dressing image with boundaries.");
                using (SKCanvas canvas = new SKCanvas(image))
                {
                    int counter = 1; //used for assigning a reference number on each prediction if AlternativeLabelling is true

                    foreach (AIPrediction prediction in Config.DrawMode == DrawMode.All ? predictions : validPredictions)
                    {
                        // Draw the box
                        SKRect rectangle = SKRect.Create(prediction.MinX, prediction.MinY, prediction.SizeX, prediction.SizeY);
                        canvas.DrawRect(rectangle, new SKPaint 
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = GetColour(Config.BoxColor),
                            StrokeWidth = Config.StrokeWidth
                        });
                            
                        // Label creation, either classic label or alternative labelling (and only if there is more than one object)
                        string label = String.Empty;
                        if (Config.AlternativeLabelling && Config.DrawMode == DrawMode.Matches) 
                        {
                            // On alternatie labelling, just place a reference number and only if there is more than one object
                            if (validPredictions.Count() > 1) 
                            {
                                label = counter.ToString();
                                counter++;
                            }
                        }
                        else
                        {
                            decimal confidence = Math.Round(prediction.Confidence, 0, MidpointRounding.AwayFromZero);
                            label = $"{prediction.Label.FirstCharToUpper()} {confidence}%";
                        }

                        // Label positioning
                        int x = prediction.MinX + Config.TextOffsetX;
                        int y = prediction.MinY + Config.FontSize + Config.TextOffsetY; // FontSize is added as text is drawn above the bottom co-ordinate

                        // Consider below box placement
                        if (Config.LabelBelowBox) 
                        {
                            y += prediction.SizeY;
                        }

                        // Draw background box for the text if required
                        SKTypeface typeface = SKTypeface.FromFamilyName(Config.Font);

                        SKPaint paint = new SKPaint
                        {
                            FilterQuality = SKFilterQuality.High,
                            IsAntialias = true,
                            Color = GetColour(Config.FontColor),
                            TextSize = Config.FontSize,
                            Typeface = typeface
                        };

                        string textBoxColor = Config.TextBoxColor;
                        if (!string.IsNullOrWhiteSpace(textBoxColor) && !textBoxColor.Equals(SKColors.Transparent.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            float textWidth = paint.MeasureText(label);
                            float textBoxWidth = textWidth + (Config.TextOffsetX * 2);
                            float textBoxHeight = Config.FontSize + (Config.TextOffsetY * 2);

                            float textBoxX = prediction.MinX + Config.StrokeWidth;
                            float textBoxY = prediction.MinY + Config.TextOffsetY;
                            if (Config.LabelBelowBox)
                            {
                                textBoxY += prediction.SizeY;
                            }

                            SKRect textRectangle = SKRect.Create(textBoxX, textBoxY, textBoxWidth, textBoxHeight);
                            canvas.DrawRect(textRectangle, new SKPaint
                            {
                                Style = SKPaintStyle.StrokeAndFill,
                                Color = GetColour(textBoxColor),
                                StrokeWidth = Config.StrokeWidth
                            });
                        }

                        // Draw the text
                        SKFont font = new SKFont(typeface, Config.FontSize);
                        canvas.DrawText(label, x, y, paint);   
                    }
                }
            }

            stopwatch.Stop();
            logger.LogInformation($"{camera.Name}: Finished dressing image boundaries ({stopwatch.ElapsedMilliseconds}ms).");

            // Save the image, including the amount of valid predictions as suffix.
            String filePath = SaveImage(logger,camera, image, validPredictions.Count().ToString());
            return new ProcessedImage(filePath);
        }


        /// <summary>
        /// Saves the original unprocessed image from the provided byte array to the camera's capture directory.
        /// </summary>
        /// <param name="camera">The camera to save the image for.</param>
        /// <param name="snapshot">The image to save.</param>
        public static string SaveOriginalImage(ILogger logger, Camera camera, byte[] snapshot)
        {
            SKBitmap image = SKBitmap.Decode(new MemoryStream(snapshot));
            return SaveImage(logger, camera, image, "Original");
        }


        /// <summary>
        /// Saves the image to the camera's capture directory.
        /// </summary>
        /// <param name="camera">The camera to save the image for.</param>
        /// <param name="image">The image to save.</param>
        private static string SaveImage(ILogger logger, Camera camera, SKBitmap image, string suffix = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string directory = Constants.DIRECTORY_CAPTURES;
            directory = Path.Combine(directory, camera.Name);

            if (!Directory.Exists(directory))
            {
                logger.LogInformation($"{camera}: Creating directory '{directory}'.");
                Directory.CreateDirectory(directory);
            }

            //euquiq, ALTERNATIVE FILE NAMING: Camera name is already used in the containing folder name
            //Also, a different separator used for suffix, which in turn holds detection data (number of valid objects)
            //Which is used for graphs.
            
            string fileName = String.Empty;

            if (Config.AlternativeLabelling) {
                fileName = $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";
                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    fileName += "-" + suffix;
                }
                fileName += ".jpg";
            } 
            else 
            {
                //Standard file naming
                fileName = $"{camera.Name}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_FFF}";
                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    fileName += "_" + suffix;
                }
                fileName += ".jpeg";
            }

            string filePath = Path.Combine(directory, fileName);
            logger.LogInformation($"{camera}: Saving image to '{filePath}'.");

            using (FileStream saveStream = new FileStream(filePath, FileMode.CreateNew))
            {
                bool saved = image.Encode(saveStream, SKEncodedImageFormat.Jpeg, 100);
                stopwatch.Stop();

                if (saved)
                {    
                    logger.LogInformation($"{camera}: Image saved to '{filePath}' ({stopwatch.ElapsedMilliseconds}ms).");
                }
                else
                {
                    logger.LogInformation($"{camera}: Failed to save image to '{filePath}' ({stopwatch.ElapsedMilliseconds}ms).");
                }
            }          
            return filePath;
        }


        /// <summary>
        /// Parses the provided colour name into an SKColor.
        /// </summary>
        /// <param name="colour">The string to parse.</param>
        private static SKColor GetColour(string hex)
        {
            if (!SKColor.TryParse(hex, out SKColor colour))
            {
                return SKColors.Red;
            }
            return colour;  
        }
    }
}