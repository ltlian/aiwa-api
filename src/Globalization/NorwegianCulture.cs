using System.Globalization;

namespace AIWA.API.Globalization
{
    public static class NorwegianCulture
    {
        public static DateTimeFormatInfo DateTimeFormatInfo = new()
        {
            DayNames = ["søndag", "mandag", "tirsdag", "onsdag", "torsdag", "fredag", "lørdag"],
            MonthNames = ["januar", "februar", "mars", "april", "mai", "juni", "juli", "august", "september", "oktober", "november", "desember", ""],
            MonthGenitiveNames = ["januar", "februar", "mars", "april", "mai", "juni", "juli", "august", "september", "oktober", "november", "desember", ""],
            FirstDayOfWeek = DayOfWeek.Monday,
            MonthDayPattern = "d. MMMM",
            LongDatePattern = "dddd d. MMMM yyyy"
        };
    }
}