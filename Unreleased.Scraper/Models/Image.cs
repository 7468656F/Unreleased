namespace Unreleased.Scraper.Models;

public record Image(string Url, string HeaderValue) : Cell(HeaderValue);