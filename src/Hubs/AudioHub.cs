using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Lli.OpenAi.Core.Speech;
using Microsoft.AspNetCore.SignalR;

namespace AIWA.API.Hubs
{
    public class AudioHub(ILogger<AudioHub> logger) : Hub
    {
        public static Stream GetAudioStream()
        {
            var filePath = Path.Join
            (
                "C:",
                "code",
                "projects",
                "aiwa",
                "outputs",
                "speech-service-test.mp3"
                //"noise.wav"
            );

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public async Task StreamAudioAsync()
        {
            logger.LogInformation("{Method_Name} - Loading file", nameof(StreamAudioAsync));


            var buffer = new byte[4096]; // buffer size can be adjusted
            int bytesRead;

            using var stream = GetAudioStream();
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await Clients.Caller.SendAsync("ReceiveAudioData", buffer[..bytesRead]);
            }
        }

        public async IAsyncEnumerable<byte[]> IterTestAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            logger.LogInformation("{Method_Name} - Loading file", nameof(IterTestAsync));

            var buffer = new byte[4096];
            int bytesRead;
            using var stream = GetAudioStream();
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                //var chunk = new byte[bytesRead];
                //Array.Copy(buffer, chunk, bytesRead);
                yield return buffer;
            }
        }

        public async IAsyncEnumerable<int> Counter(int count, int delay, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return i;

                // Use the cancellationToken in other APIs that accept cancellation
                // tokens so the cancellation can flow down to them.
                await Task.Delay(delay, cancellationToken);
            }
        }

        public async Task TriggerAudioAsync()
        {
            logger.LogInformation("Method: {Method_Name}", nameof(TriggerAudioAsync));
            await StreamAudioAsync();
        }

        //public ChannelReader<byte[]> StreamAudio()
        //{
        //    var channel = Channel.CreateUnbounded<byte[]>();
        //    _ = StreamAudioToChannelAsync(channel.Writer);
        //    return channel.Reader;
        //}

        private async Task StreamAudioToChannelAsync(ChannelWriter<byte[]> writer)
        {
            var filePath = Path.Join
            (
                "C:",
                "code",
                "projects",
                "aiwa",
                "outputs",
                $"speech-service-test.mp3"
            );

            try
            {
                logger.LogInformation("{Method_Name} - Loading file", nameof(TriggerAudioAsync));

                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                var buffer = new byte[4096]; // You can adjust the buffer size
                int bytesRead;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var chunk = new byte[bytesRead];
                    Array.Copy(buffer, chunk, bytesRead);
                    await writer.WriteAsync(chunk);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while streaming audio.");
                writer.TryComplete(ex);
            }
            finally
            {
                writer.TryComplete();
            }
        }

        //public ChannelReader<byte[]> StreamAudio()
        //{
        //    Stream audioStream = null;

        //    var channel = Channel.CreateUnbounded<byte[]>();
        //    _ = WriteToChannel(audioStream, channel.Writer);
        //    return channel.Reader;
        //}

        //private async Task WriteToChannel(Stream audioStream, ChannelWriter<byte[]> writer)
        //{
        //    while (await audioStream.ReadAsync(chunk))
        //    {
        //        await writer.WriteAsync(chunk);
        //        // Handle end of stream and channel completion as needed
        //    }
        //}
    }
}
