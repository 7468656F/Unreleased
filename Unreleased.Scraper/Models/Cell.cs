namespace Unreleased.Scraper.Models;

public abstract record Cell(string HeaderValue)
{
    public readonly string HeaderValue = HeaderValue;
};