using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

using Lli.OpenAi.Core.Assistants;
using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Files;
using Lli.OpenAi.Core.Schema.Thread;
using Lli.OpenAi.Core.Serialization;
using Lli.OpenAi.Core.Speech;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lli.OpenAi.Core.Client;

public class OpenAiHttpClient(HttpClient httpClient, IOptions<OpenAIHttpClientOptions> options, ILogger<OpenAiHttpClient> logger) : IOpenAiHttpClient
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = OpenAIJsonSerializerOptionsFactory.GetJsonSerializerOptions();

    private readonly AuthenticationHeaderValue _authenticationHeader = new("Bearer", options.Value.OpenAiKey);

    #region Chat completions

    public async Task<CreateChatCompletionResponse?> CreateChatCompletionAsync(CreateChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        using var content = JsonContent.Create(request, null, _jsonSerializerOptions);
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Endpoints.ChatCompletions)
        {
            Content = content,
            Headers = { Authorization = _authenticationHeader }
        };

        using var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
        logger.LogInformation("Sent request to {ENDPOINT_URI}", Endpoints.ChatCompletions);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CreateChatCompletionResponse>(_jsonSerializerOptions, cancellationToken);
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async IAsyncEnumerable<CreateChatCompletionStreamResponse> CreateChatCompletionStreamedAsync(CreateChatCompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Endpoints.ChatCompletions)
        {
            Headers = { Authorization = _authenticationHeader },
            Content = JsonContent.Create(request, options: _jsonSerializerOptions)
        };

        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await foreach (var responseChunk in Utf8JsonReaderHelpers.DeserializeChunkedAsync<CreateChatCompletionStreamResponse>(stream, _jsonSerializerOptions, cancellationToken))
            {
                yield return responseChunk;
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    #endregion

    #region TTS

    public Task<HttpContent> GetTtsAsync(string input, CancellationToken cancellationToken = default)
    {
        var requestData = new CreateSpeechRequest(TtsModel.Tts1, input, Voice.Nova, OutputFormat.Mp3);
        return GetTtsAsync(requestData, cancellationToken);
    }

    public async Task<HttpContent> GetTtsAsync(CreateSpeechRequest requestData, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending request to {ENDPOINT}", Endpoints.Speech);

        using var request = CreatePOSTRequest(Endpoints.Speech, _authenticationHeader, requestData);
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return response.Content;
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public Task<Stream> GetTtsStreamAsync(string input, OutputFormat outputFormat, CancellationToken cancellationToken = default)
    {
        var requestData = new CreateSpeechRequest(TtsModel.Tts1, input, Voice.Nova, outputFormat);
        return GetTtsStreamAsync(requestData, cancellationToken);
    }

    public async Task<Stream> GetTtsStreamAsync(CreateSpeechRequest requestData, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending request to {ENDPOINT}", Endpoints.Speech);

        using var request = CreatePOSTRequest(Endpoints.Speech, _authenticationHeader, requestData);
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        logger.LogInformation("Received response");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync(cancellationToken);
            //return response.Content.ReadAsStream(cancellationToken);
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    #endregion

    #region Files

    // https://api.openai.com/v1/files

    public async Task<ResponseList<OpenAIFile>> ListFilesAsync(string? purpose = null, PaginationParameters? pagination = null, CancellationToken cancellationToken = default)
    {
        var b = new UriBuilder(Endpoints.Files);
        if (!string.IsNullOrEmpty(purpose))
            b.Query = "&purpose=" + purpose;

        using var request = CreateGETRequest(b.Uri, _authenticationHeader, pagination);
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ResponseList<OpenAIFile>>(_jsonSerializerOptions, cancellationToken);
            if (result is not null)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<OpenAIFile> UploadFileAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent
        {
            { new StreamContent(stream), "file", fileName },
            { new StringContent("assistants"), "purpose"}
        };
        using var request = CreatePOSTRequest(Endpoints.Files, _authenticationHeader, content);
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OpenAIFile>(_jsonSerializerOptions, cancellationToken);
            if (result is not null)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<DeletionStatus> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        using var request = CreateDeleteRequest(Endpoints.File(fileId), _authenticationHeader);
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<DeletionStatus>(_jsonSerializerOptions, cancellationToken);
            if (result is not null)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    #endregion

    #region Image generation
    public async Task<Uri> GenerateImageAsync(JsonObject keyValuePairs, CancellationToken cancellationToken = default)
    {
        using var jsonContent = JsonContent.Create(keyValuePairs);
        using var request = CreatePOSTRequest(Endpoints.ImageGenerations, _authenticationHeader, jsonContent);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonObject = JsonObject.Parse(jsonString);
            if (jsonObject?["data"]?.AsArray()[0]?["url"] is { } urlNode)
            {
                return new Uri(urlNode.ToString());
            }

            throw new InvalidOperationException("Did not find image uri in response.");
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }
    #endregion

    #region Assistants

    public async Task<AssistantFile> AttachFileToAssistantAsync(string assistantId, string fileId, CancellationToken cancellationToken)
    {
        using var content = JsonContent.Create(new Dictionary<string, string>
        {
            { "file_id", fileId }
        });
        using var request = CreatePOSTRequest(Endpoints.AssistantFiles(assistantId), _authenticationHeader, content);
        AddBetaHeader(request);
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var assistantFile = await response.Content.ReadFromJsonAsync<AssistantFile>(_jsonSerializerOptions, cancellationToken);
            if (assistantFile is not null)
            {
                var createdAt = new DateTime(assistantFile.CreatedAt);
                logger.LogInformation("Assistant file attached at {CREATED_AT}", createdAt);
                return assistantFile;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<ResponseList<ThreadObject>> GetThreadsAsync(PaginationParameters? pagination = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateBrowserRequest(Endpoints.Threads, pagination);
        AddBetaHeader(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var threads = await response.Content.ReadFromJsonAsync<ResponseList<ThreadObject>>(_jsonSerializerOptions, cancellationToken);
            if (threads is not null)
            {
                return threads;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<ThreadObject> CreateThreadAsync(IEnumerable<CreateMessageRequest>? messages = null, CancellationToken cancellationToken = default)
    {
        using var request = CreatePOSTRequest(Endpoints.Threads, _authenticationHeader, new CreateThreadRequest(messages?.ToList()));
        AddBetaHeader(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var threadObject = await response.Content.ReadFromJsonAsync<ThreadObject>(_jsonSerializerOptions, cancellationToken);
            if (threadObject is not null)
            {
                var createdAt = new DateTime(threadObject.CreatedAt);
                logger.LogInformation("Assistant file attached at {CREATED_AT}", createdAt);
                return threadObject;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<DeletionStatus> DeleteThreadAsync(string threadId, CancellationToken cancellationToken = default)
    {
        using var request = CreateDeleteRequest(Endpoints.Thread(threadId), _authenticationHeader);
        AddBetaHeader(request);
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<DeletionStatus>(_jsonSerializerOptions, cancellationToken);
            if (result is not null)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<RunObject> RunThreadAsync(string threadId, string assistantId, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, string>
        {
            {"assistant_id", assistantId}
        };

        using var request = CreatePOSTRequest(Endpoints.ThreadRuns(threadId), _authenticationHeader, JsonContent.Create(body, options: _jsonSerializerOptions));
        AddBetaHeader(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var runObject = await response.Content.ReadFromJsonAsync<RunObject>(_jsonSerializerOptions, cancellationToken);
            if (runObject is not null)
            {
                return runObject;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<RunObject> GetThreadRunAsync(string threadId, string runId, CancellationToken cancellationToken)
    {
        using var request = CreateGETRequest(Endpoints.ThreadRun(threadId, runId), _authenticationHeader);
        AddBetaHeader(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var runObject = await response.Content.ReadFromJsonAsync<RunObject>(_jsonSerializerOptions, cancellationToken);
            if (runObject is not null)
            {
                return runObject;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<string> CreateThreadMessageAsync(string threadId, CreateMessageRequest messageCreationRequest, CancellationToken cancellationToken)
    {
        using var request = CreatePOSTRequest(Endpoints.ThreadMessages(threadId), _authenticationHeader, messageCreationRequest);
        AddBetaHeader(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            if (responseJson is not null)
            {
                // var createdAt = new DateTime(responseJson.CreatedAt);
                // logger.LogInformation("Assistant file attached at {CREATED_AT}", createdAt);
                return responseJson;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    public async Task<ResponseList<MessageObject>> GetThreadMessagesAsync(string threadId, PaginationParameters? paginationParameters = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateGETRequest(Endpoints.ThreadMessages(threadId), _authenticationHeader, paginationParameters);
        AddBetaHeader(request);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadFromJsonAsync<ResponseList<MessageObject>>(_jsonSerializerOptions, cancellationToken);
            if (responseData is not null)
            {
                return responseData;
            }
            else
            {
                throw new InvalidOperationException("Could not read result from content stream.");
            }
        }
        else
        {
            await LogJsonBodyAsync(response, cancellationToken);
            throw new InvalidOperationException($"Status code is {response.StatusCode}");
        }
    }

    private static void AddBetaHeader(HttpRequestMessage request) => request.Headers.Add("OpenAI-Beta", "assistants=v1");
    private static void AddOrgHeader(HttpRequestMessage request) => request.Headers.Add("OpenAI-Organization", "org-zyEvlAGXBPSf6qCy9U4CXzRc");
    private static void AddBrowserBearerTokenHeader(HttpRequestMessage request) => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "sess-dj4Ca9LqtrtEwE7wo95UiWM7BoAxVy3NIy7icpA6");

    #endregion

    private async Task LogJsonBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        logger.LogError("Error: {STATUS_CODE}", response.StatusCode);
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogError("Error: {JSON}", jsonString);
    }

    //private static async Task SaveFile(string filePath, HttpContent content, CancellationToken cancellationToken)
    //{
    //    using var stream = await content.ReadAsStreamAsync(cancellationToken);
    //    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
    //    await stream.CopyToAsync(fileStream, cancellationToken);
    //}

    private static HttpRequestMessage CreateGETRequest(Uri url, AuthenticationHeaderValue authenticationHeader, PaginationParameters? pagination = null)
    {
        var queryString = pagination != null ? ConstructQueryString(pagination) : string.Empty;
        var builder = new UriBuilder(url)
        {
            Query = queryString
        };

        return new(HttpMethod.Get, builder.Uri)
        {
            Headers =
            {
                Authorization = authenticationHeader
            },
        };
    }

    private static HttpRequestMessage CreateBrowserRequest(Uri url, PaginationParameters? pagination = null)
    {
        var queryString = pagination != null ? ConstructQueryString(pagination) : string.Empty;
        var builder = new UriBuilder(url)
        {
            Query = queryString
        };

        var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
        AddBrowserBearerTokenHeader(request);
        AddOrgHeader(request);
        return request;
    }

    private static HttpRequestMessage CreatePOSTRequest(Uri url, AuthenticationHeaderValue authenticationHeader, HttpContent? httpContent = null) => new(HttpMethod.Post, url)
    {
        Content = httpContent,
        Headers =
        {
            Authorization = authenticationHeader
        },
    };

    private static HttpRequestMessage CreateDeleteRequest(Uri url, AuthenticationHeaderValue authenticationHeader) => new(HttpMethod.Delete, url)
    {
        Headers =
        {
            Authorization = authenticationHeader
        },
    };

    private static HttpRequestMessage CreatePOSTRequest<T>(Uri url, AuthenticationHeaderValue authenticationHeader, T requestData) where T : IOpenAiRequest =>
        CreatePOSTRequest(url, authenticationHeader, JsonContent.Create(requestData, options: _jsonSerializerOptions));

    private static string ConstructQueryString(PaginationParameters parameters)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);

        if (parameters.Limit.HasValue)
            query["limit"] = parameters.Limit.Value.ToString();

        if (!string.IsNullOrEmpty(parameters.Order))
            query["order"] = parameters.Order;

        if (!string.IsNullOrEmpty(parameters.After))
            query["after"] = parameters.After;

        if (!string.IsNullOrEmpty(parameters.Before))
            query["before"] = parameters.Before;

        return query.ToString()!;
    }

    private static class Endpoints
    {
        internal static Uri ChatCompletions = new("https://api.openai.com/v1/chat/completions");
        internal static Uri ImageGenerations = new("https://api.openai.com/v1/images/generations");
        internal static Uri Speech = new("https://api.openai.com/v1/audio/speech");
        internal static Uri Files = new("https://api.openai.com/v1/files");
        internal static Uri File(string fileId) => !string.IsNullOrWhiteSpace(fileId)
            ? new(string.Format("https://api.openai.com/v1/files/{0}", fileId))
            : throw new ArgumentException($"'{nameof(fileId)}' cannot be null or whitespace.", nameof(fileId));
        internal static Uri AssistantFiles(string assistantId) => !string.IsNullOrWhiteSpace(assistantId)
            ? new(string.Format("https://api.openai.com/v1/assistants/{0}/files", assistantId))
            : throw new ArgumentException($"'{nameof(assistantId)}' cannot be null or whitespace.", nameof(assistantId));
        internal static Uri Threads = new("https://api.openai.com/v1/threads");
        internal static Uri Thread(string threadId) => !string.IsNullOrWhiteSpace(threadId)
            ? new(string.Format("https://api.openai.com/v1/threads/{0}", threadId))
            : throw new ArgumentException($"'{nameof(threadId)}' cannot be null or whitespace.", nameof(threadId));
        internal static Uri ThreadRuns(string threadId) => !string.IsNullOrWhiteSpace(threadId)
            ? new(string.Format("https://api.openai.com/v1/threads/{0}/runs", threadId))
            : throw new ArgumentException($"'{nameof(threadId)}' cannot be null or whitespace.", nameof(threadId));
        internal static Uri ThreadRun(string threadId, string runId)
        {
            if (string.IsNullOrWhiteSpace(runId))
            {
                throw new ArgumentException($"'{nameof(runId)}' cannot be null or whitespace.", nameof(runId));
            }
            else if (string.IsNullOrWhiteSpace(threadId))
            {
                throw new ArgumentException($"'{nameof(threadId)}' cannot be null or whitespace.", nameof(threadId));
            }
            else
            {
                return new(string.Format("https://api.openai.com/v1/threads/{0}/runs/{1}", threadId, runId));
            }
        }
        internal static Uri ThreadMessages(string threadId) => !string.IsNullOrWhiteSpace(threadId)
            ? new(string.Format("https://api.openai.com/v1/threads/{0}/messages", threadId))
            : throw new ArgumentException($"'{nameof(threadId)}' cannot be null or whitespace.", nameof(threadId));
    }
}
