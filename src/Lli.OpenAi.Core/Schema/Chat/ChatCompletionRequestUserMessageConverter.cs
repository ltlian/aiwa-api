using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Chat;

public class ChatCompletionRequestUserMessageConverter : JsonConverter<ChatCompletionRequestUserMessage>
{
    public override ChatCompletionRequestUserMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewDeserializingRequestsPayloadsNotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, ChatCompletionRequestUserMessage value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Role", value.Role);
        writer.WritePropertyName("Content");
        writer.WriteStartArray();
        foreach (var contentPart in value.Content)
        {
            JsonSerializer.Serialize(writer, contentPart, options);
        }
        writer.WriteEndArray();
        if (!string.IsNullOrEmpty(value.Name))
        {
            writer.WriteString("Name", value.Name);
        }
        writer.WriteEndObject();
    }
}