using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Thread;

public class CreateThreadRequestConverter : JsonConverter<CreateThreadRequest>
{
    public override CreateThreadRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewDeserializingRequestsPayloadsNotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, CreateThreadRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Messages?.Count() > 0)
        {
            writer.WritePropertyName("messages");
            writer.WriteStartArray();
            foreach (var message in value.Messages)
            {
                JsonSerializer.Serialize(writer, message, options);
            }
            writer.WriteEndArray();
        }

        if (value.Metadata != null)
        {
            writer.WritePropertyName("metadata");
            JsonSerializer.Serialize(writer, value.Metadata, options);
        }

        writer.WriteEndObject();
    }
}
