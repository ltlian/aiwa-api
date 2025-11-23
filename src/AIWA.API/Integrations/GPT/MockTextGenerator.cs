namespace AIWA.API.Integrations.GPT;

public static class MockTextGenerator
{
    public static IEnumerable<string> GenerateMockTextStream(int iterations = 100)
    {
        bool wasPeriod = false;
        bool wasPunct = false;
        bool shouldCapitalize = true;

        for (int i = 0; i < iterations; i++)
        {
            if (wasPunct)
            {
                wasPunct = false;

                if (wasPeriod)
                {
                    wasPeriod = false;
                    yield return string.Concat(' ', RandomText.NextChar(), RandomText.GetText(Random.Shared.Next(0, 2), false));
                }
                else
                {
                    yield return string.Concat(' ', RandomText.GetText(Random.Shared.Next(1, 3), false));
                }
            }
            else
            {
                var value = Random.Shared.Next(1, 7);
                var word = shouldCapitalize
                    ? string.Concat(RandomText.NextChar(), RandomText.GetText(Random.Shared.Next(0, 2), false))
                    : RandomText.GetText(Random.Shared.Next(1, 3), false);

                if (value == 1)
                {
                    wasPunct = true;
                    wasPeriod = true;
                }
                else if (value == 2)
                {
                    wasPunct = true;
                }

                var chunk = value switch
                {
                    1 => ".",
                    2 => ",",
                    _ => RandomText.GetText(Random.Shared.Next(1, 3), false),
                };

                if (Random.Shared.Next(5) == 1)
                {
                    yield return " " + word;
                }
                else
                {
                    yield return chunk;
                }
            }

            var chanceOfTerminator = wasPeriod ? 5 : 100;

            if (Random.Shared.Next(1, chanceOfTerminator) == 1)
            {
                yield return "\n\n";
            }
        }
    }
}
