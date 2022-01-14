using Microsoft.Extensions.Logging;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Pushover
{
    public class Pushover : NotifierBase
    {
        /// <summary>
        /// The API Key for the application for sending the notification.
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// The User Key for the user for sending the notification.
        /// </summary>
        public string UserKey { get; set; }

        public override Task SendAsync(Camera camera, ProcessedImage processedImage, IEnumerable<string> foundTypes, ILogger logger)
        {
            // TODO - https://pushover.net/api
            throw new NotImplementedException();
        }
    }
}
