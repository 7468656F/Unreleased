using System.Diagnostics;
using HtmlAgilityPack;
using Unreleased.Downloader;
using Unreleased.Scraper.Models;
using Unreleased.Scraper.Utilities;
using static Unreleased.Scraper.Parsers.RowParser;
using File = System.IO.File;
using dotenv.net;
using Microsoft.Extensions.Configuration;

namespace Unreleased.CLI;

class Program
{
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
                    if ((song.Type == "OG" || song.Type == "OG File") &&
                        (song.Portion == "OG" || song.Portion == "OG File"))
                    {
                        Debug.WriteLine("Skipping OG song");
                        continue;
                    }
                    
                    var highestVersion = songs
                        .Where(s => s.GetTitle() == song.GetTitle())
                        .OrderByDescending(s => s.GetVersion())
                        .FirstOrDefault()!
                        .GetVersion();
                    
                    if (song.GetVersion() != highestVersion) // DESIGN: only download the highest version of a song
                    {
                        Debug.WriteLine($"Skipping {song.GetTitle()} v{song.GetVersion()} (not highest version)");
                        continue;
                    }
                    
                    if (song.Links.Count == 0 || song is not
                        {
                            Type: "Throwaway" or "OG" or "OG File" or "Demo",
                            Portion: "Full" or "OG" or "OG File",
                            Quality: "Lossless" or "CD Quality" or "High Quality"
                        })
                    {
                        Debug.WriteLine($"Skipping {song.GetTitle()} v{song.GetVersion() ?? 1 } [{song.Era}] (no valid download link or invalid type/portion/quality)");
                        continue;
                    }

                    Debug.WriteLine($"Writing {song.GetTitle()} v{song.GetVersion() ?? 1} [{previousEra?.GetTitle()}] LinkCount: {song.Links.Count}");
                    
                    string? outputPath;
                    try
                    {
                        outputPath = await FileWriter.WriteSongToFile(song, previousEra!, zyteApiKey: zyteApiKey, trackerName: trackerName);
                    }
                    catch (IOException e)
                    {
                        Debug.WriteLine($"Skipped {song.GetTitle()} [{previousEra!.GetTitle()}]: {e.Message}");
                        continue;
                    }
                    catch (ArgumentException e)
                    {
                        Debug.WriteLine($"Skipped {song.GetTitle()} [{previousEra!.GetTitle()}]: {e.Message}");
                        continue;
                    }
                    
                    
                    Console.WriteLine($"Wrote {outputPath}");
                    
                    break;
                case Era era:
                    previousEra = era;
                    break;
            }
        }
        
        sw.Stop();
        Debug.WriteLine($"Processed {processedRows.Length} rows in {sw.ElapsedMilliseconds} ms ({Math.Round((double)sw.ElapsedMilliseconds / processedRows.Length, 2)} ms/row)");
        Console.ReadLine();
    }

    private static string? GetInput(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine();
    }
}