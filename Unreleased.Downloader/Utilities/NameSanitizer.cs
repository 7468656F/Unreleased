namespace Unreleased.Downloader.Utilities;

public class NameSanitizer
{
    public static string Sanitize(string name)
    {
        char[] invalidChars = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];
        
        return invalidChars.Aggregate(name, (current, invalidChar) => current.Replace(invalidChar, '_'));
    }
}