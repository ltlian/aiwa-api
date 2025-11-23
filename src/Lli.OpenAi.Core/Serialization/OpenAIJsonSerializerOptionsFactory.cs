using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Lli.OpenAi.Core.Assistants;
using Lli.OpenAi.Core.Schema;
using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Files;
using Lli.OpenAi.Core.Schema.Thread;
using Lli.OpenAi.Core.Speech;

namespace Lli.OpenAi.Core.Serialization;

public static class OpenAIJsonSerializerOptionsFactory
{
    public static JsonSerializerOptions GetJsonSerializerOptions() => new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters =
            {
                new JsonStringEnumConverter(),
                new OpenAIModelConverter(),
                new FinishReasonConverter(),
                new CreateChatCompletionRequestConverter(),
                new ChatCompletionRequestMessageConverter(),
                new CreateChatCompletionResponseConverter(),
                new CreateChatCompletionStreamResponseConverter(),
                new ChatCompletionRequestMessageContentPartConverter(),
                new TtsModelConverter(),
                new VoiceConverter(),
                new ResponseFormatConverter(),
                new AssistantToolsConverter(),
                new ThreadObjectConverter(),
                new RunObjectConverter(),
                new CreateMessageRequestConverter(),
                new CreateThreadRequestConverter(),
                new MessageObjectConverter(),
                new MessageContentObjectsConverter(),
                new OpenAIFileConverter(),
                new AssistantFileConverter()
            }
    };
}
