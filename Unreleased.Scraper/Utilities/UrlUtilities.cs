using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace Unreleased.Scraper.Utilities;

public class UrlUtilities
{
    public static string CleanGoogleRedirect(string href)
    {
        if (string.IsNullOrWhiteSpace(href)) return href;
        href = HtmlEntity.DeEntitize(href);

        if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) ||
            !uri.Host.EndsWith("google.com", StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.Equals("/url", StringComparison.OrdinalIgnoreCase))
            return href;

        var qs = HttpUtility.ParseQueryString(uri.Query);

        var target = qs.Get("q") ?? qs.Get("url") ?? qs.Get("imgurl");
        if (target is not null)
        {
            return Uri.TryCreate(target, UriKind.Absolute, out var targetUri)
                ? targetUri.ToString()
                : target;
        }
        
        return href;
    }
}