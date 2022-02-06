using SynoAI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SynoAI.Models
{
    public class Notification
    {
        /// <summary>
        /// Object for fetching the processed image
        /// </summary>
        public ProcessedImage ProcessedImage { get; set; }
        /// <summary>
        /// The list of valid predictions.
        /// </summary>
        public IEnumerable<AIPrediction> ValidPredictions { get; set; }

        /// <summary>
        /// The list of types that were found.
        /// </summary>
        public IEnumerable<string> FoundTypes
        {
            get
            {
                return GetLabels();
            }
        }

        /// <summary>
        /// Gets the labels from the predictions to use in the notifications.
        /// </summary>
        /// <param name="validPredictions">The predictions to process.</param>
        /// <returns>A list of labels.</returns>
        private IEnumerable<string> GetLabels()
        {
            if (Config.AlternativeLabelling && Config.DrawMode == DrawMode.Matches)
            {
                List<String> labels = new List<String>();
                if (ValidPredictions.Count() == 1)
                {
                    // If there is only a single object, then don't add a correlating number and instead just
                    // write out the label.
                    decimal confidence = Math.Round(ValidPredictions.First().Confidence, 0, MidpointRounding.AwayFromZero);
                    labels.Add($"{ValidPredictions.First().Label.FirstCharToUpper()} {confidence}%");
                }
                else
                {
                    // Since there is more than one object detected, include correlating number
                    int counter = 1;
                    foreach (AIPrediction prediction in ValidPredictions)
                    {
                        decimal confidence = Math.Round(prediction.Confidence, 0, MidpointRounding.AwayFromZero);
                        labels.Add($"{counter}. {prediction.Label.FirstCharToUpper()} {confidence}%");
                        counter++;
                    }
                }

                return labels;
            }
            else
            {
                return ValidPredictions.Select(x => x.Label.FirstCharToUpper()).ToList();
            }
        }
    }
}
