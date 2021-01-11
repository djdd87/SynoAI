using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Exceptions
{
    public class SynologyLoginException : Exception
    {
        public SynologyLoginException(string message) :
            base(message)
        {
        }
    }
}
