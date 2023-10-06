using System.ComponentModel.DataAnnotations;

namespace AIWA.API.Integrations.GPT4;

public class OpenAIOptions
{
    [Required]
    required public string OpenAIKey { get; set; }

    [Required]
    public string Model { get; set; } = Models.GPT4;

    public string? SystemPrompt { get; set; }
}