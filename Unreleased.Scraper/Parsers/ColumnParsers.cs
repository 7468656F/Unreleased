using Unreleased.Scraper.Utilities;

namespace Unreleased.Scraper.Parsers;

public class ColumnParsers
{
    public static int? GetLength(string textLength)
    {
        string[] splitLength = textLength.Split(':');
        
        if (splitLength.Length != 2)
            return null;
        
        (string left, string right) = (splitLength[0], splitLength[1]);

        if (!int.TryParse(left, out int minutes) || !int.TryParse(right, out int seconds))
            return null;
        if (seconds == 0 && minutes == 0)
            return null;
            
        return minutes * 60 + seconds;
    }
    public static DateTime? ParseDate(string input)
    {
        return DateTime.TryParse(input, out var date) ? date : null;
    }

    public static string ProcessTitle(string title)
    {
        return title.TrimEnd('*');
    }
    
    public static Dictionary<string, int> ParseStats(string input)
    {
        var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var stats = new Dictionary<string, int>();
        
        foreach (var line in lines)
        {
            var match = RegexPatterns.StatLineRegex().Match(line);
            if (match.Success)
            {
                var amount = int.TryParse(match.Groups["amount"].Value, out var amountParsed) ? amountParsed : 0;
                stats.Add(
                    match.Groups["name"].Value
                        .Replace("(", "")
                        .Replace(")", ""),
                    amount);
            }
        }

        return stats;
    }
}