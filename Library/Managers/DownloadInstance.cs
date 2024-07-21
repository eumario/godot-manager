using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GodotManager.Library.Data;
using GodotManager.Library.Utility;
using HttpClient = System.Net.Http.HttpClient;

namespace GodotManager.Library.Managers;

public class DownloadInstance
{
    private readonly HttpClient _client;
    private readonly Uri _address;
    private readonly CancellationTokenSource _cancel;

    public delegate void DownloadProgress(long chunkSize, long totalDownloaded);

    public delegate void DownloadCompleted(byte[] buffer);

    public delegate void DownloadFailed();

    public delegate void DownloadCancelled();

    public event DownloadProgress Progress;
    public event DownloadCompleted Completed;
    public event DownloadFailed Failed;
    public event DownloadCancelled Cancelled;

    public DownloadInstance(Uri uri)
    {
        if (Database.Settings.UseProxy)
        {
            _client = new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy(Database.Settings.ProxyHost, Database.Settings.ProxyPort),
                UseProxy = Database.Settings.UseProxy
            });
        }
        else
            _client = new HttpClient();
        
        var product = new ProductInfoHeaderValue("Godot-Manager", $"{Versions.GodotManager}");
        var comment = new ProductInfoHeaderValue($"(Platform: {Platform.GetName()})");
        _client.DefaultRequestHeaders.UserAgent.Add(product);
        _client.DefaultRequestHeaders.UserAgent.Add(comment);
        _address = uri;
        _cancel = new CancellationTokenSource();
    }

    public DownloadInstance(string url) : this(new Uri(url)) { }

    public void StartDownload()
    {
        Task.Run(async () =>
        {
            try
            {
                using var response = _client.GetAsync(_address, HttpCompletionOption.ResponseHeadersRead, _cancel.Token)
                    .Result;
                response.EnsureSuccessStatusCode();

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                using var memStream = new MemoryStream();
                var totalRead = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;

                do
                {
                    var read = await contentStream.ReadAsync(buffer);
                    if (read == 0) isMoreToRead = false;
                    else
                    {
                        await memStream.WriteAsync(buffer.AsMemory(0, read));

                        totalRead += read;
                        Progress?.Invoke(read, totalRead);
                    }
                } while (isMoreToRead);

                Completed?.Invoke(memStream.ToArray());
            }
            catch (OperationCanceledException)
            {
                Cancelled?.Invoke();
            }
            catch (HttpRequestException)
            {
                Failed?.Invoke();
            }
        });
    }

    public void CancelDownload()
    {
        _cancel.Cancel();
    }
}