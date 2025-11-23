namespace Lli.OpenAi.Core.Schema.Chat;

public interface IChatCompletionRequestMessage
{
    string Role { get; }
}

public interface IChatCompletionRequestMessage<T> : IChatCompletionRequestMessage
{
    T Content { get; }
}
