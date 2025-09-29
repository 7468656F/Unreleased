namespace Unreleased.Downloader.Downloaders;

public abstract class BaseDownloader(HttpClient httpClient, string? zyteApiKey = null)
{
    protected readonly HttpClient HttpClient = httpClient;

    public abstract Task<byte[]> DownloadAsync(string fileId);
}