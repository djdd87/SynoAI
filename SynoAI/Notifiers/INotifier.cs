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
        /// Handles the send of the notification.
        /// </summary>
        Task SendAsync(Camera camera, ISnapshotManager snapshotManager, IEnumerable<string> foundTypes, ILogger logger);
    }
}
