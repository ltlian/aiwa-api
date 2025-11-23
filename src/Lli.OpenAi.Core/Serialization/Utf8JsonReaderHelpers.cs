using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Lli.OpenAi.Core.Serialization;

public static class Utf8JsonReaderHelpers
{
    public static readonly byte[] TypeDiscriminator = Encoding.UTF8.GetBytes("type");
    public static readonly byte[] RoleDiscriminator = Encoding.UTF8.GetBytes("role");

    public static void EnsureDiscriminatorProperty(ref Utf8JsonReader reader, byte[] discriminatorValue)
    {
        if (reader.TokenType != JsonTokenType.PropertyName || !reader.ValueTextEquals(discriminatorValue))
        {
            throw new JsonException("Expected 'type' property.");
        }
    }

    public static NotImplementedException NewDeserializingRequestsPayloadsNotSupportedException() =>
        new("Deserializing request payloads is not supported");

    public static NotImplementedException NewSerializingResponsePayloadsNotSupportedException() =>
        new("Serializing response payloads is not supported");

    public static async IAsyncEnumerable<T> DeserializeChunkedAsync<T>(Stream stream, JsonSerializerOptions jsonSerializerOptions, [EnumeratorCancellation] CancellationToken cancellationToken) where T : class
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (await reader.ReadLineAsync(cancellationToken) is { } chunk)
        {
            if (chunk.Length == 0)
                continue;

            var result = HandleSSE<T>(chunk[6..], jsonSerializerOptions);
            switch (result.ResultKind)
            {
                case SseResultKind.Ok:
                    yield return result.Value!;
                    break;
                case SseResultKind.Done:
                    yield break;
                default:
                    throw new JsonException("Unexpected state");
            }
        }
    }

    private static SseResult<T> HandleSSE<T>(string chunk, JsonSerializerOptions jsonSerializerOptions) where T : class
    {
        if (chunk[0] == '{')
        {
            var data = JsonSerializer.Deserialize<T>(chunk, jsonSerializerOptions);
            return new SseResult<T>(SseResultKind.Ok, data);
        }

        if (string.Equals(chunk, "[DONE]", StringComparison.Ordinal))
            return new SseResult<T>(SseResultKind.Done);

        throw new JsonException("Unexpected state");
    }

    private record SseResult<T>(SseResultKind ResultKind, T? Value = null) where T : class { }

    private enum SseResultKind
    {
        Ok = 1,
        Done = 2
    }
}