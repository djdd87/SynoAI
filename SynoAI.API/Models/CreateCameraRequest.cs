using SynoAI.Core.Data;

namespace SynoAI.API.Models
{
    public class CreateCameraRequest
    {
        public required string Name { get; set; }
        public required QualityProfile QualityProfile { get; set; }
    }
}
