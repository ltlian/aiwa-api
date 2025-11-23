using System.Text;

namespace Lli.OpenAi.Core.Schema.Thread;

public record AssistantToolsCode() : IAssistantTool
{
    public const string Discriminator = "code_interpreter";
    public string Type { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
