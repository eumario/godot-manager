using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Image = SixLabors.ImageSharp.Image;

namespace GodotManager.Library.Network;

public class ImageDownloader : IDisposable
{

    private DownloadInstance _download;
    private string _outputPath;

    public event EventHandler<string> DownloadCompleted;
    public event EventHandler DownloadCancelled;
    public event EventHandler DownloadFailed;

    public bool Started = false;
    public bool Finished = false;
    public string Tag = string.Empty;

    public ImageDownloader(Uri uri, string tag = "", string outputPath = "")
    {
        _download = new  DownloadInstance(uri);
        _outputPath = outputPath;

        _download.Cancelled += (sender, args) => DownloadCancelled?.Invoke(this, EventArgs.Empty);
        _download.Failed += (sender, args) => DownloadFailed?.Invoke(this, EventArgs.Empty);
        _download.Completed += async (sender, data) =>
        {
            var memStream = new MemoryStream(data);
            if (_outputPath.GetExtension() == "")
            {
                var format = await Image.DetectFormatAsync(memStream);
                _outputPath += $".{format.FileExtensions.First()}";
                memStream.Seek(0, SeekOrigin.Begin);
            }
            await File.WriteAllBytesAsync(_outputPath, memStream.ToArray());
            Finished = true;
            DownloadCompleted?.Invoke(this, _outputPath);
        };
    }

    public void DownloadImage()
    {
        Started = true;
        _download.StartDownload();
    }

    public void CancelDownload() => _download.CancelDownload();

    public void Dispose() => _download.Dispose();
}