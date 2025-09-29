using System.Text.RegularExpressions;

namespace Unreleased.Downloader;

public static partial class RegexPatterns
{
    [GeneratedRegex(@"https?:\/\/(?:pillowcase\.su|pillows\.su|plwcse\.top)\/f\/(\w+)")]
    public static partial Regex PillowcaseRegex();
    
    [GeneratedRegex(@"https?:\/\/music\.froste\.lol\/song\/(\w+)(?:\/play)?")]
    public static partial Regex FrosteRegex();
    
    [GeneratedRegex(@"https?:\/\/pixeldrain\.com\/u\/(\w+)")]
    public static partial Regex PixeldrainRegex();
    
    [GeneratedRegex(@"https?:\/\/imgur\.gg\/f\/(\w+)")]
    public static partial Regex ImgurRegex();
    
    [GeneratedRegex(@"https?:\/\/krakenfiles\.com\/view\/(\w+)\/file\.html")]
    public static partial Regex KrakenfilesRegex();
}
