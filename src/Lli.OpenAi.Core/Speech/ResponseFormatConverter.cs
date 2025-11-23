using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Speech;

public class ResponseFormatConverter : JsonConverter<OutputFormat>
{
    public static string ToFileExtension(OutputFormat outputFormat) => outputFormat switch
    {
        OutputFormat.Aac => "aac",
        OutputFormat.Flac => "flac",
        OutputFormat.Mp3 => "mp3",
        OutputFormat.Opus => "opus",
        _ => throw new IndexOutOfRangeException(outputFormat.ToString())
    };

    public override OutputFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.ToLower() switch
        {
            "aac" => OutputFormat.Aac,
            "flac" => OutputFormat.Flac,
            "mp3" => OutputFormat.Mp3,
            "opus" => OutputFormat.Opus,
            _ => throw new JsonException("Invalid string " + reader.GetString()),
        };
    }

    public override void Write(Utf8JsonWriter writer, OutputFormat value, JsonSerializerOptions options) =>
        writer.WriteStringValue(ToFileExtension(value));
}
