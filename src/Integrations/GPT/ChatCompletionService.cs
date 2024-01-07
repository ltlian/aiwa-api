using AIWA.API.Integrations.GPT4;
using Azure.AI.OpenAI;
using Lli.OpenAi.Core.Client;
using Lli.OpenAi.Core.Schema.Chat;
using Microsoft.Extensions.Options;

namespace AIWA.API.Integrations.GPT;

public class ChatCompletionService(IOptions<OpenAIOptions> options, ILogger<ChatCompletionService> logger, OpenAiHttpClient openAilient) : IChatCompletion
{
    private readonly string _deploymentId = options.Value.Model;
    private readonly ILogger<ChatCompletionService> _logger = logger;
    private readonly OpenAiHttpClient _openAiClient = openAilient;
    public OpenAiHttpClient OpenAiHttpClient => _openAiClient;

    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> GetChatCompletionStreamAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = new List<IChatCompletionRequestMessage>
            {
                new ChatCompletionRequestSystemMessage(prompt)
            };

            var chatCompletionRequest = new CreateChatCompletionRequest(messages, _deploymentId);
            var result = await _openAiClient.CreateChatCompletionAsync(chatCompletionRequest, cancellationToken);

            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling OpenAI");
            throw;
        }
    }
}
