using System.Text;

namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionRequestUserMessage
(
    IEnumerable<IChatCompletionRequestMessageContentPart> Content,
    ParticipantName? Name = null
) : IChatCompletionRequestMessage<IEnumerable<IChatCompletionRequestMessageContentPart>>, IParticipantName
{
    public const string Discriminator = "user";
    public string Role { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
