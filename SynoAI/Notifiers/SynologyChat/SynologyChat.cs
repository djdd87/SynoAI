using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using SynoAI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.SynologyChat
{
    /// <summary>
    /// Calls a third party API.
    /// </summary>
    public class SynologyChat : NotifierBase
    {
        /// <summary>
        /// The URL to send the request to including the token.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Sends a notification to the Webhook.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="notification">The notification data to process.</param>
        /// <param name="logger">A logger.</param>
        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            logger.LogInformation($"{camera.Name}: SynologyChat: Processing");
            using (HttpClient client = new())
            {
                IEnumerable<string> foundTypes = notification.FoundTypes;
                string message = GetMessage(camera, foundTypes);

                var request = new
                {
                    text = message,
                    file_url = new Uri(new Uri(Config.Url), new Uri($"Image/{camera.Name}/{notification.ProcessedImage.FileName}", UriKind.Relative))
                };

                string requestJson = JsonConvert.SerializeObject(request);
                Dictionary<string, string> payload = new()
                {
                    { "payload", requestJson },
                };

                using (FormUrlEncodedContent content = new(payload))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    logger.LogInformation($"{camera.Name}: SynologyChat: POSTing message.");

                    HttpResponseMessage response = await client.PostAsync(Url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Check that it's actually successful, because Synology like to make things awkward
                        string responseString = await response.Content.ReadAsStringAsync();
                        SynologyChatResponse actualResponse = JsonConvert.DeserializeObject<SynologyChatResponse>(responseString);
                        if (actualResponse.Success)
                        {
                            logger.LogInformation($"{camera.Name}: SynologyChat: Success.");
                        }
                        else
                        {
                            logger.LogInformation($"{camera.Name}: SynologyChat: Failed with error '{actualResponse.Error.Code}': {actualResponse.Error.Errors}.");
                        }
                    }
                    else
                    {
                        logger.LogWarning($"{camera.Name}: SynologyChat: The end point responded with HTTP status code '{response.StatusCode}'.");
                    }
                }
            }
        }
    }
}
