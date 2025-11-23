using System.Text.Json;
using System.Text.Json.Serialization;
using Lli.OpenAi.Core.Serialization;

namespace Lli.OpenAi.Core.Schema.Chat;

public class ChatCompletionRequestMessageContentPartConverter : JsonConverter<IChatCompletionRequestMessageContentPart>
{
    public override IChatCompletionRequestMessageContentPart Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw Utf8JsonReaderHelpers.NewDeserializingRequestsPayloadsNotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, IChatCompletionRequestMessageContentPart value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
