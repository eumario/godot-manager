using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GodotManager.Library.Util;

public class DownloadInstance
{
    private readonly HttpClient _client;
    private readonly Uri _address;
    private readonly CancellationTokenSource _cancel;

    public delegate void DownloadProgressChangedHandler(long chunkSize, long totalDownloaded);

    public delegate void DownloadCompletedHandler(byte[] buffer);
    public delegate void DownloadFailedHandler();

    public delegate void DownloadCancelledHandler();

    public event DownloadProgressChangedHandler ProgressChanged;
    public event DownloadCompletedHandler Completed;
    public event DownloadFailedHandler Failed;
    public event DownloadCancelledHandler Cancelled;

    public DownloadInstance(Uri uri)
    {
        if (GlobalSettings.UseProxy)
        {
            _client = new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy(GlobalSettings.ProxyHost, GlobalSettings.ProxyPort),
                UseProxy = true
            });
        }
        else
            _client = new HttpClient();
        
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Godot-Manager", $"{GlobalSettings.GodotManagerVersion}"));
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"(Platform: {Platform.GetName()})"));
        _address = uri;
        _cancel = new CancellationTokenSource();
    }
    
    public DownloadInstance(string url) : this(new Uri(url)) { }

    public async Task<long> GetDownloadSize()
    {
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, _address));
        if (!response.IsSuccessStatusCode) return -1;
        response.Headers.TryGetValues("Content-Length", out var lengths);
        if (lengths == null) return -1;
        return long.TryParse(lengths.First(), out var length) ? length : -1;
    }

    public void StartDownload()
    {
        Task.Run(async () =>
        {
            try
            {
                using var response =
                    _client.GetAsync(_address, HttpCompletionOption.ResponseHeadersRead, _cancel.Token).Result;
                response.EnsureSuccessStatusCode();

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                using var memStream = new MemoryStream();
                var totalRead = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;

                do
                {
                    if (_cancel.Token.IsCancellationRequested) break;
                    var read = await contentStream.ReadAsync(buffer);
                    if (read == 0) isMoreToRead = false;
                    else
                    {
                        await memStream.WriteAsync(buffer.AsMemory(0, read));

                        totalRead += read;

                        ProgressChanged?.Invoke(read, totalRead);
                    }
                } while (isMoreToRead);

                if (_cancel.Token.IsCancellationRequested) return;
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
    
    public void CancelDownload() => _cancel.Cancel();
}