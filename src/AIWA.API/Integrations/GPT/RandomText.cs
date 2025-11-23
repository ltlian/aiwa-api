namespace AIWA.API.Integrations.GPT;

public static class RandomText
{
    private static readonly Random random = new();
    private static readonly KeyValuePair<char, double>[] distributionUpper;
    private static readonly KeyValuePair<char, double>[] distributionLower;
    private static readonly char fallback = 'Z';

    static RandomText()
    {
        // https://en.wikipedia.org/wiki/Letter_frequency

        var frequencies = new Dictionary<char, double>
        {
            ['A'] = 8.2,
            ['B'] = 1.5,
            ['C'] = 2.8,
            ['D'] = 4.3,
            ['E'] = 12.7,
            ['F'] = 2.2,
            ['G'] = 2.0,
            ['H'] = 6.1,
            ['I'] = 7.0,
            ['J'] = 0.16,
            ['K'] = 0.77,
            ['L'] = 4.0,
            ['M'] = 2.4,
            ['N'] = 6.7,
            ['O'] = 7.5,
            ['P'] = 1.9,
            ['Q'] = 0.12,
            ['R'] = 6.0,
            ['S'] = 6.3,
            ['T'] = 9.1,
            ['U'] = 2.8,
            ['V'] = 0.98,
            ['W'] = 2.4,
            ['X'] = 0.15,
            ['Y'] = 2.0,
            ['Z'] = 0.074
        };

        double total = frequencies.Values.Sum();
        double cumulative = 0;

        distributionUpper = [.. frequencies
            .OrderBy(kvp => kvp.Value)
            .Select(kvp =>
            {
                cumulative += kvp.Value / total;
                return new KeyValuePair<char, double>(kvp.Key, cumulative);
            })];

        distributionLower = [.. distributionUpper.Select(kvp => new KeyValuePair<char, double>(char.ToLower(kvp.Key), kvp.Value))];
    }

    /// <summary>
    /// Returns a random character selected according to a predefined probability distribution.
    /// </summary>
    /// <param name="upperCase">Whether to return the uppercase variant of the character.</param>
    public static char NextChar(bool upperCase = true)
    {
        var value = random.NextDouble();

        // Linear search is fine for a small set.
        foreach (var (letter, cumulative) in upperCase ? distributionUpper : distributionLower)
        {
            if (value <= cumulative)
                return letter;
        }

        return upperCase ? fallback : char.ToLower(fallback);
    }

    public static string GetText(int length, bool upperCase = true) => new([.. Enumerable.Range(0, length).Select(_ => NextChar(upperCase))]);
}
