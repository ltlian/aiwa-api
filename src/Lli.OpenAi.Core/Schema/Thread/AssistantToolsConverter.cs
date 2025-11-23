using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Thread;

public class AssistantToolsConverter : JsonConverter<IAssistantTool[]>
{
    public override IAssistantTool[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        var tools = new List<IAssistantTool>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return [.. tools];
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                Utf8JsonReaderHelpers.EnsureDiscriminatorProperty(ref reader, Utf8JsonReaderHelpers.TypeDiscriminator);
                reader.Read();

                if (reader.ValueTextEquals(AssistantToolsRetrieval.DiscriminatorValue))
                {
                    tools.Add(new AssistantToolsRetrieval());
                }
                else if (reader.ValueTextEquals(AssistantToolsCode.DiscriminatorValue))
                {
                    tools.Add(new AssistantToolsCode());
                }
                else if (reader.ValueTextEquals(AssistantToolsFunction.DiscriminatorValue))
                {
                    var function = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                    tools.Add(new AssistantToolsFunction(function!));
                }
                else
                {
                    throw new JsonException("Unknown tool type.");
                }
            }
        }

        throw new JsonException("Invalid JSON.");
    }

    public override void Write(Utf8JsonWriter writer, IAssistantTool[] value, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewSerializingResponsePayloadsNotSupportedException();
    }
}
