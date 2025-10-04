using System.IO;
using Godot;

namespace GodotManager.Library.Util;

public static class GlobalSettings
{
    public static string ProxyHost { get; set; } = "";
    public static int ProxyPort { get; set; } = 0;
    public static bool UseProxy { get; set; } = false;
    public static string CacheDir { get; set; } = ProjectSettings.GlobalizePath("user://cache");
    public static string NewsImagePath => Path.Join(CacheDir, "images", "news");
    public static string NewsAvatarImagePath => Path.Join(CacheDir, "images", "avatar");

    public static SemanticVersion GodotManagerVersion = new SemanticVersion(0, 3, 0, "dev");

}