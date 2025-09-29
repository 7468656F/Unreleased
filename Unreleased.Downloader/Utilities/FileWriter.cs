using System.Diagnostics;
using Unreleased.Downloader.Utilities;
using Unreleased.Scraper.Models;

namespace Unreleased.Downloader;

public class FileWriter
{
    /// <summary>
    ///   Formats a file name by replacing placeholders with song information.<br/>
    ///   Supported placeholders:<br/>
    ///   %emoji: Emoji of the song or default music note<br/>
    ///   %title: Title of the song<br/>
    ///   %version: Version of the song or 1<br/>
    ///   %artists: Comma-separated list of all artists<br/>
    ///   %producers: Comma-separated list of all producers<br/>
    ///   %mainArtist: First artist in the list<br/>
    ///   %mainProducer: First producer in the list<br/>
    ///   %fileYear: Year of the file date or "Unknown"<br/>
    ///   %leakYear: Year of the leak date or "Unknown"<br/>
    ///   %fileDate: File date in "yyyy-MM-dd" format or "Unknown"<br/>
    ///   %leakDate: Leak date in "yyyy-MM-dd" format or "Unknown"<br/>
    ///   %era: Era of the song<br/>
    /// </summary>
    /// <param name="song">The song to extract the values from</param>
    /// <param name="era">The era to extract the values from</param>
    /// <param name="fileName">The unformatted filename</param>
    /// <param name="fallbackToOgFilename">Whether to fall back to the OG Filename if parameter fileName is null and OG Filename is not null</param>
    /// <returns>Formatted filename</returns>
    public static string FormatFileName(Song song, Era era, string? fileName = null, bool fallbackToOgFilename = true)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            if (song.GetOgFileName() is not null && fallbackToOgFilename)
                return song.GetOgFileName()!;
            fileName = "%title [v%version] - %mainArtist";
        }
        
        fileName = fileName.Replace("%emoji", song.GetEmoji() ?? "🎵");
        
        var (artists, producers) = song.GetArtistsAndProducers();
        fileName = fileName.Replace("%title", song.GetTitle());
        fileName = fileName.Replace("%version", (song.GetVersion() ?? 1).ToString());
        
        fileName = fileName.Replace("%artists", string.Join(", ", artists));
        fileName = fileName.Replace("%producers", producers.Length > 0 ? string.Join(", ", producers) : "Unknown");
        
        fileName = fileName.Replace("%mainArtist", artists.First());
        fileName = fileName.Replace("%mainProducer", producers.Length > 0 ? producers.First() : "Unknown");
        
        fileName = fileName.Replace("%fileYear", song.FileDate is not null ? song.FileDate.Value.Year.ToString() : "Unknown");
        fileName = fileName.Replace("%leakYear", song.LeakDate is not null ? song.LeakDate.Value.Year.ToString() : "Unknown");
        
        fileName = fileName.Replace("%fileDate", song.FileDate is not null ? song.FileDate.Value.Year.ToString("yyyy-MM-dd") : "Unknown");
        fileName = fileName.Replace("%leakDate", song.LeakDate is not null ? song.LeakDate.Value.Year.ToString("yyyy-MM-dd") : "Unknown");
        
        fileName = fileName.Replace("%era", era.GetTitle());

        return fileName;
    }
    
    /// <summary>
    /// Writes metadata to a file using TagLib#.
    /// Sets title, album, year, artists, producers, notes, type, portion, quality, emoji, version, original filename, and cover image.
    /// </summary>
    /// <param name="song">The song object whose metadata will be written.</param>
    /// <param name="era">The era from which album title and cover image are taken.</param>
    /// <param name="filePath">Path to the target file where metadata will be written.</param>
    /// <param name="httpClient">Optional: An HttpClient for loading the cover image.</param>
    /// <returns>A Task representing the asynchronous write operation.</returns>
    public static async Task WriteMetadataToFile(Song song, Era era, string filePath, HttpClient? httpClient = null)
    {
        var file = TagLib.File.Create(filePath);
        
        file.Tag.Title = song.GetTitle();
        file.Tag.Album = era.GetTitle();
        if (song.FileDate is not null)
            file.Tag.Year = (uint)song.FileDate.Value.Year;
        file.Tag.Performers = song.GetArtists();
        file.Tag.Composers = song.GetProducers();

        Dictionary<string, string> comments = [];
        
        if (song.GetNotes() is not null)
            comments.Add("Notes", song.GetNotes()!);
        
        comments.Add("Type", song.Type);
        comments.Add("Portion", song.Portion);
        comments.Add("Quality", song.Quality);
        
        if (song.GetEmoji() is not null)
            comments.Add("Emoji", song.GetEmoji()!);
        
        if (song.GetVersion() is not null && !string.IsNullOrEmpty(song.GetVersion().ToString()))
            comments.Add("Version", song.GetVersion()!.ToString()!);
        
        if (song.GetOgFileName() is not null)
            comments.Add("OG Filename", song.GetOgFileName()!);
        
        file.Tag.Comment = string.Join("\n", comments.Select(kv => $"{kv.Key}: {kv.Value}"));
        
        var imageBytes = await HttpUtils.ReadUrlAsBytes(era.GetHighQualityImageUrl(), httpClient);
        file.Tag.Pictures = [ new TagLib.Picture(imageBytes) ];

        TagLib.Id3v2.Tag.DefaultVersion = 3;
        TagLib.Id3v2.Tag.ForceDefaultVersion = true;
        
        file.Save();
    }
    
    private static string GetDefaultOutputDirectory(Song song, Era era, string trackerName)
    {
        var sanitizedTrackerName = NameSanitizer.Sanitize(trackerName);
        var sanitizedEra = NameSanitizer.Sanitize(era.GetTitle()); // TODO: if more than 60% of name is sanitized, try aliases
        
        var musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        var outputDirectory = Path.Combine(musicFolder, $"Unreleased/{sanitizedTrackerName}/{sanitizedEra}");
        Directory.CreateDirectory(outputDirectory);
        return outputDirectory;
    }
    
    /// <summary>
    /// Downloads a song, saves it as a file, writes metadata, and returns the full file path.
    /// </summary>
    /// <param name="song">The song object to download and save.</param>
    /// <param name="era">The era from which album title and cover image are taken.</param>
    /// <param name="trackerName">Name of the tracker for the folder structure.</param>
    /// <param name="zyteApiKey">API key for accessing the Zyte service, required for certain download sources.</param>
    /// <param name="fileName">Optional: The desired file name (placeholders will be replaced).</param>
    /// <param name="outputDirectory">Optional: Target directory for the file.</param>
    /// <param name="httpClient">Optional: HttpClient for download and cover image.</param>
    /// <returns>The full path to the saved file.</returns>
    public static async Task<string> WriteSongToFile(
        Song song,
        Era era,
        string trackerName,
        string zyteApiKey,
        string? outputDirectory = null,
        string? fileName = null,
        HttpClient? httpClient = null)
    {
        KeyValuePair<string, string>? downloadLink = song.Links.Count > 0 ? song.Links.First() : null;
        
        if (downloadLink is null)
            throw new ArgumentNullException(nameof(downloadLink), "No download link found for the song.");

        Debug.WriteLine("Matching URL");
        var success  = Matching.MatchUrl(downloadLink.Value.Value, out FileHost host, out string fileId);
        
        if (!success)
        {
            throw new ArgumentException($"The provided download link ('{downloadLink.Value.Value}') is not valid.");
        }
        
        if (outputDirectory is null)
            outputDirectory = GetDefaultOutputDirectory(song, era, trackerName);
        
        outputDirectory = Path.GetFullPath(outputDirectory);
        
        var formattedFileName = FormatFileName(song, era, fileName, fallbackToOgFilename: false);
        var safeFileName = NameSanitizer.Sanitize(formattedFileName);
        
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        
        if (Directory.GetFiles(outputDirectory).Any(filename => Path.GetFileNameWithoutExtension(filename) == safeFileName))
            throw new IOException($"A file named '{safeFileName}' already exists in the directory '{outputDirectory}'.");

        Debug.WriteLine($"Downloading {song.GetDebugTitle()} file");
        
        var downloader = new Downloader(zyteApiKey, httpClient);
        var fileBytes = await downloader.DownloadAsync(host, fileId);

        Debug.WriteLine($"Transcoding {song.GetDebugTitle()} file");

        var filePath = await Transcoding.Transcoder.TranscodeAndSaveBytes(fileBytes, safeFileName, outputDirectory);
        
        Debug.WriteLine($"Writing metadata for {song.GetDebugTitle()}");
        await WriteMetadataToFile(song, era, filePath, httpClient);
        
        return Path.GetFullPath(filePath);
    }
}