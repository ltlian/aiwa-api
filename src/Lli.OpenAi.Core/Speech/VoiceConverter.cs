using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Speech;

public class VoiceConverter : JsonConverter<Voice>
{
    public override Voice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString()?.ToLower() switch
    {
        "alloy" => Voice.Alloy,
        "echo" => Voice.Echo,
        "fable" => Voice.Fable,
        "nova" => Voice.Nova,
        "onyx" => Voice.Onyx,
        "shimmer" => Voice.Shimmer,
        _ => throw new JsonException("Invalid string " + reader.GetString())
    };

    public override void Write(Utf8JsonWriter writer, Voice value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            Voice.Alloy => "alloy",
            Voice.Echo => "echo",
            Voice.Fable => "fable",
            Voice.Nova => "nova",
            Voice.Onyx => "onyx",
            Voice.Shimmer => "shimmer",
            _ => throw new IndexOutOfRangeException(value.ToString())
        };

        writer.WriteStringValue(stringValue);
    }
}