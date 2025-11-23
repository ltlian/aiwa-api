using System.Text.Json.Serialization;

namespace AIWA.API.Models;

public class AIWAResponse
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("diagnostics")]
    public Dictionary<string, string>[]? Diagnostics { get; set; }
}
