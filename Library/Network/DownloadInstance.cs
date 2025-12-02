using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GodotManager.Library.Util;

namespace GodotManager.Library.Network;

public class DownloadInstance : IDisposable
{
    private readonly HttpClient _client;
    private readonly Uri _address;
    private readonly CancellationTokenSource _cancel;

    public record ProgressChange(long Read, long Total);

    public event EventHandler<ProgressChange> ProgressChanged;
    public event EventHandler<byte[]> Completed;
    public event EventHandler Failed;
    public event EventHandler Cancelled;

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
        
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GodotManager", $"{GlobalSettings.GodotManagerVersion}"));
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

                        ProgressChanged?.Invoke(this, new ProgressChange(read, totalRead));
                    }
                } while (isMoreToRead);

                if (_cancel.Token.IsCancellationRequested) return;
                Completed?.Invoke(this, memStream.ToArray());
            }
            catch (OperationCanceledException)
            {
                Cancelled?.Invoke(this, EventArgs.Empty);
            }
            catch (HttpRequestException)
            {
                Failed?.Invoke(this, EventArgs.Empty);
            }
        });
    }
    
    public void CancelDownload() => _cancel.Cancel();

    public void Dispose() => _client.Dispose();
}