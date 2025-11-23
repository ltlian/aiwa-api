using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Thread;

public class CreateMessageRequestConverter : JsonConverter<CreateMessageRequest>
{
    public override CreateMessageRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewDeserializingRequestsPayloadsNotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, CreateMessageRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("role", value.Role);
        writer.WriteString("content", value.Content);

        if (value.FileIds?.Count() > 0)
        {
            writer.WritePropertyName("file_ids");
            JsonSerializer.Serialize(writer, value.FileIds, options);
        }

        if (value.Metadata?.Count > 0)
        {
            writer.WritePropertyName("metadata");
            JsonSerializer.Serialize(writer, value.Metadata, options);
        }

        writer.WriteEndObject();
    }
}
