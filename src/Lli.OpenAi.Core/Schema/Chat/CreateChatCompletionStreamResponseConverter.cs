using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Chat;

public class CreateChatCompletionStreamResponseConverter : JsonConverter<CreateChatCompletionStreamResponse>
{
    public override CreateChatCompletionStreamResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        string? id = null;
        List<ChoiceDelta>? choices = [];
        long created = 0;
        string? model = null;
        string? systemFingerprint = null;
        string? objectValue = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token.");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("Missing required property name.");
            reader.Read(); // Move to the property value

            switch (propertyName)
            {
                case "id":
                    id = ReadStringProperty(ref reader);
                    break;

                case "choices":
                    choices = ReadChoicesDelta(ref reader, options);
                    break;

                case "created":
                    created = ReadLongProperty(ref reader);
                    break;

                case "model":
                    model = ReadStringProperty(ref reader);
                    break;

                case "system_fingerprint":
                    systemFingerprint = reader.GetString();
                    break;

                case "object":
                    objectValue = ReadStringProperty(ref reader);
                    break;

                default:
                    reader.Skip(); // Skip other properties for now
                    break;
            }
        }

        return new CreateChatCompletionStreamResponse
        (
            id ?? throw new JsonException("Missing 'id' in 'CreateChatCompletionStreamResponse'."),
            choices.Count > 0 ? choices : throw new JsonException("Missing 'choices' in 'CreateChatCompletionStreamResponse'."),
            created > 0 ? created : throw new JsonException("Missing 'created' in 'CreateChatCompletionStreamResponse'."),
            model ?? throw new JsonException("Missing 'model' in 'CreateChatCompletionStreamResponse'."),
            systemFingerprint,
            objectValue ?? throw new JsonException("Missing 'object' in 'CreateChatCompletionStreamResponse'.")
        );
    }

    private static string? ReadStringProperty(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }

        throw new JsonException("Expected a string property.");
    }

    private static long ReadLongProperty(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long value))
        {
            return value;
        }

        throw new JsonException("Expected a long integer property.");
    }

    private static List<ChoiceDelta> ReadChoicesDelta(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected a start array token for 'choices'.");
        }

        var choices = new List<ChoiceDelta>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var choice = ReadChoiceDelta(ref reader, options);
            choices.Add(choice);
        }
        return choices;
    }

    private static ChoiceDelta ReadChoiceDelta(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected a start object token for an item in 'choices' array.");
        }

        ChatCompletionStreamResponseDelta? delta = null;
        FinishReason? finishReason = null;
        int index = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected a property name token in a 'ChoiceDelta' object.");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("Missing required property name.");
            reader.Read(); // Move to the property value

            switch (propertyName)
            {
                case "delta":
                    delta = ReadChatCompletionStreamResponseDelta(ref reader, options);
                    break;

                case "finish_reason":
                    finishReason = JsonSerializer.Deserialize<FinishReason>(ref reader, options);
                    break;

                case "index":
                    if (!reader.TryGetInt32(out index))
                    {
                        throw new JsonException("Expected an integer for 'index'.");
                    }
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        return new ChoiceDelta
        (
            delta ?? throw new JsonException("Missing 'delta' in 'ChoiceDelta'."),
            finishReason,
            index
        );
    }

    private static ChatCompletionStreamResponseDelta ReadChatCompletionStreamResponseDelta(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected a start object token for 'ChatCompletionStreamResponseDelta'.");
        }

        string? role = null;
        string? content = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected a property name token in a 'ChatCompletionStreamResponseDelta' object.");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("Missing required property name.");
            reader.Read(); // Move to the property value

            switch (propertyName)
            {
                case "content":
                    content = ReadStringProperty(ref reader);
                    break;

                case "role":
                    role = ReadStringProperty(ref reader);
                    break;

                case "function_call":
                case "tool_calls":
                    reader.Skip();
                    break;

                default:
                    throw new InvalidOperationException("Unexpected property");
            }
        }

        return new ChatCompletionStreamResponseDelta
        (
            role ?? throw new JsonException("Missing 'role' in 'ChatCompletionStreamResponseDelta'."),
            content ?? throw new JsonException("Missing 'content' in 'ChatCompletionStreamResponseDelta'.")
        );
    }

    public override void Write(Utf8JsonWriter writer, CreateChatCompletionStreamResponse value, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewSerializingResponsePayloadsNotSupportedException();
    }
}