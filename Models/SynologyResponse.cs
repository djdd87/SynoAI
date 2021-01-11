using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Models
{
    public class SynologyResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public SynologyError Error { get; set; }
    }
}
