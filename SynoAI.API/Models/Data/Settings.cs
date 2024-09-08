namespace SynoAI.API.Models.Data;

public class Setting
{
    public const string KEY_SSS_API = "SSS_API";
    public const string KEY_SSS_API_ALLOW_INSECURE = "SSS_API_ALLOW_INSECURE";

    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
}