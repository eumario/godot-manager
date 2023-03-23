using Godot;
using Environment = System.Environment;

namespace GodotManager.Library.Utility;

public static class Platform
{
    public static PlatformType Get()
    {
        switch (OS.GetName())
        {
            case "Windows":
            case "UWP":
                return PlatformType.Windows;
            case "macOS":
                return PlatformType.MacOS;
            case "Linux":
            case "FreeBSD":
            case "NetBSD":
            case "OpenBSD":
            case "BSD":
                return PlatformType.LinuxBSD;
            default:
                return PlatformType.Unsupported;
        }
    }

    public static string GetName()
    {
        return OS.GetName();
    }

    public static bool Is64Bit => Environment.Is64BitOperatingSystem;
}