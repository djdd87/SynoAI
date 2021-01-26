using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using SynoAI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Webhook
{
    /// <summary>
    /// Calls a third party API.
    /// </summary>
    public class Webhook : NotifierBase
    {
        /// <summary>
        /// The URL to send the request to.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The HTTP method (POST/PUT/etc).
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// The field name when posting the image.
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// Whether the image should be sent in POST/PUT/PATCH requests.
        /// </summary>
        public bool SendImage { get; set; }
        /// <summary>
        /// Whether the message should be sent in POST/PUT/PATCH requests.
        /// </summary>
        public bool SendTypes { get; set; }

        /// <summary>
        /// Sends a notification to the Webhook.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="snapshotManager">A thread safe object for fetching a readonly file stream.</param>
        /// <param name="foundTypes">The list of types that were found.</param>
        /// <param name="logger">A logger.</param>
        public override async Task Send(Camera camera, ISnapshotManager snapshotManager, IEnumerable<string> foundTypes, ILogger logger)
        {
            logger.LogInformation($"{camera.Name}: Webhook: Processing");
            using (HttpClient client = new HttpClient())
            {
                MultipartFormDataContent data = new MultipartFormDataContent();
                if (SendTypes)
                {
                    data.Add(JsonContent.Create(foundTypes));
                } 

                FileStream fileStream = null;
                switch (Method)
                {
                    case "PATCH":
                    case "POST":
                    case "PUT":
                        if (SendImage)
                        {
                            ProcessedImage processedImage = snapshotManager.GetImage(camera);
                            fileStream = processedImage.GetReadonlyStream();
                            data.Add(new StreamContent(fileStream), Field, processedImage.FileName);
                        }
                        break;                            
                }

                logger.LogInformation($"{camera.Name}: Webhook: Calling {Method}.");

                HttpResponseMessage message;
                switch (Method)
                {
                    case "DELETE":
                        message = await client.DeleteAsync(Url);
                        break;
                    case "GET":
                        message = await client.GetAsync(Url);
                        break;
                    case "PATCH":
                        message = await client.PatchAsync(Url, data);
                        break;
                    case "POST":
                        message = await client.PostAsync(Url, data);
                        break;
                    case "PUT":
                        message = await client.PutAsync(Url, data);
                        break;
                    default:
                        logger.LogError($"{camera.Name}: Webhook: The method type '{Method}' is not supported.");
                        return;
                }

                if (message.IsSuccessStatusCode)
                {
                    logger.LogInformation($"{camera.Name}: Webhook: Success.");
                }
                else
                {
                    logger.LogWarning($"{camera.Name}: Webhook: The end point responded with HTTP status code '{message.StatusCode}'.");
                }

                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Fetches the response content and parses it a DeepStack object.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>A usable object.</returns>
        private async Task<T> GetResponse<T>(HttpResponseMessage message)
        {
            string content = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
