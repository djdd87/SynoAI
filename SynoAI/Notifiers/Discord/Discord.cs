using SynoAI.App;
using SynoAI.Models;

namespace SynoAI.Notifiers.Discord
{
    internal class Discord : NotifierBase
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
            //var message = GetMessage(camera, notification.FoundTypes);
            var message = GetMessage(camera, notification.FoundTypes, new List<AIPrediction>());
            message = message.Replace("\n", "\\n");

            formData.Add(new StringContent($"{{\"content\":\"{message}\"}}"), "payload_json");
            formData.Add(new StreamContent(processedImage.GetReadonlyStream()), "file", processedImage.FileName);

            HttpResponseMessage responseMessage = await Shared.HttpClient.PostAsync(Url, formData);
            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation("{CameraName}: Discord: Notification sent successfully", camera.Name);
            }
            else
            {
                string error = await responseMessage.Content.ReadAsStringAsync();
                logger.LogError("{cameraName}: Discord: The end point responded with HTTP status code '{responseMessageStatusCode}' and error '{error}'.",
                    camera.Name,
                    responseMessage.StatusCode,
                    error);
            }
        }
    }
}
