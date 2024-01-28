using Lli.OpenAi.Core.Client;
using Lli.OpenAi.Core.Speech;

namespace AIWA.API.Integrations.GPT;

public class SpeechService(IOpenAiHttpClient openAiHttpClient) : ISpeechService
{
    public Task<Stream> GetTtsStreamAsync(string content, Voice voice = Voice.Nova, OutputFormat outputFormat = OutputFormat.Mp3, CancellationToken cancellationToken = default)
    {
        return openAiHttpClient.GetTtsStreamAsync(content, outputFormat, cancellationToken);

        /*
        using var stream = await openAiHttpClient.GetTtsStreamAsync(content);

        // Buffer to hold data chunks
        byte[] buffer = new byte[4096]; // Adjust the size of the buffer as needed
        int bytesRead;
        int chunksSent = 0;

        // var filePath = Path.Join("C:", "code", "projects", "openai-tts", "outputs", userImage.FileName);
        // await WriteStreamToFileAsync(userImage.OpenReadStream(), filePath);
        // logger.LogInformation("Saved file to {FILE_PATH}", filePath);
        // var threadId = "";
        //await HttpContext.Response.Body.WriteAsync(buffer, 0, bytesRead);

        return new StreamingAudioResult(stream);
        /**/
    }
}
