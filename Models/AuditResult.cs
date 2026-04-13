using System.Text.Json.Serialization;

namespace CodeAegis.Models;

public class AuditResult
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("vulnerabilities")]
    public List<Vulnerability> Vulnerabilities { get; set; } = new();

    [JsonPropertyName("overallSecurityScore")]
    public int OverallSecurityScore { get; set; }
}

public class Vulnerability
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("suggestedFix")]
    public string SuggestedFix { get; set; } = string.Empty;
}