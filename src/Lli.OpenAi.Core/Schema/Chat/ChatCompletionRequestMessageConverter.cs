using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Chat;

public class ChatCompletionRequestMessageConverter : JsonConverter<IChatCompletionRequestMessage>
{
    public override ChatCompletionRequestUserMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewDeserializingRequestsPayloadsNotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, IChatCompletionRequestMessage value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("role"u8, value.Role);

        switch (value)
        {
            case ChatCompletionRequestUserMessage userMessage:
                writer.WritePropertyName("content"u8);
                SerializeUserMessageContent(writer, userMessage.Content);
                break;
            case ChatCompletionRequestAssistantMessage assistantMessage:
                writer.WritePropertyName("content"u8);
                JsonSerializer.Serialize(writer, assistantMessage.Content, options);
                break;
            case ChatCompletionRequestSystemMessage systemMessage:
                writer.WritePropertyName("content"u8);
                JsonSerializer.Serialize(writer, systemMessage.Content, options);
                break;
            default:
                throw new InvalidOperationException("Unhandled type");
        }

        if (value is IParticipantName participantMessage)
        {
            if (participantMessage.Name is not null)
                writer.WriteString("name"u8, participantMessage.Name);
        }

        writer.WriteEndObject();
    }

    private static void SerializeUserMessageContent(Utf8JsonWriter writer, IEnumerable<IChatCompletionRequestMessageContentPart> content)
    {
        writer.WriteStartArray();
        foreach (var contentPart in content)
        {
            writer.WriteStartObject();
            writer.WriteString("type"u8, contentPart.Type);
            switch (contentPart)
            {
                case ChatCompletionRequestMessageContentPartText textPart:
                    writer.WriteString("text"u8, textPart.Text);
                    break;
                case ChatCompletionRequestMessageContentPartImage imagePart:
                    writer.WritePropertyName("image_url"u8);
                    SerializeImageUrl(writer, imagePart.ImageUrl);
                    break;
                default:
                    throw new JsonException("Uhandled content part type: " + contentPart.Type);
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static void SerializeImageUrl(Utf8JsonWriter writer, ImageUrl imageUrl)
    {
        writer.WriteStartObject();
        writer.WriteString("url"u8, imageUrl.Url);

        if (!string.IsNullOrEmpty(imageUrl.Detail))
        {
            writer.WriteString("detail"u8, imageUrl.Detail);
        }

        writer.WriteEndObject();
    }
}
