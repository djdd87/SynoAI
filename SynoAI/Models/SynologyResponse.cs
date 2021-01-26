using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Models
{
    public class SynologyResponse<T> : SynologyResponse
    {
        public T Data { get; set; }
    }

    public class SynologyResponse
    {
        public bool Success { get; set; }
        public SynologyError Error { get; set; }
    }
}
