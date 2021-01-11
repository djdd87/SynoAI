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
        ///// <summary>
        ///// Calls a fire and forget webhook where the webhook will be responsible for sending the notifications.
        ///// </summary>
        //Webhook,
    }
}
