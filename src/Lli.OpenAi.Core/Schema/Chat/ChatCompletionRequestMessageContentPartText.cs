using System.Text;

namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionRequestMessageContentPartText
(
    string? Text
) : IChatCompletionRequestMessageContentPart
{
    public const string Discriminator = "text";
    public string Type { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
