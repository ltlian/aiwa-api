using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Schema.Files;

public class OpenAIFileConverter : JsonConverter<OpenAIFile>
{
    public override OpenAIFile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected JSON to start with an object.");
        }

        string? id = null;
        string? @object = null;
        long? bytes = null;
        long? createdAt = null;
        string? filename = null;
        string? purpose = null;

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
                    case "bytes":
                        bytes = reader.GetInt64();
                        break;
                    case "created_at":
                        createdAt = reader.GetInt64();
                        break;
                    case "filename":
                        filename = reader.GetString();
                        break;
                    case "object":
                        @object = reader.GetString();
                        break;
                    case "purpose":
                        purpose = reader.GetString();
                        break;
                    case "status":
                    case "status_details":
                        reader.Skip();
                        break; // Part of the response metadata, but not the file object.
                    default:
                        throw new JsonException($"Unknown property: {propertyName}");
                }
            }
        }

        return new OpenAIFile
        (
            id ?? throw new JsonException("Missing property: id"),
            bytes ?? throw new JsonException("Missing property: bytes"),
            createdAt ?? throw new JsonException("Missing property: created_at"),
            filename ?? throw new JsonException("Missing property: filename"),
            @object ?? throw new JsonException("Missing property: object"),
            purpose ?? throw new JsonException("Missing property: purpose")
        );
    }

    public override void Write(Utf8JsonWriter writer, OpenAIFile value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        writer.WriteNumber("bytes", value.Bytes);
        writer.WriteNumber("created_at", value.CreatedAt);
        writer.WriteString("filename", value.Filename);
        writer.WriteString("object", value.Object);
        writer.WriteString("purpose", value.Purpose);
        writer.WriteEndObject();
    }
}