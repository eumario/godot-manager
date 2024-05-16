using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using GodotManager.Library.Data.Godot;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class ProjectFile
{
    #region Notification Events

    public delegate void FileChanged();

    public event FileChanged ProjectChanged;
    #endregion
    #region Private Variables

    private string _icon;
    private string _name;
    private string _description;
    private string _location;
    private string _renderer;
    private string _customUserDir;
    private bool _useCustomUserDir;
    private GodotVersion _godotVersion;
    private Category _category;
    private bool _favorite;
    private DateTime _lastAccessed;
    #endregion
    [BsonId] public int Id { get; set; }

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            ProjectChanged?.Invoke();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            ProjectChanged?.Invoke();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            ProjectChanged?.Invoke();
        }
    }

    public string Location
    {
        get => _location;
        set
        {
            _location = value;
            ProjectChanged?.Invoke();
        }
    }

    public string Renderer
    {
        get => _renderer;
        set
        {
            _renderer = value;
            ProjectChanged?.Invoke();
        }
    }

    public string CustomUserDir
    {
        get => _customUserDir;
        set
        {
            _customUserDir = value;
            ProjectChanged?.Invoke();
        }
    }

    public bool UseCustomUserDir
    {
        get => _useCustomUserDir;
        set
        {
            _useCustomUserDir = value;
            ProjectChanged?.Invoke();
        }
    }

    [BsonRef("versions")]
    public GodotVersion GodotVersion
    {
        get => _godotVersion;
        set
        {
            _godotVersion = value;
            ProjectChanged?.Invoke();
        }
    }

    public bool IsGodot4 { get; set; }

    [BsonRef]
    public Category Category
    {
        get => _category;
        set
        {
            _category = value;
            ProjectChanged?.Invoke();
        }
    }

    public bool Favorite
    {
        get => _favorite;
        set
        {
            _favorite = value;
            ProjectChanged?.Invoke();
        }
    }

    public DateTime LastAccessed
    {
        get => _lastAccessed;
        set
        {
            _lastAccessed = value;
            ProjectChanged?.Invoke();
        }
    }
    public List<string> Assets { get; set; }

    [BsonIgnore]
    public string DataFolder =>
        UseCustomUserDir
            ? OS.GetDataDir().PathJoin(CustomUserDir)
            : OS.GetDataDir().PathJoin("godot").PathJoin("app_userdata").PathJoin(Name);

    public static ProjectFile ReadFromFile(string filePath)
    {
        ProjectFile projectFile = null;
        var project = new ProjectConfig();
        project.Load(filePath);

        if (!project.HasSectionKey("header", "config_version")) return null;
        if (!project.HasSection("application")) return null;
        if (ValidVersion(project))
        {
            projectFile = new();
            projectFile.Name = project.GetValue("application/config/name", "Unnamed");
            projectFile.Description = project.GetValue("application/config/description", "No Description");
            projectFile.Location = filePath;
            projectFile.Icon = project.GetValue("application/config/icon", "res://icon.png");
            projectFile.IsGodot4 = project.GetValue("header/config_version") == "5";
            projectFile.Renderer = projectFile.IsGodot4
                ? project.GetValue("rendering/renderer/rendering_method", "forward_plus")
                : project.GetValue("rendering/quality/driver/driver_name", "GLES3");
            projectFile.UseCustomUserDir = project.GetValue("application/config/use_custom_user_dir", "false") == "true";
            projectFile.CustomUserDir = project.GetValue("application/config/custom_user_dir_name", "");
        }
        else
        {
            throw new FileLoadException("Invalid Project File Format.");
        }

        return projectFile;
    }

    public static bool ValidVersion(ProjectConfig projectConfig) =>
        projectConfig.GetValue("header/config_version") == "4" ||
        projectConfig.GetValue("header/config_version") == "5";

    public static bool ProjectExists(string filePath) => File.Exists(filePath);

    public bool HasPlugin(string id) => Assets.Contains(id);

    public void AddPlugin(string id)
    {
        Assets.Add(id);
        ProjectChanged?.Invoke();
    }

    public void RemovePlugin(string id)
    {
        Assets.Remove(id);
        ProjectChanged?.Invoke();
    }


    public void UpdateData()
    {
        ProjectConfig pf = new();
        pf.Load(Location);
        if (ValidVersion(pf))
        {
            Name = pf.GetValue("application/config/name", "No Name");
            Description = pf.GetValue("application/config/description", "No Description");
            Icon = pf.GetValue("application/config/icon", "res://icon.png");
            UseCustomUserDir = pf.GetValue("application/config/use_custom_user_dir", "false") == "true";
            CustomUserDir = pf.GetValue("application/config/custom_user_dir_name", "");
            Renderer = IsGodot4
                ? pf.GetValue("rendering/renderer/rendering_method", "forward_plus")
                : pf.GetValue("rendering/quality/driver/driver_name", "GLES3");
        }
        else
        {
            throw new FileLoadException("Invalid Project File Format.");
        }
    }

    public void SaveUpdatedData()
    {
        ProjectConfig pf = new();
        pf.Load(Location);
        if (ValidVersion(pf))
        {
            pf.SetValue("application/config/name", $"{Name}", true);
            pf.SetValue("application/config/description", $"{Description}", true);
            pf.SetValue("application/config/icon", $"{Icon}", true);
            pf.SetValue(IsGodot4
                ? "rendering/renderer/rendering_method"
                : "rendering/quality/driver/driver_name",
                Renderer, true);
            pf.SetValue("application/config/use_custom_user_dir", UseCustomUserDir.ToString().ToLowerInvariant());
            pf.SetValue("application/config/custom_user_dir_name", CustomUserDir, true);
            pf.Save();
        }
        else
        {
            throw new FileLoadException("Invalid Project File Format.");
        }
    }
}