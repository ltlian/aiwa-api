using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Assistants;

public class AssistantFileConverter : JsonConverter<AssistantFile>
{
    public override AssistantFile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected JSON to start with an object.");
        }

        string? id = null;
        string? obj = null;
        long? createdAt = null;
        string? assistantId = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

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
                        obj = reader.GetString();
                        break;
                    case "created_at":
                        createdAt = reader.GetInt64();
                        break;
                    case "assistant_id":
                        assistantId = reader.GetString();
                        break;
                    default:
                        throw new JsonException($"Unknown property: {propertyName}");
                }
            }
        }

        return new AssistantFile(
            id ?? throw new JsonException("Missing property: id"),
            obj ?? throw new JsonException("Missing property: object"),
            createdAt ?? throw new JsonException("Missing property: created_at"),
            assistantId ?? throw new JsonException("Missing property: assistant_id")
        );
    }

    public override void Write(Utf8JsonWriter writer, AssistantFile value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        writer.WriteString("object", value.Object);
        writer.WriteNumber("created_at", value.CreatedAt);
        writer.WriteString("assistant_id", value.AssistantId);
        writer.WriteEndObject();
    }
}
