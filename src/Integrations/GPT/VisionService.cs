using System.Text.Json.Nodes;
using AIWA.API.Data;
using AIWA.API.Data.Models;
using Lli.OpenAi.Core.Client;
using Lli.OpenAi.Core.Schema.Chat;
using static Lli.OpenAi.Core.Schema.Models;

namespace AIWA.API.Integrations.GPT;

public class VisionService(IOpenAiHttpClient openAiHttpClient, IDataStore dataStore) : IVisionService
{
    private const string ArtBuddySystemPrompt = """
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

    private const string ConversationSummarizerSystemPrompt = """
        # Conversation Summarizer

        ## Role and objectives

        This GPT's role is to take a set of comments from an observer who has commented on a user's drawing, and summarize it for the purpose of image generation.

        ## Response Strategy

        The user message will be comments describing a work-in-progress image with periodic intervals. That is to say, the first comment will be a description of when the user just started creating their drawing, and the last comment will be the most recent change in the drawing.

        You will respond with a prompt intended for DALL-E such that a new image can be created based on your understanding of the user's image.

        Ensure that as many elements as possible from the commentary are included in the response.

        ## Examples

        - User: "I like the bright, yellow sun. --- Those are some majestic mountains in the background!"
        - Response: "A landscape with a bright, yellow sun. Majestic mountains are in  the background."

        - User: "That's a lovely tropical island. I would love to go there on a vacation. --- Is that a little penguin friend I see?"
        - Response: "A cute penguin on a tropical island. The island looks suitable for vacation."
        """;

    private const string ImageDescriberPrompt = """
        # Image Describer

        ## Role and objectives

        This GPT's role is to view the user's drawing and write a verbose, enthusiastic description of the image. The description will be used as an image generation prompt for DALL-E.

        ## Response Strategy

        The GPT will receive an image from the user. The GPT will then describe the image in great detail such that DALL-E can generate a version of the image that matches it as accurately as possible.

        As many aspects of the original image should be described as possible, such as which objects are present, their orientation, placement, and so on.

        ## Expected images

        The images will be simplistic drawings made using the cursor and a limited palette, similar to images made in MS Paint. It is not necessary to comment on their simplistic nature.

        The starting point for the image, before the user started drawing, is a solid white background.

        ## Example Responses

        - "A landscape with a bright, yellow sun in the upper right corner. Two majestic mountains are in the background, slightly to the left. The rightmost mountain peak is slightly taller than the other. The ground is lush and green. A heavy, gray cloud is slightly covering the sun."
        - "A cute penguin on a tropical island. The island looks suitable for vacation. The penguin is standing upright and facing to the left, with its flippers raised. There are two palm trees leaning in opposite directions."
        - "A green and purple doggo with stubby legs, facing left. It has a long neck. Its body is purple, with a striped purple pattern along its neck and lower legs. A shining sun is in the upper left."
        """;

    private static readonly ChatCompletionRequestSystemMessage ArtBuddySysPrompt = new(ArtBuddySystemPrompt);

    public async Task<string> DescribeConversationAsync(Guid iuId, CancellationToken cancellationToken = default)
    {
        var userImages = new List<InteractionUnit>();

        await foreach (var item in dataStore.GetInteractionUnitsAsync(iuId, 10, TimeSpan.FromSeconds(5), cancellationToken))
        {
            if (item.Role == "user")
            {
                userImages.Add(item);
            }
        }

        var latestVersion = userImages.OrderBy(x => x.CreatedAt).LastOrDefault();
        if (latestVersion is null)
            throw new NullReferenceException(nameof(latestVersion));

        return await DescribeImageAsync(latestVersion.Content, cancellationToken);
    }

    public async Task<string> DescribeImageAsync(string base64Image, CancellationToken cancellationToken = default)
    {
        List<IChatCompletionRequestMessage> messagesToSend =
        [
            new ChatCompletionRequestSystemMessage(ImageDescriberPrompt),
            new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartImage(new ImageUrl(base64Image))]),
        ];

        var createChatCompletionRequest = new CreateChatCompletionRequest(messagesToSend, GPT4_Vision_preview, MaxTokens: 300);
        var response = await openAiHttpClient.CreateChatCompletionAsync(createChatCompletionRequest, cancellationToken);
        return response!.Choices[0].Message.Content;
    }

    /* Describe based on conversation log
    public async Task<string> DescribeConversationAsync(Guid iuId, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        await foreach (var item in dataStore.GetInteractionUnitsAsync(iuId, 10, TimeSpan.FromSeconds(5), cancellationToken))
        {
            if (item.Role == "assistant")
            {
                sb.AppendLine(item.Content);
                sb.AppendLine("\n---\n");
            }
        }

        List<IChatCompletionRequestMessage> messagesToSend =
        [
            new ChatCompletionRequestSystemMessage(ConversationSummarizerSystemPrompt),
            new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartText(sb.ToString())]),
        ];

        var createChatCompletionRequest = new CreateChatCompletionRequest(messagesToSend, GPT35_turbo);
        var response = await openAiHttpClient.CreateChatCompletionAsync(createChatCompletionRequest, cancellationToken);
        return response!.Choices[0].Message.Content;
    }
     */

    public async Task<Uri> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = new JsonObject
        {
            ["model"] = "dall-e-3",
            ["prompt"] = prompt,
            ["n"] = 1,
            ["size"] = "1024x1024"
        };

        return await openAiHttpClient.GenerateImageAsync(request, cancellationToken);
    }

    public async Task<string> GetChatCompletionAsync(Guid userId, string imageUrl, CancellationToken cancellationToken = default)
    {
        if (await dataStore.GetUserOrDefaultAsync(userId, cancellationToken) is not AiwaUser user)
        {
            user = new AiwaUser
            {
                Id = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                Name = "placeholder",
            };

            await dataStore.AddUserAsync(user, cancellationToken);
        }

        var newIuGuid = Guid.NewGuid();
        InteractionUnit iu;

        if (await dataStore.GetLastUserInteractionUnitOrDefaultAsync(userId, cancellationToken) is InteractionUnit parentMessage)
        {
            // Attach new unit to parent
            iu = new InteractionUnit
            {
                Id = newIuGuid,
                ParentId = parentMessage.Id,
                Parent = parentMessage,
                UserId = user.Id,
                User = user,
                CreatedAt = DateTimeOffset.UtcNow,
                Role = "user",
                Content = imageUrl
            };
        }
        else
        {
            // Start new root
            iu = new InteractionUnit
            {
                Id = newIuGuid,
                ParentId = newIuGuid,
                UserId = user.Id,
                User = user,
                CreatedAt = DateTimeOffset.UtcNow,
                Role = "user",
                Content = imageUrl
            };
        }

        await dataStore.AddInteractionUnitsAsync([iu], cancellationToken);

        List<IChatCompletionRequestMessage> messagesToSend = [];
        await foreach (var item in dataStore.GetInteractionUnitsAsync(iu.Id, 10, TimeSpan.FromSeconds(5), cancellationToken))
        {
            messagesToSend.Insert(0, item.ToChatCompletionRequestMessage());
        }

        if (messagesToSend.FirstOrDefault()?.Role != ArtBuddySysPrompt.Role)
        {
            messagesToSend.Insert(0, ArtBuddySysPrompt);
        }

        var createChatCompletionRequest = new CreateChatCompletionRequest(messagesToSend, Constants.VISION_PREVIEW, MaxTokens: 300);
        var response = await openAiHttpClient.CreateChatCompletionAsync(createChatCompletionRequest, cancellationToken)
            ?? throw new InvalidOperationException("Response is null");

        var responseContent = response.Choices[0].Message.Content!;

        var newIu = new InteractionUnit
        {
            Id = Guid.NewGuid(),
            Content = response.Choices[0].Message.Content!,
            CreatedAt = DateTimeOffset.UtcNow,
            Parent = iu,
            ParentId = iu.Id,
            Role = response.Choices[0].Message.Role,
            User = user,
            UserId = user.Id
        };

        await dataStore.AddInteractionUnitsAsync([newIu], cancellationToken);

        return responseContent;
    }
}
