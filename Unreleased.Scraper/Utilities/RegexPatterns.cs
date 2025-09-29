using System.Text.RegularExpressions;

namespace Unreleased.Scraper.Utilities;

public partial class RegexPatterns
{
    [GeneratedRegex(@"\((feat\.|prod\.|with)\s([^\)]+)\)")]
    public static partial Regex FeatureAndProducerRegex();
    
    [GeneratedRegex(
        @"^(?:(?<emoji>(?:⭐|⭐️|🏆|✨|✨️|🗑️|🥉|👑|🥇))\s)?" +
        @"(?:(?<artist>.+)\s-\s)?" +
        @"(?<title>.+?)" +
        @"(?:\s\[v(?<version>\d+)\])?" +
        @"(?:\s(?:\[v(?<version>\d+)\]|\((?<AaP>(?:feat\.|prod\.|with)\s(?:[^\)]+))\)|\[(?<AaP>(?:feat\.|prod\.|with)\s(?:[^\]]+))\])){0,2}$", // playboi carti tracker adds (feat. xxx) inside name???
        RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex NameRegex();
    
    [GeneratedRegex(@"^(?<trackerName>.+) Tracker - Google Drive$")]
    public static partial Regex TrackerNameRegex();
    
    [GeneratedRegex(
        @"OG Filename:\s*(?<filename>.+)(?:\.(?<extension>mp3|wav|aac|" + 
        @"flac|ogg|aiff|" +
        @"wma|m4a|opus|mp4|" +
        @"avi|mov|wmv|mkv|" +
        @"flv|webm|mpeg|mpg|" +
        @"3gp))?")] // TODO: remove star at end of filename
    public static partial Regex OgFileNameRegex();
    
    [GeneratedRegex(@"(?<amount>\d+)\s(?<name>.+)")]
    public static partial Regex StatLineRegex();
    
    [GeneratedRegex(@"=(?:w|h)\d+-(?:w|h)\d+")]
    public static partial Regex HighQualityImageSubstitutionRegex();
}