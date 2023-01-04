using SynoAI.Models;
using System.Net;

namespace SynoAI.Services
{
    public interface ISynologyService
    {
        Task InitialiseAsync();
        Task<Cookie> LoginAsync();
        Task<IEnumerable<SynologyCamera>> GetCamerasAsync();
        Task<byte[]> TakeSnapshotAsync(string cameraName);
    }
}
