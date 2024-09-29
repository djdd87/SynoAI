using System.Text.Json.Serialization;

namespace SynoAI.Core;

/// <summary>
/// Defines the type of zone, i.e. whether it's an exclusion or inclusion zone.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ZoneType
{
    Exclusion = 0,
    Inclusion = 1
}
