using Unreleased.Downloader;

namespace Unreleased.Tests;

public class SimpleDownloaderTests
{
    private static readonly HttpClient Client = new HttpClient();
    
    private static async Task<byte[]> GetResult(FileHost host, string fileId)
    {
        var downloader = new Unreleased.Downloader.Downloader("", Client);
        return await downloader.DownloadAsync(host, fileId);
    }
    
    [Theory]
    [InlineData("7865288b17c0b0a692b58bbbc9e16602", 1225587)] // ken - night (narcotics v1)
    [InlineData("b6394caddcb6d8add27f2c11f5d85db1", 1471392)] // ken - private
    public async Task Downloader_Pillowcase_CorrectLength(string fileId, int expectedLength)
    {
        var result = await GetResult(FileHost.Pillowcase, fileId);
        Assert.Equal(expectedLength, result.Length);
    }
    
    [Theory]
    [InlineData("qmSh9h5y", 1008363)] // ken - stars
    [InlineData("mB6MU3Xb", 5769897)] // ken & lone - red
    public async Task Downloader_Pixeldrain_CorrectLength(string fileId, int expectedLength)
    {
        var result = await GetResult(FileHost.Pixeldrain, fileId);
        Assert.Equal(expectedLength, result.Length);
    }
    
    [Theory]
    [InlineData("58cc617c48d676e30bd92415d71532d8", 7555056)] // carti - dior
    [InlineData("7783802a3b41eb7104e11e6f744472ee", 4155648)] // carti - mojo jojo v1
    public async Task Downloader_Froste_CorrectLength(string fileId, int expectedLength)
    {
        var result = await GetResult(FileHost.Froste, fileId);
        Assert.Equal(expectedLength, result.Length);
    }
    
    [Theory]
    [InlineData("2VPTvIC", 47416400)] // ken - molly
    public async Task Downloader_Imgur_CorrectLength(string fileId, int expectedLength)
    {
        var result = await GetResult(FileHost.Imgur, fileId);
        Assert.Equal(expectedLength, result.Length);
    }
}