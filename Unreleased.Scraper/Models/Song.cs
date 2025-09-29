using System.Text.RegularExpressions;
using Unreleased.Scraper.Utilities;

namespace Unreleased.Scraper.Models;

public record Song(
    // Core identification
    string Name,
    string Era,
    
    // Content metadata
    string Type,
    string Portion,
    string Quality,
    
    // Technical details
    int? TrackLength,
    
    // Dates
    DateTime? FileDate,
    DateTime? LeakDate,
    
    // Additional info
    string? Notes,
    Dictionary<string, string> Links)
{
    public required string TrackerTitle { get; init; }
    
    public Song(
        string trackerTitle,
        string name,
        string era,
        string type,
        string portion,
        string quality,
        int? trackLength,
        DateTime? fileDate,
        DateTime? leakDate,
        string? notes,
        Dictionary<string, string> links
    ) : this(name, era, type, portion, quality, trackLength, fileDate, leakDate, notes, links)
    {
        this.TrackerTitle = trackerTitle;
    }
    
    private (string? emoji, string? artist, string title, int? version, string[]? features, string[]? producers)
        _extractedValues = (null, null, string.Empty, null, null, null);
    
    private (string? emoji, string? artist, string title, int? version, string[]? features, string[]? producers)
        ExtractValues()
    {
        if (_extractedValues.Item3 != string.Empty)
            return _extractedValues;

        var lines = Name.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var (extractedEmoji, extractedArtist, extractedTitle, extractedVersion, extractedFeatures, extractedProducers) =
            NameUtilities.ExtractNameValues(lines);

        _extractedValues = (extractedEmoji, extractedArtist, extractedTitle, extractedVersion, extractedFeatures,
            extractedProducers);
        return _extractedValues;
    }

    public string? GetEmoji()
    {
        var extractedValues = ExtractValues();
        return extractedValues.emoji;
    }
    
    public string? GetArtist()
    {
        var extractedValues = ExtractValues();
        return extractedValues.artist;
    }
    
    public string GetTitle()
    {
        var extractedValues = ExtractValues();
        return extractedValues.title;
    }

    public int? GetVersion()
    {
        var extractedValues = ExtractValues();
        return extractedValues.version;
    }
    
    private  (string[]? artists, string[]? producers) _extractedArtistsAndProducers = (null, null);
    public (string[] artists, string[] producers) GetArtistsAndProducers()
    {
        if (_extractedArtistsAndProducers.artists is not null && _extractedArtistsAndProducers.producers is not null)
            return _extractedArtistsAndProducers!;
        
        List<string> artists = [];
        List<string> producers = [];
        
        var artistFromTitle = GetArtist();

        if (!string.IsNullOrEmpty(artistFromTitle))
        {
            artists.Add(artistFromTitle);
        }
        else
        {
            artists.Add(TrackerNameExtractor.ExtractTrackerName(TrackerTitle));
        }
        
        var extractedValues = ExtractValues();
        
        var lines = Name.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length > 1)
        {
            var (extractedFeatures, extractedProducers) = NameUtilities.ExtractFeaturesAndProducers(lines[1]);
            
            artists.AddRange(extractedFeatures);
            producers.AddRange(extractedProducers);
        }
        
        artists.AddRange(extractedValues.features ?? []);
        producers.AddRange(extractedValues.producers ?? []);

        _extractedArtistsAndProducers = (
            artists.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray(),
            producers.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
        
        return _extractedArtistsAndProducers!;
    }

    public string[] GetArtists()
    {
        var (artists, _) = GetArtistsAndProducers();
        return artists;
    }
    
    public string[] GetProducers()
    {
        var (_, producers) = GetArtistsAndProducers();
        return producers;
    }

    private Match? _ogFileNameMatch = null;
    public string? GetOgFileName()
    {
        if (string.IsNullOrEmpty(Notes))
            return null;
        
        var ogFileNameMatch = _ogFileNameMatch ??= RegexPatterns.OgFileNameRegex().Match(Notes);
        if (ogFileNameMatch.Success)
        {
            return Path.GetFileNameWithoutExtension(ogFileNameMatch.Groups["filename"].Value);
        }

        return null;
    }

    public string? GetNotes()
    {
        if (Notes == null) 
            return null;
        
        if (GetOgFileName() != null)
        {
            var lines = Notes.Split('\n');
            return string.Join('\n', lines.Skip(1));
        }
        
        return Notes;
    }
    
    public string[] GetAliases()
    {
        var lines = Name.Split('\n');
        if (lines.Length < 3)
            return [];
        
        return lines[2]
            .TrimStart('(')
            .TrimEnd(')')
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
};