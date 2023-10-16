using System;
using System.Collections.Generic;
using System.IO;
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
            pf.SetValue("application/config/name", $"{Name}");
            pf.SetValue("application/config/description", $"{Description}");
            pf.SetValue("application/config/icon", $"{Icon}");
            pf.Save();
        }
        else
        {
            throw new FileLoadException("Invalid Project File Format.");
        }
    }
}