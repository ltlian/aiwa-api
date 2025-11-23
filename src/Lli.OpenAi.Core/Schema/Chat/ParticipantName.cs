using System.Text.RegularExpressions;

namespace Lli.OpenAi.Core.Schema.Chat;

public readonly partial struct ParticipantName
{
    private readonly string _value;

    private ParticipantName(string value)
    {
        _value = value;
    }

    public static implicit operator string(ParticipantName name) => name._value;

    public static ParticipantName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !ValidChatParticipantName().IsMatch(value))
        {
            throw new ArgumentException($"Invalid participant name. The name '{value}' does not match {ValidChatParticipantName()}", nameof(value));
        }

        return new ParticipantName(value);
    }

    public override string ToString() => _value;

    /// <summary>
    /// Validates a participant name for OpenAI Chat Completions.
    /// </summary>
    [GeneratedRegex("^[a-zA-Z0-9_-]{1,64}$")]
    private static partial Regex ValidChatParticipantName();

    [GeneratedRegex("[^a-zA-Z0-9_-]+")]
    private static partial Regex InValidChatParticipantName();

    public static string SanitizeName(string input)
    {
        string s = input.Replace(" ", "_");
        s = InValidChatParticipantName().Replace(s, string.Empty);
        s = s.Trim('_').Trim('-');
        return s.Length <= 64 ? s : s[..64];
    }

    [GeneratedRegex(@"[^\u0000-\u007F]+")]
    private static partial Regex UnicodeSymbols();
}
