using Microsoft.Extensions.Logging;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Notifiers
{
    public interface INotifier
    {
        /// <summary>
        /// The list of camera names that the notifier is for.
        /// </summary>
        IEnumerable<string> Cameras { get; set; }
        /// <summary>
        /// Handles the send of the notification.
        /// </summary>
        Task Send(Camera camera, string filePath, IEnumerable<string> foundTypes, ILogger logger);
    }
}
