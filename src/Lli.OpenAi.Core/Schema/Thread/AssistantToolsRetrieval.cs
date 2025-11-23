using System.Text;

namespace Lli.OpenAi.Core.Schema.Thread;

public record AssistantToolsRetrieval() : IAssistantTool
{
    public const string Discriminator = "retrieval";
    public string Type { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
