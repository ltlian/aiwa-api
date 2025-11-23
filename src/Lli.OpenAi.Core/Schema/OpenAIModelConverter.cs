using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lli.OpenAi.Core.Schema;

public class OpenAIModelConverter : JsonConverter<OpenAIModel>
{
    public override OpenAIModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => OpenAIModelMap.ToModel(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, OpenAIModel value, JsonSerializerOptions options) => writer.WriteStringValue(OpenAIModelMap.ToString(value));
}
