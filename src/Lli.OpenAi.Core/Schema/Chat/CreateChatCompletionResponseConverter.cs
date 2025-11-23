using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Chat;

public class CreateChatCompletionResponseConverter : JsonConverter<CreateChatCompletionResponse>
{
    public override CreateChatCompletionResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        // Initialize variables to hold the properties of CreateChatCompletionResponse
        string? id = null;
        List<Choice>? choices = [];
        long created = 0;
        string? model = null;
        string? systemFingerprint = null;
        string? objectValue = null;
        CompletionUsage? usage = null;

        // Read through the JSON object
        while (reader.Read())
        {
            // Check if we've reached the end of the object
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            // Ensure that we are dealing with a property name
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token.");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("Missing required property name.");
            reader.Read(); // Move to the property value

            switch (propertyName)
            {
                case "id":
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException("Expected a string for the 'id' property.");
                    }
                    id = reader.GetString();
                    break;

                case "choices":
                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        throw new JsonException("Expected a start array token for the 'choices' property.");
                    }

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType != JsonTokenType.StartObject)
                        {
                            throw new JsonException("Expected a start object token for an item in 'choices' array.");
                        }

                        var choice = ReadChoice(ref reader, options);
                        choices.Add(choice);
                    }
                    break;

                case "created":
                    if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt64(out created))
                    {
                        throw new JsonException("Expected a long integer for the 'created' property.");
                    }
                    break;

                case "model":
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException("Expected a string for the 'model' property.");
                    }
                    model = reader.GetString();
                    break;

                case "system_fingerprint":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        systemFingerprint = reader.GetString();
                    }
                    break;

                case "object":
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException("Expected a string for the 'object' property.");
                    }
                    objectValue = reader.GetString();
                    break;

                case "usage":
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException("Expected a start object token for the 'usage' property.");
                    }
                    usage = JsonSerializer.Deserialize<CompletionUsage>(ref reader, options);
                    break;

                default:
                    // Skip other properties for now
                    reader.Skip();
                    break;
            }
        }

        if (choices.Count == 0 || created == 0 || id == null || model == null || objectValue == null)
        {
            throw new JsonException("Missing required property.");
        }

        // Construct and return the CreateChatCompletionResponse object
        return new CreateChatCompletionResponse(id, choices, created, model, systemFingerprint, objectValue, usage);
    }

    private static Choice ReadChoice(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected a start object token for an item in 'choices' array.");
        }

        FinishReason? finishReason = null;
        int index = 0;
        ChatCompletionResponseMessage? message = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected a property name token in a 'Choice' object.");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("Missing required property name.");
            reader.Read(); // Move to the property value
            
            switch (propertyName)
            {
                case "finish_reason":
                    finishReason = JsonSerializer.Deserialize<FinishReason>(ref reader, options);
                    break;

                case "index":
                    if (!reader.TryGetInt32(out index))
                    {
                        throw new JsonException("Expected an integer for 'index'.");
                    }
                    break;

                case "message":
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException("Expected a start object token for 'message'.");
                    }
                    message = JsonSerializer.Deserialize<ChatCompletionResponseMessage>(ref reader, options);
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        return new Choice
        (
            message ?? throw new JsonException("Missing 'message'"),
            finishReason ?? throw new JsonException("Missing 'finishReason'"),
            index
        );
    }


    public override void Write(Utf8JsonWriter writer, CreateChatCompletionResponse value, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewSerializingResponsePayloadsNotSupportedException();
    }
}