namespace Lli.OpenAi.Core.Schema.Chat;

public record CreateChatCompletionRequest
(
    List<IChatCompletionRequestMessage> Messages,
    OpenAIModel Model,
    double? FrequencyPenalty = null,
    Dictionary<int, int>? LogitBias = null,
    bool? Logprobs = null,
    int? TopLogprobs = null,
    int? MaxTokens = null,
    int? N = null,
    double? PresencePenalty = null,
    ResponseFormat? ResponseFormat = null,
    long? Seed = null,
    object? Stop = null, // Can be string or array of strings
    bool? Stream = null,
    double? Temperature = null,
    double? TopP = null,
    List<ChatCompletionTool>? Tools = null,
    ChatCompletionToolChoiceOption? ToolChoice = null,
    string? User = null
);

public record ResponseFormat
(
    string Type
);

public record ChatCompletionTool
(
    // To be implemented later and can be ignored for now.
);

public record ChatCompletionToolChoiceOption
(
    // To be implemented later and can be ignored for now.
);
