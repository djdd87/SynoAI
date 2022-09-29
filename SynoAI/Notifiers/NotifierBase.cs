using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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
        

        public virtual Task InitializeAsync(ILogger logger) { return Task.CompletedTask; }

        public abstract Task SendAsync(Camera camera, Notification notification, ILogger logger);

        public virtual Task CleanupAsync(ILogger logger) { return Task.CompletedTask; }

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

        protected string GetImageUrl(Camera camera, Notification notification)
        {
            if (Config.SynoAIUrL == null)
            {
                return null;
            }

            UriBuilder builder = new UriBuilder(Config.SynoAIUrL);
            builder.Path += $"{camera.Name}/{notification.ProcessedImage.FileName}";

            return builder.Uri.ToString();
        }

        /// <summary>
        /// Generates a JSON representation of the notification.
        /// </summary>
        protected string GenerateJSON(Camera camera, Notification notification, bool sendImage)
        {
            dynamic jsonObject = new ExpandoObject();

            jsonObject.camera = camera.Name;
            jsonObject.foundTypes = notification.FoundTypes;
            jsonObject.predictions = notification.ValidPredictions;
            jsonObject.message = GetMessage(camera, notification.FoundTypes);

            if (sendImage)
            {
                jsonObject.image = ToBase64String(notification.ProcessedImage.GetReadonlyStream());
            }

            string imageUrl = GetImageUrl(camera, notification);
            if (imageUrl != null)
            {
                jsonObject.imageUrl = imageUrl;
            }

            return JsonConvert.SerializeObject(jsonObject);
        }

        /// <summary>
        /// Returns FileStream data as a base64-encoded string
        /// </summary>
        private string ToBase64String(FileStream fileStream)
        {
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, (int)fileStream.Length);

            return Convert.ToBase64String(buffer);
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