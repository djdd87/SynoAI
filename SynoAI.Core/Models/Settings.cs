namespace SynoAI.Core.Data;

/// <summary>
/// Represents a configuration setting in the system.
/// </summary>
public class Setting
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}