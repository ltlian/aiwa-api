using System.Text;

namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionRequestMessageContentPartImage
(
    ImageUrl ImageUrl
) : IChatCompletionRequestMessageContentPart
{
    public const string Discriminator = "image_url";
    public string Type { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
