using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SynoAI.Models;
using SynoAI.Services;
using SynoAI.Extensions;

namespace SynoAI.Notifiers
{
    public abstract class NotifierBase : INotifier
    {
        public IEnumerable<string> Cameras { get; set;} 
        public abstract Task SendAsync(Camera camera, ISnapshotManager fileAccessor, IList<string> foundTypes, ILogger logger);

        protected string GetMessage(Camera camera, IList<string> foundTypes)
        {
            if (Config.AlternativeLabelling && Config.DrawMode == DrawMode.Matches)
            {
                //defaulting into generic label type
                String typeLabel = "object";

                if (camera.Types.Count() == 1) {
                    //Only one object type configured: use it instead of generic "object" label
                    typeLabel = camera.Types.First();
                }

                if (foundTypes.Count() > 1)
                {
                    //Several objects detected
                    return $"{camera.Name}: {foundTypes.Count()} {typeLabel}s\n{String.Join("\n", foundTypes.Select(x => x).ToArray())}";
                } 
                else 
                {
                    //Just one object detected
                    return $"{camera.Name}: {foundTypes.First()}";    
                }      
            }
            else 
            {
                //Standard (old) labelling
                return $"Motion detected on {camera.Name}\n\nDetected {foundTypes.Count()} objects:\n{String.Join("\n", foundTypes.Select(x => x).ToArray())}";   
            }
        }
    }
}