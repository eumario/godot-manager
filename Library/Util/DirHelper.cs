using System.IO;
using Godot;

namespace GodotManager.Library.Util;

public static class DirHelper
{
    public static string GetConfigFolder(string tag, bool selfContained)
    {
        if (selfContained) return GlobalSettings.EnginePath.PathJoin(tag).PathJoin("editor_data");
        switch (Platform.Get())
        {
            case PlatformType.Linux32:
            case PlatformType.Linux64:
                return OS.GetEnvironment("HOME").PathJoin(".config").PathJoin("godot");
            case PlatformType.Windows32:
            case PlatformType.Windows64:
                return OS.GetEnvironment("APPDATA").PathJoin("Godot");
            case PlatformType.Mac:
                return OS.GetEnvironment("HOME").PathJoin("Library").PathJoin("Application Support").PathJoin("Godot");
        }

        return "";
    }

    public static string GetConfigPath(string tag) => GetConfigFolder(tag, File.Exists(GlobalSettings.EnginePath.PathJoin(tag).PathJoin("._sc_")));

    public static string GetEditorDataPath(string tag, bool selfContained)
    {
        if (selfContained) return GlobalSettings.EnginePath.PathJoin(tag).PathJoin("editor_data");
        switch (Platform.Get())
        {
            case PlatformType.Linux32:
            case PlatformType.Linux64:
                return OS.GetEnvironment("HOME").PathJoin(".local").PathJoin("share").PathJoin("godot");
            case PlatformType.Windows32:
            case PlatformType.Windows64:
                return OS.GetEnvironment("APPDATA").PathJoin("Godot");
            case PlatformType.Mac:
                return OS.GetEnvironment("HOME").PathJoin("Library").PathJoin("Application Support").PathJoin("Godot");
        }

        return "";
    }
    
    public static string GetEditorDataPath(string tag) => GetEditorDataPath(tag, File.Exists(GlobalSettings.EnginePath.PathJoin(tag).PathJoin("._sc_")));

    public static string GetEditorCachePath(string tag, bool selfContained)
    {
        if (selfContained) return GlobalSettings.EnginePath.PathJoin(tag).PathJoin("editor_data");
        switch (Platform.Get())
        {
            case PlatformType.Linux32:
            case PlatformType.Linux64:
                return OS.GetEnvironment("HOME").PathJoin(".cache").PathJoin("godot");
            case PlatformType.Windows32:
            case PlatformType.Windows64:
                return OS.GetEnvironment("TEMP").PathJoin("Godot");
            case PlatformType.Mac:
                return OS.GetEnvironment("HOME").PathJoin("Library").PathJoin("Caches").PathJoin("Godot");
        }

        return "";
    }
}