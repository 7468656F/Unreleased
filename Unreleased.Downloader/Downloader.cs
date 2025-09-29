using Unreleased.Downloader.Downloaders;

namespace Unreleased.Downloader;

public class Downloader(string zyteApiKey, HttpClient? client = null)
{
    private readonly HttpClient _httpClient = client ?? new HttpClient();

    public async Task<byte[]> DownloadAsync(FileHost host, string fileId)
    {
        BaseDownloader? downloader = null;
        string argument = fileId;
        
        switch (host)
        {
            case FileHost.Pillowcase:
            case FileHost.Froste:
            case FileHost.Pixeldrain:
                argument = host switch
                {
                    FileHost.Pillowcase => "https://api.pillows.su/api/get/" + fileId,
                    FileHost.Pixeldrain => "https://pixeldrain.com/api/file/" + fileId,
                    FileHost.Froste => $"https://music.froste.lol/song/{fileId}/file",
                    _ => throw new NotSupportedException(),
                };
                downloader = new SimpleDownloader(_httpClient);
                break;
            case FileHost.Imgur:
                downloader = new ImgurDownloader(_httpClient, zyteApiKey);
                break;
            case FileHost.Krakenfiles:
                downloader = new KrakenfilesDownloader(_httpClient);
                break;
        }
        
        if (downloader is null)
            throw new NotSupportedException($"The specified host '{host}' is not supported.");
        
        return await downloader.DownloadAsync(argument);
    }
}

public enum FileHost
{
    Pillowcase,
    Pixeldrain,
    Froste,
    Imgur,
    Krakenfiles,
    Invalid,
}