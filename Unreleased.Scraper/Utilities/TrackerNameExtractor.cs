namespace Unreleased.Scraper.Utilities;

public class TrackerNameExtractor
{
    public static string ExtractTrackerName(string title)
    {
        var match = RegexPatterns.TrackerNameRegex().Match(title);
        if (match.Success)
        {
            return match.Groups["trackerName"].Value.Trim();
        }
        
        return title
            .Replace(" - Google Drive", "")
            .Replace(" Tracker", "")
            .Trim();
    }
}