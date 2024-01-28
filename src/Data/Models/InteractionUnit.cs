using Lli.OpenAi.Core.Schema.Chat;

namespace AIWA.API.Data.Models;

public class InteractionUnit : IAiwaEntity
{
    public required Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid ParentId { get; set; }
    public virtual InteractionUnit Parent { get; set; } = null!;
    public Guid UserId { get; set; }
    public virtual AiwaUser User { get; set; } = new() { Id = Guid.Empty };
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public IChatCompletionRequestMessage ToChatCompletionRequestMessage() => ToChatCompletionRequestMessage(this);

    public static IChatCompletionRequestMessage ToChatCompletionRequestMessage(InteractionUnit interactionUnit)
    {
        switch (interactionUnit.Role)
        {
            case "user":
                if (interactionUnit.Content.StartsWith("data:image/png;base64,"))
                    return new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartImage(new ImageUrl(interactionUnit.Content))]);
                else
                    return new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartText(interactionUnit.Content)]);
            case "assistant":
                return new ChatCompletionRequestAssistantMessage(interactionUnit.Content);
            case "system":
                return new ChatCompletionRequestSystemMessage(interactionUnit.Content);
            default:
                throw new InvalidOperationException($"Unhandled role (Expected 'user', assistant', or 'system'. Actual '{interactionUnit.Role}')");
        }
    }

    public bool IsRoot() => Id == ParentId;
}
