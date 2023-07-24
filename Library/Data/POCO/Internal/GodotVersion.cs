using System;
using System.Data;
using System.IO;
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

    [BsonRef("github_versions")]
    public GithubVersion GithubVersion { get; set; }
    [BsonRef("tuxfamily_versions")]
    public TuxfamilyVersion TuxfamilyVersion { get; set; }
    [BsonRef] public CustomEngineDownload CustomEngine { get; set; }

    public string GetDisplayName() => $"Godot {Tag + (IsMono ? " - Mono" : "")}";

    [BsonIgnore]
    public SemanticVersion SemVersion => GithubVersion != null ? GithubVersion.SemVersion : TuxfamilyVersion?.SemVersion;

    public string GetExecutablePath()
    {
        #if GODOT_MACOS || GODOT_OSX
        var exePath = Path.Combine(Location, (IsMono ? "Godot_mono.app" : "Godot.app"), "Contents", "MacOS", ExecutableName);
        #else
        var exePath = Path.Combine(Location, ExecutableName);
        #endif
        return exePath;
    }
}