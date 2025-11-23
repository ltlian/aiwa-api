using System.Text.Json.Serialization;

namespace AIWA.API.Models;

public class UkesmailInput
{
    [JsonPropertyName("entries")]
    public required IEnumerable<string> Entries { get; set; }
}
