using System.Text;

namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionRequestAssistantMessage
(
    string? Content = null,
    ParticipantName? Name = null
) : IChatCompletionRequestMessage<string?>, IParticipantName
{
    public const string Discriminator = "assistant";
    public string Role { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
