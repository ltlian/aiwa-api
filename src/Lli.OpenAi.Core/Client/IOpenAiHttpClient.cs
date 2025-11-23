using System.Text.Json.Nodes;

using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Files;
using Lli.OpenAi.Core.Schema.Thread;
using Lli.OpenAi.Core.Speech;

namespace Lli.OpenAi.Core.Client;

public interface IOpenAiHttpClient
{
    Task<CreateChatCompletionResponse?> CreateChatCompletionAsync(CreateChatCompletionRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<CreateChatCompletionStreamResponse> CreateChatCompletionStreamedAsync(CreateChatCompletionRequest request, CancellationToken cancellationToken);
    Task<ThreadObject> CreateThreadAsync(IEnumerable<CreateMessageRequest>? messages = null, CancellationToken cancellationToken = default);
    Task<DeletionStatus> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);
    Task<DeletionStatus> DeleteThreadAsync(string threadId, CancellationToken cancellationToken = default);
    Task<Uri> GenerateImageAsync(JsonObject keyValuePairs, CancellationToken cancellationToken = default);
    Task<ResponseList<MessageObject>> GetThreadMessagesAsync(string threadId, PaginationParameters? paginationParameters = null, CancellationToken cancellationToken = default);
    Task<RunObject> GetThreadRunAsync(string threadId, string runId, CancellationToken cancellationToken);
    Task<ResponseList<ThreadObject>> GetThreadsAsync(PaginationParameters? pagination = null, CancellationToken cancellationToken = default);
    Task<Stream> GetTtsStreamAsync(string input, OutputFormat outputFormat, CancellationToken cancellationToken = default);
    Task<ResponseList<OpenAIFile>> ListFilesAsync(string? purpose = null, PaginationParameters? pagination = null, CancellationToken cancellationToken = default);
    Task<RunObject> RunThreadAsync(string threadId, string assistantId, CancellationToken cancellationToken);
    Task<OpenAIFile> UploadFileAsync(Stream stream, string fileName, CancellationToken cancellationToken);
}