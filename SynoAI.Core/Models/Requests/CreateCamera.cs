using Microsoft.EntityFrameworkCore.Query.Internal;
using SynoAI.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoAI.Core.Models.Requests
{
    public class CreateCamera
    {
        public required string Name { get; set; }
        public required QualityProfile QualityProfile { get; set; }
    }
}
