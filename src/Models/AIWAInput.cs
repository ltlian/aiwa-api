using System.Text.Json.Serialization;

namespace AIWA.API.Models;

public class AIWAInput
{
    [JsonPropertyName("entries")]
    public required IEnumerable<string> Entries { get; set; }
}
