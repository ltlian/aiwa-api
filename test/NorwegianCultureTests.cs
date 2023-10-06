using AIWA.API.Globalization;

namespace AIWA.API.Test;

public class NorwegianCultureTests
{
    [Fact]
    public void DateObjectSerializesTo_NorwegianFormat()
    {
        var expected = "24 oktober";
        var date = new DateOnly(2023, 10, 24);

        var result = date.ToString("d MMMM", NorwegianCulture.DateTimeFormatInfo);

        Assert.Equal(expected, result);
    }
}
