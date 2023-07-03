using System;
using System.IO;
using System.Reflection;
using Godot;
using Godot.Collections;

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
    
    public static string GetDatabaseConnection() => $"Filename={FileUtil.GetUserFolder("godot_manager.dat")}";

    public static Texture2D LoadImage(string path)
    {
        var filePath = path.NormalizePath();
        if (filePath == string.Empty)
        {
            return GD.Load<Texture2D>(path);
        }
        else
        {
            if (!File.Exists(filePath))
                return null;

            if (SixLabors.ImageSharp.Image.DetectFormat(filePath) == null)
                return null;

            var image = Image.LoadFromFile(filePath);
            return ImageTexture.CreateFromImage(image);
        }
    }

    public static string EngineVersion
    {
        get
        {
            Dictionary versionInfo = Engine.GetVersionInfo();
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
        OS.ShellOpen($"\"{url}\"");
    }
}