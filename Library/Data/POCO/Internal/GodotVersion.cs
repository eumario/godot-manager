using System;
using System.IO;
using GodotManager.Library.Data.POCO.MirrorManager;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class GodotVersion
{
    [BsonId]
    public int Id { get; set; }
    public string Tag { get; set; }
    public bool IsMono { get; set; }
    public string Location { get; set; }
    public string ExecutableName { get; set; }
    public string CacheLocation { get; set; }
    public string Url { get; set; }
    public DateTime DownloadDate { get; set; }
    public bool HideConsole { get; set; }
    [BsonRef] public GithubVersion GithubVersion { get; set; }
    [BsonRef] public MirrorVersion MirrorVersion { get; set; }
    [BsonRef] public CustomEngineDownload CustomEngine { get; set; }

    public string GetDisplayName() => $"Godot {Tag + (IsMono ? " - Mono" : "")}";

    public string GetExecutablePath()
    {
        string exePath = "";
        #if GODOT_MACOS || GODOT_OSX
        exePath = Path.Combine(Location, (IsMono ? "Godot_mono.app" : "Godot.app"), "Contents", "MacOS", ExecutableName);
        #else
        exePath = Path.Combine(Location, ExecutableName);
        #endif
        return exePath;
    }
}