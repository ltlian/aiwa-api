using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Speech;

public class TtsModelConverter : JsonConverter<TtsModel>
{
    public override TtsModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.ToLowerInvariant() switch
        {
            "tts-1" => TtsModel.Tts1,
            "tts-1-hd" => TtsModel.Tts1Hd,
            _ => throw new JsonException("Invalid string " + reader.GetString()),
        };
    }

    public override void Write(Utf8JsonWriter writer, TtsModel value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            TtsModel.Tts1 => "tts-1",
            TtsModel.Tts1Hd => "tts-1-hd",
            _ => throw new IndexOutOfRangeException(value.ToString())
        };

        writer.WriteStringValue(stringValue);
    }
}