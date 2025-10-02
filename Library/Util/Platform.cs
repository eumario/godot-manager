using Godot;
using Environment = System.Environment;

namespace GodotManager.Library.Util;

[Tool]
public static class Platform
{
    public static string GetName() => OS.GetName();
    public static bool Is64Bit => Environment.Is64BitOperatingSystem;

    public static PlatformType Get() => OS.GetName() switch
    {
        "Windows" => Is64Bit ? PlatformType.Windows64 : PlatformType.Windows32,
        "UWP" => Is64Bit ? PlatformType.Windows64 : PlatformType.Windows32,
        "macOS" => PlatformType.Mac,
        "Linux" => Is64Bit ? PlatformType.Linux64 : PlatformType.Linux32,
        "FreeBSD" => Is64Bit ? PlatformType.Linux64 : PlatformType.Linux32,
        "NetBSD" => Is64Bit ? PlatformType.Linux64 : PlatformType.Linux32,
        "OpenBSD" => Is64Bit ? PlatformType.Linux64 : PlatformType.Linux32,
        "BSD" => Is64Bit ? PlatformType.Linux64 : PlatformType.Linux32,
        _ => PlatformType.Unsupported
    };
}