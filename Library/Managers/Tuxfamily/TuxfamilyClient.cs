using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;
using LiteDB;

namespace GodotManager.Library.Managers.Tuxfamily;

public class TuxfamilyClient
{
    private readonly HttpClient _httpClient;

    public TuxfamilyClient(HttpClient client = null)
    {
        _httpClient = client ?? new HttpClient();
        var product = new ProductInfoHeaderValue("Godot-Manager", $"{Versions.GodotManager}");
        var comment = new ProductInfoHeaderValue($"(Platform: {Platform.GetName()})");
        _httpClient.DefaultRequestHeaders.UserAgent.Add(product);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(comment);
    }

    public async Task<List<TuxfamilyVersion>> GetVersions() =>
        await _httpClient.GetFromJsonAsync<List<TuxfamilyVersion>>("https://tuxscrape.cdwgames.com/listings/");

    public async Task<List<LatestRelease>> GetLatestVersions() =>
        await _httpClient.GetFromJsonAsync<List<LatestRelease>>("https://tuxscrape.cdwgames.com/latest/");
}