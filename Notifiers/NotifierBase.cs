using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SynoAI.Models;
using SynoAI.Services;

namespace SynoAI.Notifiers
{
    public abstract class NotifierBase : INotifier
    {
        public IEnumerable<string> Cameras { get; set;} 
        public abstract Task Send(Camera camera, ISnapshotManager fileAccessor, IEnumerable<string> foundTypes, ILogger logger);
    }
}