using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Speech;

public class SpeechRequestConverter : JsonConverter<CreateSpeechRequest>
{
    public override CreateSpeechRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewDeserializingRequestsPayloadsNotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, CreateSpeechRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        JsonSerializer.Serialize(writer, value.Model, options);
        writer.WriteString("input", value.Input);
        JsonSerializer.Serialize(writer, value.Voice, options);
        if (value.ResponseFormat.HasValue)
            JsonSerializer.Serialize(writer, value.ResponseFormat.Value, options);
        if (value.Speed.HasValue)
            writer.WriteNumber("speed", value.Speed.Value);
        writer.WriteEndObject();
    }
}
