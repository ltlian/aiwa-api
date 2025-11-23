namespace Lli.OpenAi.Core.Schema;

public static class OpenAIModelMap
{
    public static string ToString(OpenAIModel openAIModel) => openAIModel switch
    {
        OpenAIModel.Gpt41106Preview => "gpt-4-1106-preview",
        OpenAIModel.Gpt4VisionPreview => "gpt-4-vision-preview",
        OpenAIModel.Gpt4 => "gpt-4",
        OpenAIModel.Gpt40314 => "gpt-4-0314",
        OpenAIModel.Gpt40613 => "gpt-4-0613",
        OpenAIModel.Gpt432k => "gpt-4-32k",
        OpenAIModel.Gpt432k0314 => "gpt-4-32k-0314",
        OpenAIModel.Gpt432k0613 => "gpt-4-32k-0613",
        OpenAIModel.Gpt35Turbo => "gpt-3.5-turbo",
        OpenAIModel.Gpt35Turbo16k => "gpt-3.5-turbo-16k",
        OpenAIModel.Gpt35Turbo0301 => "gpt-3.5-turbo-0301",
        OpenAIModel.Gpt35Turbo0613 => "gpt-3.5-turbo-0613",
        OpenAIModel.Gpt35Turbo1106 => "gpt-3.5-turbo-1106",
        OpenAIModel.Gpt35Turbo16k0613 => "gpt-3.5-turbo-16k-0613",
        _ => throw new ArgumentException("Unhandled model", nameof(openAIModel))
    };

    public static OpenAIModel ToModel(string openAIModel) => openAIModel switch
    {
        "gpt-4-1106-preview" => OpenAIModel.Gpt41106Preview,
        "gpt-4-vision-preview" => OpenAIModel.Gpt4VisionPreview,
        "gpt-4" => OpenAIModel.Gpt4,
        "gpt-4-0314" => OpenAIModel.Gpt40314,
        "gpt-4-0613" => OpenAIModel.Gpt40613,
        "gpt-4-32k" => OpenAIModel.Gpt432k,
        "gpt-4-32k-0314" => OpenAIModel.Gpt432k0314,
        "gpt-4-32k-0613" => OpenAIModel.Gpt432k0613,
        "gpt-3.5-turbo" => OpenAIModel.Gpt35Turbo,
        "gpt-3.5-turbo-16k" => OpenAIModel.Gpt35Turbo16k,
        "gpt-3.5-turbo-0301" => OpenAIModel.Gpt35Turbo0301,
        "gpt-3.5-turbo-0613" => OpenAIModel.Gpt35Turbo0613,
        "gpt-3.5-turbo-1106" => OpenAIModel.Gpt35Turbo1106,
        "gpt-3.5-turbo-16k-0613"  => OpenAIModel.Gpt35Turbo16k0613,
        _ => throw new ArgumentException("Unhandled model", nameof(openAIModel))
    };
}
