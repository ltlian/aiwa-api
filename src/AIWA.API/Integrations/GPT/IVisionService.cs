namespace AIWA.API.Integrations.GPT;

public interface IVisionService
{
    Task<string> DescribeConversationAsync(Guid iuId, CancellationToken cancellationToken = default);
    Task<string> DescribeImageAsync(string base64Image, CancellationToken cancellationToken = default);
    Task<Uri> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> GetChatCompletionAsync(Guid userId, string imageUrl, CancellationToken cancellationToken = default);
}
