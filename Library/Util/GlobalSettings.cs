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
    public static string CacheDir { get; set; } = "user://cache".GlobalizePath();
    public static string NewsImagePath => Path.Join(CacheDir, "images", "news");
    public static string NewsAvatarImagePath => Path.Join(CacheDir, "images", "avatar");
    public static string AssetLibraryPath => Path.Join(CacheDir, "assets");
    public static string TemplateLibraryPath => Path.Join(CacheDir, "templates");

    public static string EngineCache => Path.Join(CacheDir, "engines");
    
    public static string EnginePath = "user://versions".GlobalizePath();
    
    public static string NewsCachePath = Path.Join(CacheDir, "news.json");

    public static SemanticVersion GodotManagerVersion = new SemanticVersion(0, 3, 0, "dev");

    public static bool IsFirstRun = true;

    public static void LoadSettings()
    {
        if (File.Exists("user://central_store.json".GlobalizePath()))
        {
            MigrateFrom2X();
            return;
        }
        
        if (!File.Exists("user://settings.json".GlobalizePath()))
            return;
        
        var dict = Godot.Json.ParseString(File.ReadAllText("user://settings.json".GlobalizePath()))
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
        using var fh = File.Open("user://settings.json".GlobalizePath(), FileMode.Create);
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
        
        if (!Directory.Exists(NewsImagePath)) Directory.CreateDirectory(NewsImagePath);
        if (!Directory.Exists(NewsAvatarImagePath)) Directory.CreateDirectory(NewsAvatarImagePath);
        if (!Directory.Exists(EnginePath)) Directory.CreateDirectory(EnginePath);
        if (!Directory.Exists(AssetLibraryPath)) Directory.CreateDirectory(AssetLibraryPath);
        if (!Directory.Exists(TemplateLibraryPath)) Directory.CreateDirectory(TemplateLibraryPath);
        if (!Directory.Exists(EngineCache)) Directory.CreateDirectory(EngineCache);
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