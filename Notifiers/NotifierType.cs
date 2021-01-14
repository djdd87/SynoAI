using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Notifiers
{
    /// <summary>
    /// A list of support notification types.
    /// </summary>
    public enum NotifierType
    {
        /// <summary>
        /// Sends a notification message and image using the Pushbullet API.
        /// </summary>
        Pushbullet,
        /// <summary>
        /// Calls a webhook with the image attached.
        /// </summary>
        Webhook
    }
}
