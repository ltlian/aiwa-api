using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Schema.Thread;

public class RunObjectConverter : JsonConverter<RunObject>
{
    public override RunObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string? id = null;
        string? objectValue = null;
        long? createdAt = null;
        string? assistantId = null;
        string? threadId = null;
        string? status = null;
        long? startedAt = null;
        long? expiresAt = null;
        long? cancelledAt = null;
        long? failedAt = null;
        long? completedAt = null;
        Dictionary<string, object>? lastError = null;
        string? model = null;
        string? instructions = null;
        IAssistantTool[]? tools = null;
        string[]? fileIds = null;
        Dictionary<string, object>? metadata = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new RunObject
                (
                    id ?? throw new JsonException("Missing property 'id'"),
                    objectValue ?? throw new JsonException("Missing property 'objectValue'"),
                    createdAt ?? throw new JsonException("Missing property 'createdAt'"),
                    assistantId ?? throw new JsonException("Missing property 'assistantId'"),
                    threadId ?? throw new JsonException("Missing property 'threadId'"),
                    status ?? throw new JsonException("Missing property 'status'"),
                    startedAt,
                    expiresAt,
                    cancelledAt,
                    failedAt,
                    completedAt,
                    lastError,
                    model ?? throw new JsonException("Missing property 'model'"),
                    instructions ?? throw new JsonException("Missing property 'instructions'"),
                    tools,
                    fileIds,
                    metadata
                );
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read(); // Move to PropertyValue

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
                    case "assistant_id":
                        assistantId = reader.GetString();
                        break;
                    case "thread_id":
                        threadId = reader.GetString();
                        break;
                    case "status":
                        status = reader.GetString();
                        break;
                    case "started_at":
                        startedAt = reader.TokenType != JsonTokenType.Null ? reader.GetInt64() : null;
                        break;
                    case "expires_at":
                        expiresAt = reader.TokenType != JsonTokenType.Null ? reader.GetInt64() : null;
                        break;
                    case "cancelled_at":
                        cancelledAt = reader.TokenType != JsonTokenType.Null ? reader.GetInt64() : null;
                        break;
                    case "failed_at":
                        failedAt = reader.TokenType != JsonTokenType.Null ? reader.GetInt64() : null;
                        break;
                    case "completed_at":
                        completedAt = reader.TokenType != JsonTokenType.Null ? reader.GetInt64() : null;
                        break;
                    case "last_error":
                        lastError = reader.TokenType != JsonTokenType.Null ? JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options) : null;
                        break;
                    case "model":
                        model = reader.GetString();
                        break;
                    case "instructions":
                        instructions = reader.GetString();
                        break;
                    case "tools":
                        tools = JsonSerializer.Deserialize<IAssistantTool[]>(ref reader, options);
                        break;
                    case "file_ids":
                        fileIds = JsonSerializer.Deserialize<string[]>(ref reader, options);
                        break;
                    case "metadata":
                        metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                        break;
                    default:
                        throw new JsonException($"Unknown property: {propertyName}.");
                }
            }
        }

        throw new JsonException("Invalid JSON.");
    }

    public override void Write(Utf8JsonWriter writer, RunObject value, JsonSerializerOptions options)
    {
        // Implement if serialization is required
        throw new NotImplementedException();
    }
}
