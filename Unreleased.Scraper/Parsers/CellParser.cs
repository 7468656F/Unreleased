using HtmlAgilityPack;
using Unreleased.Scraper.Models;
using Unreleased.Scraper.Utilities;

namespace Unreleased.Scraper.Parsers;

public class CellParser
{
    private static string[] GetLines(HtmlNode htmlNode, bool child = false)
    {
        var html = htmlNode.InnerHtml
            .Replace("<br>", "\n")
            .Replace("<br/>", "\n")
            .Replace("<br />", "\n");
        
        var text = HtmlEntity.DeEntitize(HtmlNode.CreateNode("<div>" + html + "</div>").InnerText);
        
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
    
    public static string GetRawText(HtmlNode htmlNode)
    {
        var lines = GetLines(htmlNode);
        return string.Join('\n', lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }
    
    public static Cell GetCell(HtmlNode htmlNode, string header)
    {
        var img = htmlNode.Descendants("img").FirstOrDefault();
        if (img is not null)
        {
            var src = img.GetAttributeValue("src", string.Empty);
            if (!string.IsNullOrWhiteSpace(src))
                return new Image(src, header);
        }

        var aElements = htmlNode.Descendants("a").ToList();
        if (aElements.Count > 0)
        {
            Dictionary<string, string> links = [];
            foreach (var a in aElements)
            {
                var content = HtmlEntity.DeEntitize(a.InnerText ?? string.Empty);
                var hrefValue = a.GetAttributeValue("href", string.Empty);
                string? href = string.IsNullOrWhiteSpace(UrlUtilities.CleanGoogleRedirect(hrefValue)) ? null : hrefValue;
                if (!string.IsNullOrWhiteSpace(content) && !links.ContainsKey(content))
                    links.Add(content, href!);
            }
            return new Text(GetRawText(htmlNode), header, links);
        }

        return new Text(GetRawText(htmlNode), header);
    }
}