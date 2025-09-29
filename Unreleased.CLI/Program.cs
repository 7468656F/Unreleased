using System.Diagnostics;
using Unreleased.Downloader;
using Unreleased.Scraper.Models;
using Unreleased.Scraper.Utilities;
using static Unreleased.Scraper.Parsers.RowParser;
using dotenv.net;
using Microsoft.Extensions.Configuration;

namespace Unreleased.CLI;

class Program
{
    private static string[] SongMatchesFilters(Song song)
    {
        bool linksOk = song.Links.Count > 0;
        bool typeOk = song.Type is "Throwaway" or "OG" or "OG File" or "Demo" or "High Bitrate Rip";
        bool portionOk = song.Portion is "Full" or "OG" or "OG File";
        bool qualityOk = song.Quality is "Lossless" or "CD Quality" or "High Quality";
        bool ogSongOk = !((song.Type == "OG" || song.Type == "OG File") &&
                         (song.Portion == "OG" || song.Portion == "OG File"));

        List<string> invalidReasons = [];
        
        if (!typeOk)
            invalidReasons.Add($"Type: {song.Type}");
        if (!portionOk)
            invalidReasons.Add($"Portion: {song.Portion}");
        if (!qualityOk)
            invalidReasons.Add($"Quality: {song.Quality}");
        if (!linksOk)
            invalidReasons.Add("No valid download link");
        if (!ogSongOk)
            invalidReasons.Add("OG song");
        
        return invalidReasons.ToArray();
    }
    
    public static async Task Main(string[] args)
    {
        DotEnv.Load();
        
        var url = GetInput("URL");

        if (string.IsNullOrWhiteSpace(url))
            return;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        
        var zyteApiKey = configuration["ZyteApiKey"];
        
        if (string.IsNullOrWhiteSpace(zyteApiKey))
            throw new InvalidOperationException("Zyte API key is not set in appsettings.json");

        var scraper = new Scraper.Scraper(zyteApiKey);
        var page = await scraper.ScrapePage(url, true, true);
        
        var sw = Stopwatch.StartNew();
        
        var document = Scraper.Scraper.GetHtmlDocument(page);

        var table = document.DocumentNode.SelectNodes("//table/tbody/tr");
        var rows = table
            .Select((node, index) => (index: index, node: node))
            .Where(t => t.node.Attributes["style"]?.Value?.Contains("height:") ?? false) // DESIGN: exclude freezebar-cells
            .ToArray();
        
        var titleNode = document.DocumentNode.SelectSingleNode("//title");
        var pageTitle = titleNode?.InnerText.Trim() ?? "Unknown";
        var trackerName = TrackerNameExtractor.ExtractTrackerName(pageTitle);

        var processedRows = ProcessRows(rows, trackerName);
        var songs = processedRows.OfType<Song>().ToArray();

        Debug.WriteLine("Rows: " + rows.Length);
        Debug.WriteLine("Processed Rows: " + processedRows.Length);
        Debug.WriteLine("Songs: " + songs.Length);
        
        Era? previousEra = null;
        foreach (var row in processedRows)
        {
            switch (row)
            {
                case Song song:
                    await ProcessSongAsync(songs, song, previousEra, zyteApiKey, trackerName);
                    continue;
                case Era era:
                    previousEra = era;
                    break;
            }
        }
        
        // TODO: show eta, progress bar, etc.
        
        sw.Stop();
        Debug.WriteLine($"Processed {processedRows.Length} rows in {sw.ElapsedMilliseconds} ms ({Math.Round((double)sw.ElapsedMilliseconds / processedRows.Length, 2)} ms/row)");
    }

    private static async Task ProcessSongAsync(Song[] songs, Song song, Era? previousEra, string zyteApiKey,
        string trackerName)
    {
        var highestVersion = songs
            .Where(s => s.GetTitle() == song.GetTitle() &&
                        SongMatchesFilters(s).Length == 0)
            .OrderByDescending(s => s.GetVersion())
            .FirstOrDefault()?
            .GetVersion();

        if (song.GetVersion() != highestVersion) // DESIGN: only download the highest version of a song
        {
            Debug.WriteLine($"Skipping {song.GetDebugTitle()} (not highest version)");
            return;
        }
                    
        var invalidReasons = SongMatchesFilters(song);
        if (invalidReasons.Length > 0)
        {
            Debug.WriteLine($"Skipping {song.GetDebugTitle()} due to invalid attributes: " + string.Join(", ", invalidReasons));
            return;
        }


        Debug.WriteLine($"Writing {song.GetDebugTitle()} LinkCount: {song.Links.Count}");

        string? outputPath;
        try
        {
            outputPath = await FileWriter.WriteSongToFile(song, previousEra!, zyteApiKey: zyteApiKey, trackerName: trackerName);
        }
        catch (IOException e)
        {
            Debug.WriteLine($"Skipped {song.GetDebugTitle()}: {e.Message}");
            return;
        }
        catch (ArgumentException e)
        {
            Debug.WriteLine($"Skipped {song.GetDebugTitle()}: {e.Message}");
            return;
        }


        Console.WriteLine($"Wrote {outputPath}");

        return;
    }

    private static string? GetInput(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine();
    }
}