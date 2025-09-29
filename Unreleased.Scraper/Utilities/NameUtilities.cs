using System.Text.RegularExpressions;
using Unreleased.Scraper.Parsers;

namespace Unreleased.Scraper.Utilities;

public partial class NameUtilities
{
    private static void AddSplitValues(ref List<string> list, string value)
    {
        list.AddRange(value
            .Split([',', '&'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()));
    }
    
    
    /// <summary>
    /// Extracts featured artists and producers from the second line of a track title.
    /// </summary>
    public static (string[] artists, string[] producers) ExtractFeaturesAndProducers(string secondLine)
    {
        var matchGroups = RegexPatterns.FeatureAndProducerRegex()
            .Matches(secondLine)
            .Select(m => m.Groups.Cast<Group>().ToArray())
            .ToArray();
        
        List<string> artists = [];
        List<string> producers = [];

        foreach (var groups in matchGroups)
        {
            switch (groups[1].Value)
            {
                case "feat.":
                case "with":
                    AddSplitValues(ref artists, groups[2].Value);
                    break;
                case "prod.":
                    AddSplitValues(ref producers, groups[2].Value);
                    break;
            }
        }

        return (artists.ToArray(), producers.ToArray());
    }
    
    public static (string? emoji, string? artist, string title, int? version, string[] features, string[] producers)
        ExtractNameValues(string[] lines)
    {
        var match = RegexPatterns.NameRegex()
            .Match(lines[0]);

        string? emoji = match.Groups["emoji"].Success ? match.Groups["emoji"].Value?.Trim() : null;
        string? artist = match.Groups["artist"].Success ? match.Groups["artist"].Value.Trim() : null;
        string title = ColumnParsers.ProcessTitle(match.Groups["title"].Value.Trim());
        int? version = int.TryParse(match.Groups["version"].Value, out var v) ? v : null;
        List<string> features = [];
        List<string> producers = [];
        if (match.Groups["AaP"].Success)
        {
            var (extractedFeatures, extractedProducers) = ExtractFeaturesAndProducers(match.Groups["AaP"].Value);
            features.AddRange(extractedFeatures);
            producers.AddRange(extractedProducers);
        }
        
        if (lines.Length > 1)
        {
            var (extractedFeatures, extractedProducers) = ExtractFeaturesAndProducers(lines[1]);
            features.AddRange(extractedFeatures);
            producers.AddRange(extractedProducers);
        }

        return (emoji, artist, title, version,
            features.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray(),
            producers.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
    }
}