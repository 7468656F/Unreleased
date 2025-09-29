namespace Unreleased.Downloader;

public class Matching
{
    public static bool MatchUrl(string url, out FileHost host, out string fileId)
    {
        var pillowcaseMatch = RegexPatterns.PillowcaseRegex().Match(url);
        if (pillowcaseMatch.Success)
        {
            host = FileHost.Pillowcase;
            fileId = pillowcaseMatch.Groups[1].Value;
            return true;
        }
        
        var frosteMatch = RegexPatterns.FrosteRegex().Match(url);
        if (frosteMatch.Success)
        {
            host = FileHost.Froste;
            fileId = frosteMatch.Groups[1].Value;
            return true;
        }
        
        var pixeldrainMatch = RegexPatterns.PixeldrainRegex().Match(url);
        if (pixeldrainMatch.Success)
        {
            host = FileHost.Pixeldrain;
            fileId = pixeldrainMatch.Groups[1].Value;
            return true;
        }
        
        var imgurMatch = RegexPatterns.ImgurRegex().Match(url);
        if (imgurMatch.Success)
        {
            host = FileHost.Imgur;
            fileId = imgurMatch.Groups[1].Value;
            return true;
        }
        
        var krakenfilesMatch = RegexPatterns.KrakenfilesRegex().Match(url);
        if (krakenfilesMatch.Success)
        {
            host = FileHost.Krakenfiles;
            fileId = krakenfilesMatch.Groups[1].Value;
            return true;
        }

        host = FileHost.Invalid;
        fileId = string.Empty;
        return false;
    }
}