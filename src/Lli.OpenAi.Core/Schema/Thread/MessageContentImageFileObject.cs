using System.Text;

namespace Lli.OpenAi.Core.Schema.Thread;

public record MessageContentImageFileObject
(
    ImageFileDetails ImageFile
) : IMessageContentObject
{
    public const string Discriminator = "image_file";
    public string Type { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
