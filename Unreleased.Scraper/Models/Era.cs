using Unreleased.Scraper.Utilities;

namespace Unreleased.Scraper.Models;

public record Era(
    Dictionary<string, int> Stats,
    string Name,
    string Timeline,
    string ImageUrl,
    string? Description // some trackers (e.g.: Eminem Tracker) don't have a description
)
{
    public string GetTitle() => Name.Split('\n').First();

    public string[] GetAliases()
    {
        var lines = Name.Split('\n');
        if (lines.Length < 2)
            return [];
        
        return lines[1]
            .TrimStart('(')
            .TrimEnd(')')
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public string GetHighQualityImageUrl()
    {
        return RegexPatterns.HighQualityImageSubstitutionRegex().Replace(ImageUrl, "");
    }
};