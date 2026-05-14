namespace photo_scraper
{
    using System.Text.RegularExpressions;

    public static class StringUtilities
    {
        public static string SanitizeFolderName(string title)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()));
            return Regex.Replace(title, "[" + invalidChars + "]", "_").Trim();
        }

        public static int ExtractYearFromText(string text)
        {
            var match = Regex.Match(text, @"20\d{2}");
            return match.Success ? int.Parse(match.Value) : 0;
        }
    }
}
