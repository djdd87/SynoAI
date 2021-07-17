using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SynoAI.Extensions;
using SynoAI.Models;
using SynoAI.Services;

namespace SynoAI.Notifiers
{
    public abstract class NotifierBase : INotifier
    {
        public IEnumerable<string> Cameras { get; set;} 
        public abstract Task SendAsync(Camera camera, ISnapshotManager fileAccessor, IEnumerable<string> foundTypes, ILogger logger);

        protected string GetMessage(Camera camera, IEnumerable<string> foundTypes)
        {
            if (Config.AlternativeLabelling && Config.DrawMode == DrawMode.Matches)
            {
                String typeLabel = "object";

                if (camera.Types.Count() == 1) {
                    typeLabel = camera.Types.First();
                }

                if (foundTypes.Count() > 1)
                {
                    return $"Detected {foundTypes.Count()} {typeLabel}s on {camera.Name}:\n{String.Join("\n", foundTypes.Select(x => x).ToArray())}";
                }
                return $"Detected on {camera.Name}:{foundTypes.First().Substring(foundTypes.First().IndexOf(" "))}";              
            } 
            return $"Motion detected on {camera.Name}\n\nDetected {foundTypes.Count()} objects:\n{String.Join("\n", foundTypes.Select(x => x.FirstCharToUpper()).ToArray())}";         
        }
    }
}