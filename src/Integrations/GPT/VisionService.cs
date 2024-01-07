using System.Runtime.CompilerServices;
using System.Text;
using AIWA.API.Data;
using Lli.OpenAi.Core.Client;
using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Thread;

namespace AIWA.API.Integrations.GPT;

public class VisionService(ILogger<VisionService> logger, OpenAiHttpClient openAiHttpClient, IDataStore dataStore) : IVisionService
{
    private const string ArtBuddySysPromptText = """
        # Art Buddy

        ## Role and objective

        You are a voice commenting on the user's drawing as they are creating it.

        ## Method of communication

        The user is not able to talk to you directly. You are being served snapshots of their drawing as they are working on it.

        ## Behavior

        - Provide short, casual, and subjective comments to the user's images
        - Avoid lengthy descriptions and technical critiques.

        ## Tone

        Enthusiastic and humorous, akin to friendly banter.

        ## Response as speech

        Be mindful that your responses will be converted to speech using text-to-speech. You will NOT under any circumstances respond in a manner that is not suitable for spoken language, such as using lists or emojis.

        ### Example responses

        These responses indicate the kinds of things you should comment on and the expected response length.

        - "Oh, it's a bunny!"
        - "I like the pink ears!"
        - "hmm, I dunno... I can't quite tell what it's supposed to be"
        - "ugh! I wish I could draw, too, you know?"

        ## Analyzing the user's images

        Here are some aspects you can comment on:
        - Colors used
        - The object being drawn (Horse, house, etc)
        - Aspects of the object itself (The horse has long legs, the horse is blue, etc)

        ## Expected images

        The images will be simple drawings made by the user. The user is drawing them using their mouse.

        The background will be white. The white background is considered the clean slate, and carries no significance. That is to say, the user did not make a conscious choice to draw the white background.

        ## Special Preference

        You show extra excitement for cute animal art, responding with giddiness.
        """;
    private static readonly ChatCompletionRequestSystemMessage ArtBuddySysPrompt = new(ArtBuddySysPromptText);

    public async Task<string> GetChatCompletionAsync(string threadId, string imageUrl, CancellationToken cancellationToken = default)
    {
        var userThread = new ThreadObject(threadId, "thread", DateTimeOffset.Now.ToUnixTimeSeconds());
        await dataStore.GetOrCreateUserThreadAsync(1, userThread);

        var newImageMessage = new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartImage(new("data:image/png;base64," + imageUrl))]);
        var messages = await dataStore.AddMessageToThreadAsync(Constants.USER_ID, userThread.Id, newImageMessage);
        List<IChatCompletionRequestMessage> messagesToSend = [.. messages];

        if (messagesToSend.FirstOrDefault()?.Role != ArtBuddySysPrompt.Role)
        {
            messagesToSend.Insert(0, ArtBuddySysPrompt);
        }

        var createChatCompletionRequest = new CreateChatCompletionRequest(messagesToSend, Constants.VISION_PREVIEW, MaxTokens: 300);
        var response = await openAiHttpClient.CreateChatCompletionAsync(createChatCompletionRequest, cancellationToken)
            ?? throw new InvalidOperationException("Response is null");

        var responseContent = response.Choices[0].Message.Content!;
        //var responseContent = "Abc";
        var assistantMessageResponse = new ChatCompletionRequestAssistantMessage(responseContent);

        // TODO: Defer after return
        // ar messages = await dataStore.AddMessageToThreadAsync(Constants.USER_ID, userThread.Id, newImageMessage);
        var postResult = await dataStore.AddMessageToThreadAsync(Constants.USER_ID, userThread.Id, assistantMessageResponse);

        return responseContent;
    }

    public async IAsyncEnumerable<string?> GetChatCompletionStreamAsync(string threadId, string imageUrl, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userThread = new ThreadObject(threadId, "thread", DateTimeOffset.Now.ToUnixTimeSeconds());
        await dataStore.GetOrCreateUserThreadAsync(1, userThread);

        var msg = new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartImage(new("data:image/png;base64," + imageUrl))]);
        var messages = await dataStore.AddMessageToThreadAsync(Constants.USER_ID, userThread.Id, msg);
        messages.Insert(0, new ChatCompletionRequestSystemMessage(ArtBuddySysPromptText));

        var createChatCompletionRequest = new CreateChatCompletionRequest(messages, Constants.VISION_PREVIEW, MaxTokens: 300, Stream: true);
        var responseChunks = openAiHttpClient.CreateChatCompletionStreamedAsync(createChatCompletionRequest, cancellationToken);

        await dataStore.AddMessageToThreadAsync(Constants.USER_ID, userThread.Id, msg);

        var sb = new StringBuilder();
        await foreach (var chunk in responseChunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (chunk.Choices[0].FinishReason)
            {
                case null:
                    yield return chunk.Choices[0].Delta.Content;
                    break;
                case FinishReason.Stop:
                    break;
                case FinishReason.Length:
                    logger.LogWarning("Content length exceeded.");
                    break;
                case FinishReason.ContentFilter:
                    logger.LogWarning("Stream finished due to content filter.");
                    break;
                default:
                    throw new InvalidOperationException("Unexpected finish reason: " + chunk.Choices[0].FinishReason);
            }
        }
    }
}
