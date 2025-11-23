using System.ComponentModel.DataAnnotations;

namespace Lli.OpenAi.Core.Client;

public class OpenAIHttpClientOptions
{
    [Required]
    public required string OpenAiKey { get; set; }
}
