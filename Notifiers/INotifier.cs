using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Notifiers
{
    public interface INotifier
    {
        void Send(ILogger logger);
    }
}
