using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using SynoAI.Services;

namespace SynoAI.Notifiers
{
    public abstract class NotifierBase : INotifier
    {
        public IEnumerable<string> Cameras { get; set; } 
        public IEnumerable<string> Types { get; set; } 
        
        public abstract Task SendAsync(Camera camera, ProcessedImage processedImage, IEnumerable<string> foundTypes, ILogger logger);

        protected string GetMessage(Camera camera, IEnumerable<string> foundTypes, string errorMessage = null)
        {
            string result ;
            if (Config.AlternativeLabelling && Config.DrawMode == DrawMode.Matches)
            {
                // Defaulting into generic label type
                String typeLabel = "object";

                if (camera.Types.Count() == 1) {
                    // Only one object type configured: use it instead of generic "object" label
                    typeLabel = camera.Types.First();
                }

                if (foundTypes.Count() > 1)
                {
                    // Several objects detected
                    result = $"{camera.Name}: {foundTypes.Count()} {typeLabel}s\n{String.Join("\n", foundTypes.Select(x => x).ToArray())}";
                } 
                else 
                {
                    // Just one object detected
                    result =  $"{camera.Name}: {foundTypes.First()}";    
                }      
            }
            else 
            {
                // Standard (old) labelling
                result =  $"Motion detected on {camera.Name}\n\nDetected {foundTypes.Count()} objects:\n{String.Join("\n", foundTypes.Select(x => x).ToArray())}";   
            }

            if (!string.IsNullOrWhiteSpace(errorMessage)){
                result += $"\nAn error occurred during the creation of the notification: {errorMessage}";
            }

            return result;
        }


        /// <summary>
        /// Fetches the response content and parses it as the specified type.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>A usable object.</returns>
        protected async Task<T> GetResponse<T>(HttpResponseMessage message)
        {
            string content = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}