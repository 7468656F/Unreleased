using System.Text.Json;

namespace Unreleased.Downloader.Utilities;

public class HttpUtils
{
    private static async Task<HttpResponseMessage> GetResponseAsync(string url, HttpClient? httpClient = null)
    {
        if (httpClient is null)
            httpClient = new HttpClient();
        
        return await httpClient.GetAsync(url);
    }
    
    public static async Task<byte[]> ReadUrlAsBytes(string url, HttpClient? httpClient = null)
    {
        var response = await GetResponseAsync(url, httpClient);
        return await response.Content.ReadAsByteArrayAsync();
    }
    
    public static async Task<string> ReadUrlAsString(string url, HttpClient? httpClient = null)
    {
        var response = await GetResponseAsync(url, httpClient);
        return await response.Content.ReadAsStringAsync();
    }
    
    public static async Task<JsonDocument> ReadUrlAsJson(string url, HttpClient? httpClient = null)
    {
        var content = await ReadUrlAsString(url, httpClient);
        return JsonDocument.Parse(content);
    }
}