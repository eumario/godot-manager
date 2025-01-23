using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;
using Image = Godot.Image;

namespace GodotManager.Library.Utility;

public static class Util
{
    private static readonly string[] ByteSizes = new string[] { "B", "KB", "MB", "GB", "TB" };
    
    public static string FormatSize(double bytes)
    {
        double len = bytes;
        int order = 0;
        while (len > 1024 && order < ByteSizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {ByteSizes[order]}";
    }

    public static string Join(this string[] parts, string sep) => string.Join(sep, parts);
    
    public static string GetDatabaseConnection() => $"Filename={FileUtil.GetUserFolder("godot_manager.dat")};Connection=shared";

    public static async Task<Texture2D?> LoadImage(string path)
    {
        if (path.StartsWith("res://")) return GD.Load<Texture2D>(path);
        var fullPath = ProjectSettings.GlobalizePath(path);
        if (fullPath == null) return null;
        
        using var inStream = new FileStream(fullPath, FileMode.Open);
        using var mem = new MemoryStream();
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
                case TgaFormat:
                    img.LoadTgaFromBuffer(buffer);
                    break;
                case WebpFormat:
                    img.LoadWebpFromBuffer(buffer);
                    break;
                case GifFormat:
                    var gif =await SixLabors.ImageSharp.Image.LoadAsync(mem);
                    var gifmem = new MemoryStream();
                    var frame = gif.Frames.CloneFrame(0);
                    await frame.SaveAsPngAsync(gifmem);
                    gifmem.Seek(0, SeekOrigin.Begin);
                    buffer = gifmem.ToArray();
                    img.LoadPngFromBuffer(buffer);
                    break;
                default:
                    // Convert to PNG to load.
                    var isImg = await SixLabors.ImageSharp.Image.LoadAsync(mem);
                    var newmem = new MemoryStream();
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

    public static string EngineVersion
    {
        get
        {
            var versionInfo = Engine.GetVersionInfo();
            return $"{versionInfo["major"]}.{versionInfo["minor"]}.{versionInfo["patch"]}";
        }
    }

    public static SemanticVersion GetVersion(Type type)
    {
        return new SemanticVersion(Assembly.GetAssembly(type)?.GetName().Version ?? new Version(0,0,0,0));
    }

    public static Vector2I ToVector2I(this Vector2 vector)
    {
        var v = vector.Floor();
        return new Vector2I((int)v.X, (int)v.Y);
    }
    
    public static void LaunchWeb(string url)
    {
        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal
        };
        Process.Start(psi);
    }

    public static void RunInMainThread(Action action)
    {
        Callable.From(action).CallDeferred();
    }
}