using SynoAI.Models;
using System.Net;

namespace SynoAI.Services
{
    /// <summary>
    /// Interface for interacting with the Synology service.
    /// </summary>
    public interface ISynologyService
    {
        /// <summary>
        /// Task to initialise the service
        /// </summary>
        Task InitialiseAsync();
        /// <summary>
        /// Logon to the service
        /// </summary>
        Task<Cookie> LoginAsync();
        /// <summary>
        /// Get the cameras available in Synology
        /// </summary>
        Task<IEnumerable<SynologyCamera>> GetCamerasAsync();
        /// <summary>
        /// taking the snapshot
        /// </summary>
        Task<byte[]> TakeSnapshotAsync(string cameraName);
    }
}
