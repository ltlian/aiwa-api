using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Chat;

public class CreateChatCompletionRequestConverter : JsonConverter<CreateChatCompletionRequest>
{
    public override CreateChatCompletionRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewDeserializingRequestsPayloadsNotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, CreateChatCompletionRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("messages");
        JsonSerializer.Serialize(writer, value.Messages, options);

        writer.WriteString("model"u8, OpenAIModelMap.ToString(value.Model));

        if (value.FrequencyPenalty.HasValue)
        {
            writer.WriteNumber("frequency_penalty"u8, value.FrequencyPenalty.Value);
        }

        if (value.MaxTokens.HasValue)
        {
            writer.WriteNumber("max_tokens"u8, value.MaxTokens.Value);
        }

        if (value.Stream.HasValue)
        {
            writer.WriteBoolean("stream"u8, value.Stream.Value);
        }

        writer.WriteEndObject();
    }
}
