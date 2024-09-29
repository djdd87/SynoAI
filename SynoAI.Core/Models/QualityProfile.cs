using System.Text.Json.Serialization;

namespace SynoAI.Core.Models;

/// <summary>
/// Represents the camera quality options in SSS.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QualityProfile
{
    HighQuality = 0,
    Balanced = 1
}