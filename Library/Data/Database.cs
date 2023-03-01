using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Godot;
using GodotManager.Library.Data.POCO.AssetLib;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Data.POCO.MirrorManager;
using GodotManager.Library.Utility;
using LiteDB;

namespace GodotManager.Library.Data;

public class Database
{
    private static Database _instance;

    protected Database()
    {
        SetupGodotTypes();
        SetupDatabase();
        var node = new Node();
        node.Name = "Monitor";
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        sceneTree.Root.AddChild(node);
        node.TreeExited += () =>
        {
            if (_database == null) return;
            _settingsInstance.LastPosition = DisplayServer.WindowGetPosition();
            _settingsInstance.LastSize = DisplayServer.WindowGetSize();
            if (_settings.Count() == 0)
                _settings.Insert(_settingsInstance);
            else
                _settings.Update(_settingsInstance);
            _database.Checkpoint();
            _database.Dispose();
        };
    }

    public static Database Instance => _instance ??= new Database();
    public static Settings Settings => Instance._settingsInstance;

    private LiteDatabase _database;
    
    private ILiteCollection<Settings> _settings;
    
    private ILiteCollection<ProjectFile> _projects;

    private ILiteCollection<GodotVersion> _versions;
    
    private ILiteCollection<AssetPlugin> _plugins;
    private ILiteCollection<AssetProject> _templates;

    private ILiteCollection<MirrorSite> _mirrors;
    private ILiteCollection<AssetMirror> _assetMirrors;

    private ILiteCollection<GithubVersion> _githubVersions;
    private ILiteCollection<MirrorVersion> _mirrorVersions;
    private ILiteCollection<CustomEngineDownload> _customEngines;

    private ILiteCollection<Category> _categories;
    
    private Settings _settingsInstance;

    private void SetupGodotTypes()
    {
        BsonMapper.Global.RegisterType<Vector2I>(
            i =>
            {
                var doc = new BsonDocument();
                doc["x"] = i.X;
                doc["y"] = i.Y;
                return doc;
            },
            bson => new Vector2I(bson["x"].AsInt32, bson["y"].AsInt32)
        );
    }

    public void SetupDatabase()
    {
        if (!Directory.Exists(FileUtil.GetUserFolder()))
            Directory.CreateDirectory(FileUtil.GetUserFolder());
        GD.Print(Util.GetDatabaseConnection());
        _database = new LiteDatabase(Util.GetDatabaseConnection());
        _settings = _database.GetCollection<Settings>("settings");
        _projects = _database.GetCollection<ProjectFile>("projects");
        _versions = _database.GetCollection<GodotVersion>("versions");
        _plugins = _database.GetCollection<AssetPlugin>("plugins");
        _templates = _database.GetCollection<AssetProject>("templates");
        _mirrors = _database.GetCollection<MirrorSite>("mirrors");
        _assetMirrors = _database.GetCollection<AssetMirror>("asset_mirrors");
        _githubVersions = _database.GetCollection<GithubVersion>("github_versions");
        _mirrorVersions = _database.GetCollection<MirrorVersion>("mirror_versions");
        _customEngines = _database.GetCollection<CustomEngineDownload>("custom_engines");
        _categories = _database.GetCollection<Category>("categories");

        if (_settings.Count() == 0)
            SetupDefaultSettings();
        else
            _settingsInstance = _settings.Query().First();
    }

    private void SetupDefaultSettings()
    {
        var godot = new AssetMirror() { Name = "godotengine.org", Url = "https://godotengine.org/asset-library/api" };
        var local = new AssetMirror() { Name = "localhost", Url = "http://localhost/asset-library/api" };
        _assetMirrors.Insert(godot);
        _assetMirrors.Insert(local);
        var settings = new Settings
        {
            //Id = Guid.NewGuid(),
            FirstTimeRun = true,
            DefaultEngine = null,
            ProjectPath = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects").NormalizePath(),
            EnginePath = FileUtil.GetUserFolder("versions"),
            CachePath = FileUtil.GetUserFolder("cache"),
            LastView = "List View",
            DefaultView = "List View",
            CheckForEngineUpdates = true,
            CheckForProgramUpdates = true,
            LastCheck = DateTime.UtcNow.AddDays(-1),
            LastUpdateCheck = DateTime.UtcNow.AddDays(-1),
            LastMirrorCheck = DateTime.UtcNow.AddDays(-1),
            LastMirrorUpdateCheck = new(),
            UpdateCheckInterval = TimeSpan.FromDays(1),
            UseSystemTitlebar = false,
            UseLastMirror = false,
            ScanDirs = new() { OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects").NormalizePath() },
            UseProxy = false,
            ProxyHost = "localhost",
            ProxyPort = 8080,
            ShortcutMade = false,
            ShortcutRoot = false,
            CurrentAssetMirror = godot,
            LastEngineMirror = 0,
            LocalAddonCount = 0,
            SettingsShare = new()
        };
        _settings.Insert(settings);
        _database.Checkpoint();
        _settingsInstance = settings;
    }

    #region Project Functions
    public static bool HasProject(string name) =>
        Instance._projects.Query().Where(pf => pf.Location == name).FirstOrDefault() != null;

    public static ProjectFile GetProject(string name) =>
        Instance._projects.Query().Where(pf => pf.Location == name).First();

    public static void AddProject(ProjectFile projectFile)
    {
        Instance._projects.Insert(projectFile);
        Instance._database.Checkpoint();
    }

    public static void RemoveProject(ProjectFile projectFile)
    {
        Instance._projects.Delete(projectFile.Id);
        Instance._database.Checkpoint();
    }

    public static ProjectFile[] AllProjects() =>
        Instance._projects.Query().ToArray();
    #endregion
    
    #region Asset Plugin Functions
    #endregion
    
    #region Asset Template Functions
    #endregion

    #region Category Functions
    #endregion

    #region GodotVersion Functions
    public static GodotVersion FindVersion(int id) =>
        Instance._versions.Query().Where(gv => gv.Id == id).FirstOrDefault();

    public static GodotVersion GetVersion(int id) =>
        Instance._versions.Query().Where(gv => gv.Id == id).First();

    public static bool HasVersion(string tag) =>
        Instance._versions.Query().Where(gv => gv.Tag == tag).FirstOrDefault() != null;

    public static List<GodotVersion> AllVersions() =>
        Instance._versions.Query().ToList();
    #endregion
}