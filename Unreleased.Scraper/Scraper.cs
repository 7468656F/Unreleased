using System.Net;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;

namespace Unreleased.Scraper;

public class Scraper(string apiKey)
{
    public static HtmlDocument GetHtmlDocument(string content)
    {
        var document = new HtmlDocument();
        document.LoadHtml(content);
        
        return document;
    }
    
    public async Task<string> ScrapePage(string url, bool httpResponseBody, bool followRedirect)
    {
        var handler = new HttpClientHandler() 
        { 
            AutomaticDecompression = DecompressionMethods.All 
        }; 
        var client = new HttpClient(handler); 
        
        var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(apiKey + ":"); 
        var auth = Convert.ToBase64String(bytes); 
        client.DefaultRequestHeaders.Add("Authorization", "Basic " + auth); 
        client.DefaultRequestHeaders.Add("Accept-Encoding", "br, gzip, deflate"); 

        var input = new Dictionary<string, object>{
            { "url", url },
            { "httpResponseBody", httpResponseBody },
            { "followRedirect", followRedirect }, 
        };
        var inputJson = JsonSerializer.Serialize(input); 
        var content = new StringContent(inputJson, Encoding.UTF8, "application/json"); 

        var response = await client.PostAsync("https://api.zyte.com/v1/extract", content); 
        var body = await response.Content.ReadAsByteArrayAsync(); 

        var data = JsonDocument.Parse(body); 
        var base64HttpResponseBody = data.RootElement.GetProperty("httpResponseBody").ToString(); 
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64HttpResponseBody));
    }
}