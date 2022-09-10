using Microsoft.Extensions.Logging;
using SynoAI.Models;
using SynoAI.Services;
using System.Collections.Generic;
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
        /// The list of types that the notifier is for.
        /// </summary>
        IEnumerable<string> Types { get; set; }
        /// <summary>
        /// Handles any initialization for the notifier (e.g. establishing a long-lived connection).
        /// </summary>
        Task InitializeAsync(ILogger logger);
        /// <summary>
        /// Handles the send of the notification.
        /// </summary>
        Task SendAsync(Camera camera, Notification notification, ILogger logger);
        /// <summary>
        /// Handles any clean up for the notifier (e.g. closing a long-lived connection).
        /// </summary>
        Task CleanupAsync(ILogger logger);
    }
}
