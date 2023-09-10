﻿using SynoAI.App;
using SynoAI.Models;
using System.Net.Http.Headers;

namespace SynoAI.Notifiers.Pushover
{
    internal class Pushover : NotifierBase
    {
        private readonly string URI_MESSAGE = "https://api.pushover.net/1/messages.json";

        /// <summary>
        /// The API Key for the application for sending the notification.
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// The User Key for the user for sending the notification.
        /// </summary>
        public string UserKey { get; set; }
        /// <summary>
        /// The devices to send to.
        /// </summary>
        public IEnumerable<string> Devices { get; set; }
        /// <summary>
        /// The priority of the message.
        /// </summary>
        public PushoverPriority Priority { get; set; }
        /// <summary>
        /// How often (in seconds) the Pushover servers will send the same notification to the user (minimum 30 seconds).
        /// </summary>
        public int Retry { get; set; }
        /// <summary>
        /// How many seconds your notification will continue to be retried for (every retry seconds) (maximum 10800 seconds).
        /// </summary>
        public int Expire { get; set; }        
        /// <summary>
        /// The pushover sound to use.
        /// </summary>
        public string Sound { get; set; }
        

        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                logger.LogError($"{nameof(ApiKey)} must be specified.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(UserKey))
            {
                logger.LogError($"{nameof(UserKey)} must be specified.");
                return;
            }

            // Build the form message
            logger.LogInformation("{cameraName}: Pushover: Building message",
                camera.Name);

            //string message = GetMessage(camera, notification.FoundTypes, new List<AIPrediction>());
            string message = GetMessage(camera, notification.FoundTypes, notification.ValidPredictions.ToList());
            string device = Devices == null || !Devices.Any() ? String.Empty : string.Join(',', Devices);
            string title = $"{camera.Name}: Movement Detected";

            MultipartFormDataContent form = new()
            {
                { new StringContent(device), "\"device\"" },
                { new StringContent(message), "\"message\"" },
                { new StringContent(((int)Priority).ToString()), "\"priority\"" },
                { new StringContent(Retry.ToString()), "\"retry\"" },
                { new StringContent(Expire.ToString()), "\"expire\"" },
                { new StringContent(Sound ?? String.Empty), "\"sound\"" },
                { new StringContent(ApiKey), "\"token\"" },
                { new StringContent(UserKey), "\"user\"" },
                { new StringContent(title), "\"title\"" }
            };

            // Send the message
            using (FileStream imageStream = notification.ProcessedImage.GetReadonlyStream())
            using (StreamContent imageContent = new(imageStream))
            {
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                form.Add(imageContent, "attachment", "image.png");

                // Remove content type that is not in the docs
                foreach (var param in form)
                {
                    param.Headers.ContentType = null;
                }

                logger.LogInformation("{cameraName}: Pushover: Sending message",
                    camera.Name);
                HttpResponseMessage responseMessage = await Shared.HttpClient.PostAsync(URI_MESSAGE, form);
                if (responseMessage.IsSuccessStatusCode)
                {
                    logger.LogInformation("{cameraName}: Pushover: Notification sent successfully",
                        camera.Name);
                }
                else
                {
                    string error = await responseMessage.Content.ReadAsStringAsync();
                    logger.LogError("{cameraName}: Pushover: The end point responded with HTTP status code '{responseMessageStatusCode}' and error '{error}'.",
                        camera.Name,
                        responseMessage.StatusCode,
                        error);
                }
            }
        }
    }
}
