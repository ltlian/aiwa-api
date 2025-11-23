using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Schema.Chat;

public class FinishReasonConverter : JsonConverter<FinishReason>
{
    public override FinishReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.ToLower() switch
        {
            "stop" => FinishReason.Stop,
            "length" => FinishReason.Length,
            "tool_calls" => FinishReason.ToolCalls,
            "content_filter" => FinishReason.ContentFilter,
            "function_call" => FinishReason.FunctionCall,
            _ => throw new JsonException("Invalid string " + reader.GetString()),
        };
    }

    public override void Write(Utf8JsonWriter writer, FinishReason value, JsonSerializerOptions options)
    {
        string stringValue = value switch
        {
            FinishReason.Stop => "stop",
            FinishReason.Length => "length",
            FinishReason.ToolCalls => "tool_calls",
            FinishReason.ContentFilter => "content_filter",
            FinishReason.FunctionCall => "function_call",
            _ => throw new IndexOutOfRangeException(value.ToString())
        };

        writer.WriteStringValue(stringValue);
    }
}