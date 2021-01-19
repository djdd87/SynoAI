using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Webhook
{
    /// <summary>
    /// Calls a third party API.
    /// </summary>
    public class Webhook : INotifier
    {
        /// <summary>
        /// The URL to send the request to.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The HTTP method (POST/PUT/etc).
        /// </summary>
        public string Method { get; set; } = "POST";
        /// <summary>
        /// The field name when posting the image.
        /// </summary>
        public string Field { get; set; } = "image";

        /// <summary>
        /// Sends a notification to the Webhook.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="image">The processed image.</param>
        /// <param name="foundTypes">The list of types that were found.</param>
        /// <param name="logger">A logger.</param>
        public async Task Send(Camera camera, string filePath, IEnumerable<string> foundTypes, ILogger logger)
        {
            using (logger.BeginScope($"Webhook '{Url}'"))
            {
                logger.LogInformation("Calling Webhook");
                using (HttpClient client = new HttpClient())
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        MultipartFormDataContent data = new MultipartFormDataContent
                        {
                            { new StreamContent(fileStream), Field, Path.GetFileName(filePath) }
                        };

                        HttpResponseMessage message;
                        switch (Method)
                        {
                            case "DELETE":
                                logger.LogInformation("Calling DELETE.");
                                message = await client.DeleteAsync(Url);
                                break;
                            case "GET":
                                logger.LogInformation("Calling GET.");
                                message = await client.GetAsync(Url);
                                break;
                            case "PATCH":
                                logger.LogInformation("PATCHINGing file.");
                                message = await client.PatchAsync(Url, data);
                                break;
                            case "POST":
                                logger.LogInformation("POSTing file.");
                                message = await client.PostAsync(Url, data);
                                break;
                            case "PUT":
                                logger.LogInformation("PUTing file.");
                                message = await client.PutAsync(Url, data);
                                break;
                            default:
                                logger.LogError($"The Webhook method type '{Method}' is not supported.");
                                return;
                        }

                        if (message.IsSuccessStatusCode)
                        {
                            logger.LogInformation($"Success.");
                        }
                        else
                        {
                            logger.LogWarning($"The Webhook response with HTTP status code '{message.StatusCode}'.");
                        }
                    }
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
