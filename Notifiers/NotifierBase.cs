using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SynoAI.Models;

namespace SynoAI.Notifiers
{
    public abstract class NotifierBase : INotifier
    {
        public IEnumerable<string> Cameras { get; set;} 
        public abstract Task Send(Camera camera, string filePath, IEnumerable<string> foundTypes, ILogger logger);
    }
}