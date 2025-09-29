using System.Diagnostics;
using HtmlAgilityPack;
using Unreleased.Scraper.Models;
using static Unreleased.Scraper.Utilities.UrlUtilities;
using static Unreleased.Scraper.Parsers.CellParser;

namespace Unreleased.Scraper.Parsers;

public class RowParser
{
    private static Cell[] GetRowCells(HtmlNode[] columns, string[] headers) 
        => columns.Select((column, index) => GetCell(column, headers[index])).ToArray();
    
    public static CellTypes DetermineCellType(Cell cell)
    {
        switch (cell)
        {
            case Text text:
            {
                var links = text.Links;
                if (links is not null && links.Count > 0)
                    return CellTypes.TextWithLinks;
                return CellTypes.Text;
            }
            case Image:
                return CellTypes.Image;
        }

        return CellTypes.Unknown;
    }
    
    public static Song ProcessSong(HtmlNode[] columns, string trackerTitle, Cell[] cells)
    {
        var textWithLinks = cells.TakeLast(2) // DESIGN: only allow last two columns to have links
            .OfType<Text>()
            .Where(cell => DetermineCellType(cell) == CellTypes.TextWithLinks && 
                           cell.HeaderValue.Contains("link", StringComparison.InvariantCultureIgnoreCase))
            .ToArray();
        
        var texts = cells
            .OfType<Text>()
            .SkipLast(textWithLinks.Length)
            .ToArray();
        
        var links = textWithLinks
            .SelectMany(t =>
            {
                var contents = t.Content.Split('\n') ?? [];
                var linksDict = t.Links ?? new Dictionary<string, string>();
                return contents.Zip(linksDict, (content, link) => new { content, link });
            })
            .ToDictionary(x => x.content, x => CleanGoogleRedirect(x.link.Value));
        
        var era = texts[0].Content;
        var name = texts[1].Content;
        var notes = texts[2].Content;
        var trackLength = ColumnParsers.GetLength(texts[3].Content);
        var fileDate = ColumnParsers.ParseDate(texts[4].Content);
        var leakDate = ColumnParsers.ParseDate(texts[5].Content);
        var type = texts[6].Content;
        var portion = texts[7].Content;
        var quality = texts[8].Content;
        
        var song = new Song(
            Era: era,
            Name: name,
            Notes: notes,
            TrackLength: trackLength,
            FileDate: fileDate,
            LeakDate: leakDate,
            Type: type,
            Portion: portion,
            Quality: quality,
            Links: links
        ) { TrackerTitle = trackerTitle };
        
        return song;
    }
    
    public static Era ProcessEra(Cell[] cells)
    {
        var texts = cells
            .OfType<Text>()
            .ToArray();
        var images = cells.TakeLast(2)
            .OfType<Image>()
            .ToArray();
        
        var era = new Era(
            Stats: ColumnParsers.ParseStats(texts[0].Content),
            Name: texts[1].Content,
            Timeline: texts[2].Content,
            ImageUrl: images[0].Url,
            Description: texts[3]?.Content);
        
        return era;
    }
    
    public static object? ProcessRow(HtmlNode row, string trackerTitle, string[] headers)
    {
        var columns = row.ChildNodes
                .Skip(1) // skip first column (row number)
                .ToArray();
        
        var cells = GetRowCells(columns, headers);

        if (cells.Any(cell => cell is Image))
        {
            return ProcessEra(cells);
        }
        
        if (string.IsNullOrEmpty((cells[1] as Text)?.Content)) // IDEA: could skip every song with foreign era
            return null; // DESIGN: skip rows with empty song name
        
        return ProcessSong(columns, trackerTitle, cells);
    }
    
    public static object[] ProcessRows((int index, HtmlNode node)[] rows, string trackerTitle)
    {
        List<object> results = [];
        
        string[]? headers = null;
        foreach (var (index, row) in rows)
        {
            if (row.ChildNodes.Count(node => node.HasChildNodes) >= 11
                && headers is null) // DESIGN: header row has at least 11 columns (including row number)
            {
                headers = GetHeaders(row);
                continue;
            }
            
            if (headers is not null)
            {
                try
                {
                    var result = ProcessRow(row, trackerTitle, headers);
                    if (result is not null)
                        results.Add(result);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing row {index}: {ex.Message}");
                }
            }
        }

        return results.ToArray();
    }
    
    private static string[] GetHeaders(HtmlNode headerRow)
        => headerRow.ChildNodes
            .Skip(1) // skip first column (row number)
            .Where(node => node.Name == "td")
            .Select(GetRawText)
            .ToArray();
}