using Unreleased.Scraper.Utilities;

namespace Unreleased.Tests;

public class RegexTests
{
    [Theory]
    [InlineData(
        "Don Toliver - Rock N Roll (Remix)",
        null,
        "Don Toliver",
        "Rock N Roll (Remix)",
        null,
        new string[]{},
        new string[]{})]
    
    [InlineData(
        "🏆 Me Vs. Me*",
        "🏆",
        null,
        "Me Vs. Me",
        null,
        new string[]{},
        new string[]{})]
    
    [InlineData(
        "𝓫𝓸𝓼𝓼𝓮𝓼 [V1]",
        null,
        null,
        "𝓫𝓸𝓼𝓼𝓮𝓼",
        1,
        new string[]{},
        new string[]{})]
    [InlineData(
        "⭐ So Cold (feat. A$AP Rocky)",
        "⭐",
        null,
        "So Cold",
        null,
        new string[]{ "A$AP Rocky" },
        new string[]{})]
    public void Regex_Name_Correct(string name, string? expectedEmoji, string? expectedArtist, string expectedTitle, int? expectedVersion, string[] expectedFeatures, string[] expectedProducers)
    {
        var (extractedEmoji, extractedArtist, extractedTitle, extractedVersion, extractedFeatures, extractedProducers) =
            NameUtilities.ExtractNameValues([name]);
        
        Assert.Equal(extractedEmoji, expectedEmoji);
        Assert.Equal(extractedArtist, expectedArtist);
        Assert.Equal(extractedTitle, expectedTitle);
        Assert.Equal(extractedVersion, expectedVersion);
    }
    
    [Theory]
    [InlineData(
        "(prod. Lil 88 & Brandon Dalton)",
        new string[]{},
        new string[]{ "Lil 88", "Brandon Dalton" })]
    
    [InlineData(
        "(with Destroy Lonely) (prod. F1LTHY & AM)",
        new string[]{ "Destroy Lonely" },
        new string[]{ "F1LTHY", "AM" })]
    
    [InlineData(
        "(prod. Warren Hunter, star boy & Outtatown)",
        new string[]{},
        new string[]{ "Warren Hunter", "star boy", "Outtatown" })]
    
    [InlineData(
        "(feat. Destroy Lonely) (prod. Gab3, Jonah Abraham & KP Beatz)",
        new string[]{ "Destroy Lonely" },
        new string[]{ "Gab3", "Jonah Abraham", "KP Beatz" })]
    public void Regex_FeaturesAndProducers_Correct(string line, string[] expectedFeatures, string[] expectedProducers)
    {
        var (extractedFeatures, extractedProducers) = NameUtilities.ExtractFeaturesAndProducers(line);
        
        Assert.Equal(extractedFeatures, expectedFeatures);
        Assert.Equal(extractedProducers, expectedProducers);
    }
}