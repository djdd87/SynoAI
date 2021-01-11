using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Pushbullet
{
    /// <summary>
    /// Configuration for sending a Pushbullet notification.
    /// </summary>
    public class Pushbullet : INotifier
    {
        /// <summary>
        /// The API Key for sending the notification.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Sends a message and an image using the Pushbullet API.
        /// </summary>
        public void Send(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
