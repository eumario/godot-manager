using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GodotManager.Library.Data;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Managers;

public class ImageDownloader : IDisposable
{
    private HttpClient _httpClient;
    private Uri _uri;
    private readonly CancellationTokenSource _cancel;

    public event EventHandler<string> DownloadCompleted;
    public event EventHandler DownloadCancelled;
    public event EventHandler DownloadFailed;

    public bool Started = false;
    public bool Finished = false;
    public string Tag = string.Empty;

    public ImageDownloader(Uri uri, string tag = "")
    {
        if (Database.Settings.UseProxy)
        {
            _httpClient = new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy(Database.Settings.ProxyHost, Database.Settings.ProxyPort),
                UseProxy = Database.Settings.UseProxy
            });
        }
        else
            _httpClient = new HttpClient();

        var product = new ProductInfoHeaderValue("Godot-Manager", $"{Versions.GodotManager}");
        var comment = new ProductInfoHeaderValue($"(Platform: {Platform.GetName()})");
        
        _httpClient.DefaultRequestHeaders.UserAgent.Add(product);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(comment);
        
        _uri = uri;
        Tag = tag;
        _cancel = new CancellationTokenSource();
    }

    public async Task DownloadImage()
    {
        var output = Database.Settings.CachePath.Join("images", "news", _uri.AbsolutePath.GetFilename());
        Started = true;
        Task.Run(async () =>
        {
            try
            {
                using var response = _httpClient.GetAsync(_uri, HttpCompletionOption.ResponseHeadersRead, _cancel.Token)
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
                    }

                } while (isMoreToRead);

                if (!Directory.Exists(Path.GetDirectoryName(output)))
                    Directory.CreateDirectory(Path.GetDirectoryName(output)!);

                await File.WriteAllBytesAsync(output, memStream.ToArray());

                Finished = true;

                DownloadCompleted?.Invoke(this, output);
            }
            catch (OperationCanceledException)
            {
                DownloadCancelled?.Invoke(this, EventArgs.Empty);
            }
            catch (HttpRequestException)
            {
                DownloadFailed?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}