using Unreleased.Downloader.Utilities;
using Unreleased.SheetsAPI.Scraper;

namespace Unreleased.Downloader.Downloaders;

public class KrakenfilesDownloader(HttpClient httpClient) : BaseDownloader(httpClient, null)
{
    public override async Task<byte[]> DownloadAsync(string fileId)
    {
        var jsonData = await HttpUtils.ReadUrlAsJson($"https://krakenfiles.com/json/{fileId}", HttpClient);
        var uploadDate = jsonData.RootElement.GetProperty("uploadDate").GetString();
        var serverUrl = jsonData.RootElement.GetProperty("serverUrl").GetString();
        var type = jsonData.RootElement.GetProperty("type").GetString();
        
        var url = serverUrl + $"/uploads/{uploadDate}/{fileId}/{type}.m4a";
        
        return await HttpUtils.ReadUrlAsBytes(url, HttpClient);
    }
}