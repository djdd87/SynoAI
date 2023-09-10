using Microsoft.AspNetCore.Components.Forms;
using SynoAI.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Net.Http;

namespace SynoAI.Notifiers.Telegram
{
    /// <summary>
    /// Calls a third party API.
    /// </summary>
    internal class Telegram : NotifierBase
    {
        /// <summary>
        /// The ID of the chat to send notifications to
        /// </summary>
        public string ChatID { get; set; }
        /// <summary>
        /// The token used to authenticate to Telegram
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Photo base URL
        /// </summary>
        public string PhotoBaseURL { get; set; }

        /// <summary>
        /// Sends a message and an image using the Telegram API.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="notification">The notification data to process.</param>
        /// <param name="logger">A logger.</param>
        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            using (logger.BeginScope($"Telegram '{ChatID}'"))
            {
                // Assign camera name to variable for logger placeholder
                string cameraName = camera.Name;
                ProcessedImage processedImage = notification.ProcessedImage;
                IEnumerable<string> foundTypes = notification.FoundTypes;

                try
                {
                    TelegramBotClient bot = new(Token);

                    //string message = GetMessage(camera, foundTypes);
                    string message = GetMessage(camera, foundTypes, new List<AIPrediction>());

                    if (string.IsNullOrWhiteSpace(PhotoBaseURL))
                    {
                        // The photo base URL hasn't been specified, which means we need to send the file ourselves
                        using FileStream fileStream = processedImage.GetReadonlyStream();
                        var inputFile = new InputFileStream(fileStream, processedImage.FileName);
                        await bot.SendPhotoAsync(chatId: ChatID, photo: inputFile, caption: message);
                        // TODO - Add a config to disable the sending of the image?
                    } 
                    else 
                    {
                        string photoUrl = $"{PhotoBaseURL}/{camera.Name}/{processedImage.FileName}";
                        //api requires a download of the file
                        using HttpClient httpClient = new();
                        using Stream photoStream = await httpClient.GetStreamAsync(photoUrl);
                        await bot.SendPhotoAsync(chatId: ChatID, photo: new InputFileStream(photoStream, processedImage.FileName), caption: message);
                    }

                    logger.LogInformation("{cameraName}: Telegram notification sent successfully", cameraName);
                } 
                catch (Exception ex)
                {
                    logger.LogError("{cameraName}: Error occurred sending telegram", cameraName);
                    logger.LogError(ex, "An exception occurred");
                }
            }
        }
    }
}
