using System;
using System.Collections.Generic;
using System.IO;
using GodotManager.Library.Data.Godot;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class ProjectFile
{
    [BsonId] public int Id { get; set; }
    public string Icon { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    [BsonRef]
    public GodotVersion GodotVersion { get; set; }
    public bool IsGodot4 { get; set; }
    [BsonRef]
    public Category Category { get; set; }
    public bool Favorite { get; set; }
    public DateTime LastAccessed { get; set; }
    public List<string> Assets { get; set; }

    public static ProjectFile ReadFromFile(string filePath)
    {
        ProjectFile projectFile = null;
        ProjectConfig project = new ProjectConfig();
        project.Load(filePath);

        if (!project.HasSectionKey("header", "config_version")) return null;
        if (!project.HasSection("application")) return null;
        if (!project.HasSectionKey("application", "config/name")) return null;
        if (ValidVersion(project))
        {
            projectFile = new();
            projectFile.Name = project.GetValue("application", "config/name");
            projectFile.Description = project.GetValue("application", "config/description", "No Description");
            projectFile.Location = filePath;
            projectFile.Icon = project.GetValue("application", "config/icon", "res://icon.png");
            projectFile.IsGodot4 = project.GetValue("header", "config_version") == "5";
        }
        else
        {
            throw new FileLoadException("Invalid Project File Format.");
        }

        return projectFile;
    }

    public static bool ValidVersion(ProjectConfig projectConfig) =>
        projectConfig.GetValue("header", "config_version") == "4" ||
        projectConfig.GetValue("header", "config_version") == "5";

    public static bool ProjectExists(string filePath) => File.Exists(filePath);

    public bool HasPlugin(string id) => Assets.Contains(id);

    public void UpdateData()
    {
        ProjectConfig pf = new();
        pf.Load(Location);
        if (ValidVersion(pf))
        {
            Name = pf.GetValue("application", "config/name");
            Description = pf.GetValue("application", "config/description", "No Description");
            Icon = pf.GetValue("application", "config/icon", "res://icon.png");
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
            pf.SetValue("application", "config/name", $"\"{Name}\"");
            pf.SetValue("application","config/description", $"\"{Description}\"");
            pf.SetValue("application","config/icon", $"\"{Icon}\"");
            pf.Save();
        }
        else
        {
            throw new FileLoadException("Invalid Project File Format.");
        }
    }
}