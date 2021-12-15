using SynoAI.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

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
