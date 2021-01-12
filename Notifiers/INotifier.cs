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
        Task Send(Camera camera, string filePath, IEnumerable<string> foundTypes, ILogger logger);
    }
}
