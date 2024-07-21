using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GodotManager.Library.Data;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Managers;

public class HttpApiRequest : IDisposable
{
    private readonly HttpClient Client;
    private readonly Uri Address;
    private readonly CancellationTokenSource Cancel;

    private readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public HttpApiRequest(Uri uri)
    {
        if (Database.Settings.UseProxy)
        {
            Client = new HttpClient(new HttpClientHandler
            {
                Proxy = new WebProxy(Database.Settings.ProxyHost, Database.Settings.ProxyPort),
                UseProxy = Database.Settings.UseProxy
            });
        }
        else
            Client = new HttpClient();

        var product = new ProductInfoHeaderValue("Godot-Manager", $"{Versions.GodotManager}");
        var comment = new ProductInfoHeaderValue($"(Platform: {Platform.GetName()})");
        Client.DefaultRequestHeaders.UserAgent.Add(product);
        Client.DefaultRequestHeaders.UserAgent.Add(comment);
        Address = uri;
        Cancel = new CancellationTokenSource();
    }

    public async Task<T> MakeRequest<T>(Uri uri)
    {
        var data = await MakeRequest(uri);
        var obj = JsonSerializer.Deserialize<T>(data, Options);
        return obj;
    }

    public async Task<T> MakeRequest<T>(string pathAndParameters)
    {
        return await MakeRequest<T>(new Uri(Address, pathAndParameters));
    }

    public async Task<T> MakeRequest<T>()
    {
        return await MakeRequest<T>(Address);
    }

    public async Task<string> MakeRequest(Uri uri)
    {
        using var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, Cancel.Token);
        return await response.Content.ReadAsStringAsync();
    }
    
    public async Task<string> MakeRequest(string pathRequestAndParams)
    {
        return await MakeRequest(new Uri(Address, pathRequestAndParams));
    }
    
    public async Task<string> MakeRequest()
    {
        return await MakeRequest(Address);
    }

    public void Dispose()
    {
        Client?.Dispose();
        Cancel?.Dispose();
    }
}