using System;
using System.Collections.Generic;
using Godot;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class Settings
{
    [BsonId]
    public int Id { get; set; }
    
    // Setup Check
    public bool FirstTimeRun { get; set; }
    
    // Pathing Settings
    public string ProjectPath { get; set; }
    public string EnginePath { get; set; }
    public string CachePath { get; set; }
    
    // UI Settings
    public string DefaultView { get; set; }
    public string LastView { get; set; }
    public bool UseSystemTitlebar { get; set; }
    public bool CloseManagerOnEdit { get; set; }
    public bool EnableAutoScan { get; set; }
    public bool FavoritesToggled { get; set; }
    public bool UncategorizedToggled { get; set; }
    public bool UseLastMirror { get; set; }
    public bool NoConsole { get; set; }
    public bool RememberPosition { get; set; }
    public bool RememberSize { get; set; }
    public Vector2I LastPosition { get; set; }
    public Vector2I LastSize { get; set; }
    
    // Update Checks
    public DateTime LastCheck { get; set; }
    public DateTime LastUpdateCheck { get; set; }
    public DateTime LastMirrorCheck { get; set; }
    public TimeSpan UpdateCheckInterval { get; set; }
    public Dictionary<int, DateTime> LastMirrorUpdateCheck { get; set; }
    public bool CheckForEngineUpdates { get; set; }
    public bool CheckForProgramUpdates { get; set; }
    
    // Network Stuff
    [BsonRef] public AssetMirror CurrentAssetMirror { get; set; }
    public int LastEngineMirror { get; set; }
    
    // Proxy Settings
    public bool UseProxy { get; set; }
    public string ProxyHost { get; set; }
    public int ProxyPort { get; set; }
    
    // General Settings
    [BsonRef] public GodotVersion DefaultEngine { get; set; }
    public bool SelfContainedEditors { get; set; }
    public List<string> ScanDirs { get; set; }
    public int LocalAddonCount { get; set; }
    [BsonRef] public List<GodotVersion> SettingsShare { get; set; }
    public bool ShortcutMade { get; set; }
    public bool ShortcutRoot { get; set; }
}