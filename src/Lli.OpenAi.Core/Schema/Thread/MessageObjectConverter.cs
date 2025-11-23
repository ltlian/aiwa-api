using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Thread;

public class MessageObjectConverter : JsonConverter<MessageObject>
{
    public override MessageObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        string? id = null;
        string? objectValue = null;
        long? createdAt = null;
        string? threadId = null;
        string? role = null;
        IMessageContentObject[]? content = null;
        string? assistantId = null;
        string? runId = null;
        string[]? fileIds = null;
        Dictionary<string, object>? metadata = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "id":
                        id = reader.GetString();
                        break;
                    case "object":
                        objectValue = reader.GetString();
                        break;
                    case "created_at":
                        createdAt = reader.GetInt64();
                        break;
                    case "thread_id":
                        threadId = reader.GetString();
                        break;
                    case "role":
                        role = reader.GetString();
                        break;
                    case "content":
                        //content = ReadContentArray(ref reader, options);
                        content = JsonSerializer.Deserialize<IMessageContentObject[]>(ref reader, options);
                        break;
                    case "assistant_id":
                        assistantId = reader.GetString();
                        break;
                    case "run_id":
                        runId = reader.GetString();
                        break;
                    case "file_ids":
                        fileIds = JsonSerializer.Deserialize<string[]>(ref reader, options);
                        break;
                    case "metadata":
                        metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                        break;
                }
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
        }

        return new MessageObject
        (
            id ?? throw new JsonException("The 'id' property is missing."),
            objectValue ?? throw new JsonException("The 'object' property is missing."),
            createdAt ?? throw new JsonException("The 'created_at' property is missing."),
            threadId ?? throw new JsonException("The 'thread_id' property is missing."),
            role ?? throw new JsonException("The 'role' property is missing."),
            content ?? throw new JsonException("The 'content' property is missing."),
            assistantId,
            runId,
            fileIds,
            metadata
        );
    }

    public override void Write(Utf8JsonWriter writer, MessageObject value, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewSerializingResponsePayloadsNotSupportedException();
    }
}