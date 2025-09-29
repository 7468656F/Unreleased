namespace Unreleased.Scraper.Models;

public record Text(string Content, string HeaderValue, Dictionary<string, string>? Links = null) : Cell(HeaderValue);