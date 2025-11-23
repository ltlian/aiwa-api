using System.Text;

namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionRequestSystemMessage
(
    string? Content,
    ParticipantName? Name = null
) : IChatCompletionRequestMessage<string?>, IParticipantName
{
    public const string Discriminator = "system";
    public string Role { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
