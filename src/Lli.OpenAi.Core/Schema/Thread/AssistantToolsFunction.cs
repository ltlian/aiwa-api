using System.Text;

namespace Lli.OpenAi.Core.Schema.Thread;

public record AssistantToolsFunction(Dictionary<string, object> Function /* To be implemented later */) : IAssistantTool
{
    public const string Discriminator = "function";
    public string Type { get; } = Discriminator;
    public static readonly byte[] DiscriminatorValue = Encoding.UTF8.GetBytes(Discriminator);
}
