using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using SynoAI.Extensions;
using SynoAI.Models;
using SynoAI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;

namespace SynoAI.Notifiers.Telegram
{
    /// <summary>
    /// Calls a third party API.
    /// </summary>
    public class Telegram : NotifierBase
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
        /// Sends a message and an image using the Pushbullet API.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="snapshotManager">A thread safe object for fetching the processed image.</param>
        /// <param name="foundTypes">The list of types that were found.</param>
        /// <param name="logger">A logger.</param>
        public override async Task SendAsync(Camera camera, ISnapshotManager snapshotManager, IEnumerable<string> foundTypes, ILogger logger)
        {
            using (logger.BeginScope($"Telegram '{ChatID}'"))
            {
                // Assign camera name to variable for logger placeholder
                string cameraName = camera.Name;

                try
                {
                    var bot = new TelegramBotClient(Token);

                    ProcessedImage processedImage = snapshotManager.GetImage(camera);
                    var photoUrl = $"{PhotoBaseURL}/{camera.Name}/{processedImage.FileName}";

                    var message = $"Motion detected on {camera.Name}\n\nDetected {foundTypes.Count()} objects:\n{String.Join("/n", foundTypes.Select(x => x.FirstCharToUpper()).ToArray())}";


                    if(!string.IsNullOrEmpty(PhotoBaseURL)) {
                        await bot.SendPhotoAsync(ChatID, photoUrl, message);
                    } else {
                        await bot.SendTextMessageAsync(ChatID, message);
                    }


                    logger.LogInformation($"{cameraName}: Telegram notification sent successfully", cameraName);
                } 
                catch (Exception ex)
                {
                    logger.LogError($"{cameraName}: Error occurred sending telegram", cameraName);
                    logger.LogError(ex, "An exception occurred");
                }
            }
        }
    }
}
