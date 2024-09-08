using System.ComponentModel.DataAnnotations;

namespace SynoAI.Core.Data;

/// <summary>
/// Represents a configuration setting in the system.
/// </summary>
/// <param name="Key">The key of the setting.</param>
/// <param name="Value">The value of the setting.</param>
public class Setting
{
    /// <summary>
    /// The API path to Synology Surveillance Station.
    /// </summary>
    public const string KEY_SSS_API = "SSS_API";
    /// <summary>
    /// Whether to allow insecure URLs, e.g. bad certificate.
    /// </summary>
    public const string KEY_SSS_API_ALLOW_INSECURE = "SSS_API_ALLOW_INSECURE";

    [Key]
    [MaxLength(100)]
    public required string Key {get;set;} 
    [MaxLength(500)]
    public required string Value {get;set;} 
}