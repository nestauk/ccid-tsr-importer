using System.Net;
using System.Text.Json;

public class EndpointReader
{
    public static async Task<JsonDocument> ReadEndpointAsync(string url)
    {
        using var client = new HttpClient();
        var json = await client.GetStringAsync(url);
        return JsonDocument.Parse(json);
    }


}