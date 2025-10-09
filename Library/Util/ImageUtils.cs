using System.IO;
using System.Threading.Tasks;
using Godot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using Image = Godot.Image;

namespace GodotManager.Library.Util;

public static class ImageUtils
{
    public static async Task<Texture2D> LoadImage(string path)
    {
        if (path.StartsWith("res://")) return GD.Load<Texture2D>(path);
        var fullPath = ProjectSettings.GlobalizePath(path);
        if (fullPath == null) return null;

        using var inStream = new FileStream(fullPath, FileMode.Open);
        using var mem = new MemoryStream();
        using var gifmem = new MemoryStream();
        using var newmem = new MemoryStream();
        await inStream.CopyToAsync(mem);
        mem.Seek(0, SeekOrigin.Begin);

        try
        {
            var format = await SixLabors.ImageSharp.Image.DetectFormatAsync(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var buffer = mem.ToArray();
            var img = new Image();

            switch (format)
            {
                case BmpFormat:
                    img.LoadBmpFromBuffer(buffer);
                    break;
                case JpegFormat:
                    img.LoadJpgFromBuffer(buffer);
                    break;
                case PngFormat:
                    img.LoadPngFromBuffer(buffer);
                    break;
                case WebpFormat:
                    img.LoadWebpFromBuffer(buffer);
                    break;
                case GifFormat:
                    var gif = await SixLabors.ImageSharp.Image.LoadAsync(mem);
                    
                    var frame = gif.Frames.CloneFrame(0);
                    await frame.SaveAsPngAsync(gifmem);
                    gifmem.Seek(0, SeekOrigin.Begin);
                    buffer = gifmem.ToArray();
                    img.LoadPngFromBuffer(buffer);
                    break;
                default:
                    var isImg = await SixLabors.ImageSharp.Image.LoadAsync(mem);
                    await isImg.SaveAsPngAsync(newmem);
                    newmem.Seek(0, SeekOrigin.Begin);
                    buffer = newmem.ToArray();
                    img.LoadPngFromBuffer(buffer);
                    break;
            }

            return ImageTexture.CreateFromImage(img);
        }
        catch (UnknownImageFormatException)
        {
            mem.Seek(0, SeekOrigin.Begin);
            var buffer = mem.ToArray();
            var img = new Image();
            img.LoadSvgFromBuffer(buffer);
            return ImageTexture.CreateFromImage(img);
        }

        return null;
    }
}