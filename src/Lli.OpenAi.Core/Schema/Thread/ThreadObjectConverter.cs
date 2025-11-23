using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Schema.Thread;

public class ThreadObjectConverter : JsonConverter<ThreadObject>
{
    public override ThreadObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        string? id = null;
        string? objectValue = null;
        long createdAt = default;
        Dictionary<string, string>? metadata = null;

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
                    case "metadata":
                        metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
                        break;
                }
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
        }

        return new ThreadObject(
            id ?? throw new JsonException("The 'id' property is missing."),
            objectValue ?? throw new JsonException("The 'object' property is missing."),
            createdAt,
            metadata);
    }

    public override void Write(Utf8JsonWriter writer, ThreadObject value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
