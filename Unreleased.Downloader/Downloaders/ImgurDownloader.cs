namespace Unreleased.Downloader.Downloaders;

public class ImgurDownloader(HttpClient httpClient, string zyteApiKey) : BaseDownloader(httpClient, zyteApiKey)
{
    public override async Task<byte[]> DownloadAsync(string fileId)
    {
        var url = $"https://imgur.gg/f/{fileId}";
        
        var scraper = new Scraper.Scraper(zyteApiKey);
        var content = await scraper.ScrapePage(
            url,
            true, 
            true);

        var document =  Scraper.Scraper.GetHtmlDocument(content);
        
        var audios = document.DocumentNode.SelectNodes("//audio[normalize-space(@src) != '']");
        var audio = audios.First(x => x.Attributes.Contains("src"));
        
        var downloader = new SimpleDownloader(HttpClient);
        return await downloader.DownloadAsync(audio.Attributes["src"].Value);
    }
}