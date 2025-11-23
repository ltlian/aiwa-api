namespace Lli.OpenAi.Core.Speech;

// See https://platform.openai.com/docs/guides/text-to-speech/supported-output-formats
public enum OutputFormat
{
    /// <summary>
    /// Default
    /// </summary>
    Mp3 = 1,

    /// <summary>
    /// For digital audio compression, preferred by YouTube, Android, iOS.
    /// </summary>
    Aac = 2,

    /// <summary>
    /// For lossless audio compression, favored by audio enthusiasts for archiving.
    /// </summary>
    Flac = 3,

    /// <summary>
    /// For internet streaming and communication, low latency.
    /// </summary>
    Opus = 4
}
