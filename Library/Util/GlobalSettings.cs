using System;
using System.IO;
using Godot;
using GodotManager.Library.FileIO;

namespace GodotManager.Library.Util;

public static class GlobalSettings
{
    public static string ProxyHost { get; set; } = "";
    public static int ProxyPort { get; set; } = 0;
    public static bool UseProxy { get; set; } = false;
    public static string CacheDir { get; set; } = ProjectSettings.GlobalizePath("user://cache");
    public static string NewsImagePath => Path.Join(CacheDir, "images", "news");
    public static string NewsAvatarImagePath => Path.Join(CacheDir, "images", "avatar");
    public static string AssetLibraryPath => Path.Join(CacheDir, "assets");
    public static string TemplateLibraryPath => Path.Join(CacheDir, "templates");
    
    public static string EnginePath = ProjectSettings.GlobalizePath("user://versions");
    
    public static string NewsCachePath = Path.Join(CacheDir, "news.json");

    public static SemanticVersion GodotManagerVersion = new SemanticVersion(0, 3, 0, "dev");

    public static bool IsFirstRun = true;

    public static void LoadSettings()
    {
        if (File.Exists(ProjectSettings.GlobalizePath("user://central_store.json")))
        {
            MigrateFrom2X();
            return;
        }
        
        if (!File.Exists(ProjectSettings.GlobalizePath("user://settings.json")))
            return;
        
        var dict = Godot.Json.ParseString(File.ReadAllText(ProjectSettings.GlobalizePath("user://settings.json")))
            .AsGodotDictionary();
        if (new SemanticVersion(dict["GodotManagerVersion"].AsString()) < GodotManagerVersion)
        {
            UpgradeSettings();
        }
        
        ProxyHost = dict["ProxyHost"].AsString();
        ProxyPort = dict["ProxyPort"].AsInt32();
        UseProxy = dict["UseProxy"].AsBool();
        CacheDir = dict["CacheDir"].AsString();
        EnginePath = dict["EnginePath"].AsString();
        IsFirstRun = dict["IsFirstRun"].AsBool();
    }

    public static void SaveSettings()
    {
        using var fh = File.Open(ProjectSettings.GlobalizePath("user://settings.json"), FileMode.Create);
        var dict = new Godot.Collections.Dictionary();
        dict["ProxyHost"] = ProxyHost;
        dict["ProxyPort"] = ProxyPort;
        dict["UseProxy"] = UseProxy;
        dict["CacheDir"] = CacheDir;
        dict["EnginePath"] = EnginePath;
        dict["GodotManagerVersion"] = GodotManagerVersion.ToString();
        dict["IsFirstRun"] = IsFirstRun;
        fh.Write(Godot.Json.Stringify(dict).ToAsciiBuffer());
        fh.Close();
    }

    public static void EnsureDirectories()
    {
        if (!IsFirstRun)
            return;

        if (Directory.Exists(CacheDir)) return;
        Directory.CreateDirectory(NewsImagePath);
        Directory.CreateDirectory(NewsAvatarImagePath);
        Directory.CreateDirectory(EnginePath);
        Directory.CreateDirectory(AssetLibraryPath);
        Directory.CreateDirectory(TemplateLibraryPath);
    }

    private static void UpgradeSettings()
    {
        GD.Print("Upgrading Settings...");
    }

    public static void MigrateFrom2X()
    {
        GD.Print("Migrating from Godot Manager 2.x...");
    }
}