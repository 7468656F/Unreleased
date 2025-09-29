using Unreleased.Downloader.Utilities;

namespace Unreleased.Downloader.Downloaders;

public class SimpleDownloader(HttpClient httpClient) : BaseDownloader(httpClient, null)
{
    public override async Task<byte[]> DownloadAsync(string url)
    {
        return await HttpUtils.ReadUrlAsBytes(url, HttpClient);
    }
}