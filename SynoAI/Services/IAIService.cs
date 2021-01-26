using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Services
{
    public interface IAIService
    {
        Task<IEnumerable<AIPrediction>> ProcessAsync(Camera camera, byte[] image);
    }
}
