
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AIWA.API.Data;
using AIWA.API.Integrations.GPT;
using Lli.OpenAi.Core.Assistants;
using Lli.OpenAi.Core.Client;
using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Files;
using Lli.OpenAi.Core.Schema.Thread;
using Lli.OpenAi.Core.Serialization;
using Lli.OpenAi.Core.Speech;
using static Lli.OpenAi.Core.Schema.Models;

namespace AIWA.API;

public class DebugHostedService(ILogger<DebugHostedService> logger, IOpenAiHttpClient openAiHttpClient, IServiceScopeFactory scopeFactory) : IHostedService
{
    private const int UserId = 1;
    private const string AssistantId = "asst_PxpeQ8LGRMBElMunkzisUgnf";
    private const string ThreadId = "thread_SG5TMFR7LEt2dIUlTlVlJg1g";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // await DeleteAllFilesAsync(cancellationToken);
        // await DeleteAllThreadsAsync(cancellationToken);

        // await TestChatCompletionImageAsync(cancellationToken);
        // await TestChatCompletion(cancellationToken);
        // await TestAssistantAsync(cancellationToken);
        // await PrintAllThreadMessagesAsync(cancellationToken);
        // await TestGitProcessAsync(cancellationToken);
        // await TestDataStore(cancellationToken);
        // var userId = Guid.NewGuid();
        var userId = Guid.Parse("04fa2b03-9d06-4655-ace0-41e1e07ec841");

        //await TestVisionServiceAsync(userId, "userImage-1.png", cancellationToken);
        //await TestVisionServiceAsync(userId, "userImage-2.png", cancellationToken);
        //await TestVisionServiceAsync(userId, "userImage-3.png", cancellationToken);
        //await TestImageGenerationAsync("A cozy island with a palm tree", cancellationToken);
        using var scope = scopeFactory.CreateAsyncScope();
        var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();
        var iu = await dataStore.GetLastUserInteractionUnitOrDefaultAsync(userId, cancellationToken);
        await TestImageDescriptionAsync(iu!.Id, cancellationToken);
        //await TestSpeechServiceAsync(cancellationToken);

    }

    private async Task TestImageDescriptionAsync(Guid iuId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateAsyncScope();
        var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();
        var speechService = scope.ServiceProvider.GetRequiredService<ISpeechService>();
        var visionService = scope.ServiceProvider.GetRequiredService<IVisionService>();
        var description = await visionService.DescribeConversationAsync(iuId, cancellationToken);
        logger.LogInformation("\tResponse:\n\t{RESULT}", description);
        var uri = await visionService.GenerateImageAsync(description, cancellationToken);
        logger.LogInformation("\tResponse:\n\t{RESULT}", uri);
    }

    private async Task TestVisionServiceAsync(Guid userId, string fileName, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateAsyncScope();
        var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();
        var speechService = scope.ServiceProvider.GetRequiredService<ISpeechService>();
        var visionService = scope.ServiceProvider.GetRequiredService<IVisionService>();

        var path = GetFilePath(fileName);
        var base64Image = GetBase64StringForImage(path);

        var result = await visionService.GetChatCompletionAsync(userId, base64Image, cancellationToken);
        logger.LogInformation("\tResponse:\n\t{RESULT}", result);
    }

    private async Task TestImageGenerationAsync(string prompt, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateAsyncScope();
        var visionService = scope.ServiceProvider.GetRequiredService<IVisionService>();
        var result = await visionService.GenerateImageAsync(prompt, cancellationToken);
        logger.LogInformation("\tResponse:\n\t{RESULT}", result);
    }

    private static async Task TestGitProcessAsync(CancellationToken cancellationToken)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            FileName = "CMD.exe",
            Arguments = @"/c git status"
            //Arguments = @"/c cd d:\GIT\proj1\ & git pull master",
        };

        var process = System.Diagnostics.Process.Start(startInfo);
        var output = await process!.StandardOutput.ReadToEndAsync(cancellationToken);
        Console.WriteLine(output);

        await process.WaitForExitAsync(cancellationToken);
    }

    private async Task DeleteAllFilesAsync(CancellationToken cancellationToken)
    {
        bool hasMore = true;
        var pagination = new PaginationParameters(Limit: 10);

        while (hasMore)
        {
            var files = await openAiHttpClient.ListFilesAsync(pagination: pagination, cancellationToken: cancellationToken);
            foreach (var fileObject in files.Data)
            {
                var dres = await openAiHttpClient.DeleteFileAsync(fileObject.Id, cancellationToken);
                if (dres.Deleted)
                    logger.LogInformation("Deleted file {fileObject_Id}", fileObject.Id);
                else
                    logger.LogWarning("Could not delete file {fileObject_Id}", fileObject.Id);
            }

            hasMore = files.Data.Count > 0;
        }
    }

    private async Task DeleteAllThreadsAsync(CancellationToken cancellationToken)
    {
        bool hasMore = true;
        var pagination = new PaginationParameters(Limit: 10);

        while (hasMore)
        {
            var threads = await openAiHttpClient.GetThreadsAsync(pagination, cancellationToken);
            foreach (var threadObject in threads.Data)
            {
                var dres = await openAiHttpClient.DeleteThreadAsync(threadObject.Id, cancellationToken);
                if (dres.Deleted)
                    logger.LogInformation("Deleted thread {thread_Id}", threadObject.Id);
                else
                    logger.LogWarning("Could not delete thread {thread_Id}", threadObject.Id);
            }

            hasMore = threads.Data.Count > 0;
        }

        logger.LogInformation("Deleted all threads");
    }

    private async Task PrintAllThreadMessagesAsync(CancellationToken cancellationToken)
    {
        bool hasMore = true;
        var pagination = new PaginationParameters(Limit: 10);

        while (hasMore)
        {
            var threads = await openAiHttpClient.GetThreadsAsync(pagination, cancellationToken);
            foreach (var threadObject in threads.Data)
            {
                await PrintThreadMessagesAsync(threadObject.Id, cancellationToken);
            }

            hasMore = threads.Data.Count > 0;
        }

        logger.LogInformation("Done. Idling...");
        await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
    }

    private async Task TestAssistantAsync(CancellationToken cancellationToken)
    {
        string fileId;
        //string threadId;
        //string message0Id;
        //string message1Id;

        //fileId = "file-XRIQ53XwUfTcxS9rKlJDq2NU";
        //threadId = "thread_ZSdRbLYQZuFusqbOkIAxsxS4";
        //message0Id = "msg_teK98ahhukWczZAP7aYt1avo";
        //message1Id = "msg_my6xeLF7TIRJaoFxIzKqygVm";

        using var scope = scopeFactory.CreateAsyncScope();
        var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();

        // Create thread
        var fileName = "userImage-2.png";
        var path = GetFilePath(fileName);

        using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
        {
            var uploadFileResult = await openAiHttpClient.UploadFileAsync(fs, fileName, cancellationToken);
            fileId = uploadFileResult.Id;
        }

        var cmr = new CreateMessageRequest("user", "Here is the user's image", [fileId]);
        var threadCreateResult = await openAiHttpClient.CreateThreadAsync([cmr], cancellationToken);
        logger.LogInformation("Created thread at {uri}", string.Concat("https://platform.openai.com/playground?mode=assistant&thread=", threadCreateResult.Id));
        //var addThreadToUserTask = dataStore.AddThreadToUserAsync(UserId, threadCreateResult);

        // var attachResult = await _openAiHttpClient.AttachFileToAssistantAsync(AssistantId, fileId, cancellationToken);
        // var mcr = new MessageCreationRequest("user", "Hi! What's the color of this creature?", [fileId]);
        // var threadMessageCreateResult = await _openAiHttpClient.CreateThreadMessageAsync(ThreadId, mcr, cancellationToken);

        /* Run thread */
        var threadRunResult = await openAiHttpClient.RunThreadAsync(threadCreateResult.Id, AssistantId, cancellationToken);
        //await addThreadToUserTask;

        var threadId = threadRunResult.ThreadId;
        bool isComplete = false;
        bool isFailed = false;

        while (!isComplete)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            logger.LogInformation("Checking run status");
            var threadRun = await openAiHttpClient.GetThreadRunAsync(threadRunResult.ThreadId, threadRunResult.Id, cancellationToken);
            isComplete = string.CompareOrdinal("completed", threadRun.Status) == 0;
            isFailed = string.CompareOrdinal("failed", threadRun.Status) == 0;
            logger.LogInformation("Status is {threadRun_Status}", threadRun.Status);
            if (isFailed)
            {
                throw new InvalidOperationException($"Error: {threadRun}");
            }
        }

        await PrintThreadMessagesAsync(threadId, cancellationToken);
        /**/

        //var pagination = new PaginationParameters { After = message1Id };
        //string threadId = "thread_YdrTPifJsmb0UD2GLSTYcRW2";

        //await foreach (var thread in dataStore.GetUserThreadsAsync(UserId, cancellationToken))
        //{
        //    threadId = thread.Id;
        //    break;
        //}

        logger.LogInformation("Done. Idling...");
        await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
    }
    private async Task PrintThreadMessagesAsync(string threadId, CancellationToken cancellationToken)
    {
        var pagination = new PaginationParameters { Limit = 10, Order = "asc" };
        var newMessages = await openAiHttpClient.GetThreadMessagesAsync(threadId, pagination, cancellationToken);

        foreach (var message in newMessages.Data)
        {
            foreach (var messageContent in message.Content)
            {
                if (messageContent is MessageContentTextObject contentTextObject)
                {
                    logger.LogInformation("{role}:\n{text_value}", message.Role, contentTextObject.Text.Value);
                }
                else if (messageContent is MessageContentImageFileObject contentImageFileObject)
                {
                    logger.LogInformation("file id: {file_id}", contentImageFileObject.ImageFile.FileId);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected message type");
                }
            }
        }
    }

    private static string GetFilePath(string fileName)
    {
        // "C:\code\projects\aiwa\userimages\userimage-2.png"
        // var fileName = string.Concat("test05", ".", responseFormat.ToString()!.ToLowerInvariant());
        // return Path.Join("C:", "code", "projects", "openai-tts", "outputs", fileName);

        return Path.Join("C:", "code", "projects", "aiwa", "userimages", fileName);
    }

    private async Task TestChatCompletionAsync(CancellationToken cancellationToken)
    {
        var content = new List<ChatCompletionRequestMessageContentPartText>
        {
            new("Hi! This is a test message I'm using while I'm integrating with you via API. Please verify this message by including the name of a random member of the japanese band 'Perfume' in your response. Go ahead and respond with a longer message so that I can test my chunked streaming functionality.")
        };

        var messages = new List<IChatCompletionRequestMessage>
        {
            new ChatCompletionRequestUserMessage(content)
        };

        var createChatCompletionRequest = new CreateChatCompletionRequest(messages, GPT4_1106_Preview, Stream: true);
        // var result = await _openAiHttpClient.CreateChatCompletionAsync(createChatCompletionRequest, cancellationToken);
        var res = openAiHttpClient.CreateChatCompletionStreamedAsync(createChatCompletionRequest, cancellationToken);
        await foreach (var item in res)
        {
            Console.Write(item.Choices[0].Delta.Content);
        }

        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = {
            new ModelConverter(),
            new VoiceConverter(),
            new ResponseFormatConverter(),
            new JsonStringEnumConverter(),
            new OpenAIFileConverter(),
            new AssistantFileConverter(),
            new FinishReasonConverter(),
            new AssistantToolsConverter(),
            new ThreadObjectConverter(),
            new RunObjectConverter(),
            new CreateMessageRequestConverter(),
            new CreateThreadRequestConverter(),
            new MessageObjectConverter(),
            new MessageContentObjectsConverter(),
            new CreateChatCompletionRequestConverter(),
            new ChatCompletionRequestMessageContentPartConverter(),
            new ChatCompletionRequestMessageConverter(),
        }
    };

    private const string ArtBuddySysPrompt = """
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

    private async Task TestSpeechServiceAsync(CancellationToken cancellationToken)
    {
        var format = OutputFormat.Mp3;
        var filePath = Path.Join
        (
            "C:",
            "code",
            "projects",
            "aiwa",
            "outputs",
            $"speech-service-test-{DateTimeOffset.Now.ToUnixTimeSeconds()}.{ResponseFormatConverter.ToFileExtension(format)}"
        );

        using var scope = scopeFactory.CreateAsyncScope();
        var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();
        var speechService = scope.ServiceProvider.GetRequiredService<ISpeechService>();

        using var stream = await speechService.GetTtsStreamAsync("This is a test from the speech service.", outputFormat: format, cancellationToken: cancellationToken);
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        await stream.CopyToAsync(fileStream, cancellationToken);

    }

    //private async Task TestVisionServiceAsync(CancellationToken cancellationToken)
    //{
    //    var fileName2 = "userImage-2.png";
    //    var path2 = GetFilePath(fileName2);
    //    var base64Image2 = GetBase64StringForImage(path2);

    //    //var fileName3 = "userImage-3.png";
    //    //var path3 = GetFilePath(fileName3);
    //    //var base64Image3 = GetBase64StringForImage(path3);

    //    using var scope = _scopeFactory.CreateAsyncScope();
    //    var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();
    //    var visionService = scope.ServiceProvider.GetRequiredService<IVisionService>();

    //    var res = visionService.GetChatCompletionStreamAsync("thread_id", base64Image2, cancellationToken);
    //    await foreach (var item in res)
    //    {
    //        Console.Write(item);
    //    }
    //}

    private async Task TestChatCompletionImageAsync(CancellationToken cancellationToken)
    {
        var fileName2 = "userImage-2.png";
        var path2 = GetFilePath(fileName2);
        var base64Image2 = GetBase64StringForImage(path2);

        var fileName3 = "userImage-3.png";
        var path3 = GetFilePath(fileName3);
        var base64Image3 = GetBase64StringForImage(path3);

        var content2 = new List<IChatCompletionRequestMessageContentPart>
        {
            //new ChatCompletionRequestMessageContentPartText("What's in this image?"),
            new ChatCompletionRequestMessageContentPartImage
            (
                new ImageUrl("data:image/png;base64," + base64Image2)
            )
        };

        var content3 = new List<IChatCompletionRequestMessageContentPart>
        {
            new ChatCompletionRequestMessageContentPartImage
            (
                new ImageUrl("data:image/png;base64," + base64Image3)
            )
        };

        var messages = new List<IChatCompletionRequestMessage>
        {
            new ChatCompletionRequestSystemMessage(ArtBuddySysPrompt),
            new ChatCompletionRequestUserMessage(content2),
            new ChatCompletionRequestAssistantMessage("It's a little green dino! Or maybe a horse?.."),
            new ChatCompletionRequestUserMessage(content3)
        };

        var createChatCompletionRequest = new CreateChatCompletionRequest(messages, Constants.VISION_PREVIEW, MaxTokens: 300, Stream: true);

        //var json = JsonSerializer.Serialize(createChatCompletionRequest, _jsonSerializerOptions);
        // var result = await _openAiHttpClient.CreateChatCompletionAsync(createChatCompletionRequest, cancellationToken);
        //var res = _openAiHttpClient.CreateChatCompletionStreamedAsync(createChatCompletionRequest, cancellationToken);
        var res = openAiHttpClient.CreateChatCompletionStreamedAsync(createChatCompletionRequest, cancellationToken);
        await foreach (var item in res)
        {
            if (item.Choices[0].FinishReason == FinishReason.Length)
            {
                Console.WriteLine("Oops!");
            }
            else
            {
                Console.Write(item.Choices[0].Delta.Content);
            }
        }

        Console.WriteLine();
        logger.LogInformation("Done. Idling...");
        await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private static string GetBase64StringForImage(string path)
    {
        using FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read);
        return Base64Writer.GetBase64String(fs);
    }
}
