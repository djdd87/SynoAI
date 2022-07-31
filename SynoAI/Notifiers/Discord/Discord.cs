using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SynoAI.App;
using SynoAI.Models;

namespace SynoAI.Notifiers.Discord
{
    public class Discord : NotifierBase
    {
        /// <summary>
        /// Discord Webhook Url.
        /// </summary>
        public string Url { get; set; }

        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            var formData = new MultipartFormDataContent();
            ProcessedImage processedImage = notification.ProcessedImage;

            // Discord seems to require double escaped newlines
            var message = GetMessage(camera, notification.FoundTypes);
            message = message.Replace("\n", "\\n");

            formData.Add(new StringContent($"{{\"content\":\"{message}\"}}"), "payload_json");
            formData.Add(new StreamContent(processedImage.GetReadonlyStream()), "file", processedImage.FileName);

            HttpResponseMessage responseMessage = await Shared.HttpClient.PostAsync(Url, formData);
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation($"{camera.Name}: Discord: Notification sent successfully");
            }
            else
            {
                string error = await responseMessage.Content.ReadAsStringAsync();
                logger.LogError($"{camera.Name}: Discord: The end point responded with HTTP status code '{responseMessage.StatusCode}' and error '{error}'.");
            }
        }
    }
}
