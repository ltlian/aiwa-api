using System.Text;

namespace Lli.OpenAi.Core.Schema.Thread;

public record MessageContentTextObject
(
    MessageContentTextObjectText Text
) : IMessageContentObject
{
    public const string Discriminator = "text";
    public string Type { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
