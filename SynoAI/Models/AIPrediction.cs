using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Models
{
    public class AIPrediction
    {
        public string Label { get; set; }
        public decimal Confidence { get; set; }
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }

        public int SizeX
        {
            get
            {
                return MaxX - MinX;
            }
        }

        public int SizeY
        {
            get
            {
                return MaxY - MinY;
            }
        }
    }
}
