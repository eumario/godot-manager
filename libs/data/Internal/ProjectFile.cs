using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;

[JsonObject(MemberSerialization.OptIn)]
public class ProjectFile : Godot.Object
{
	[JsonProperty] public string Icon;
	[JsonProperty] public string Name;
	[JsonProperty] public string Description;
	[JsonProperty] public string Location;
	[JsonProperty] public string GodotVersion;
	[JsonProperty] public int CategoryId;
	[JsonProperty] public bool Favorite;
	[JsonProperty] public string RenderingEngine;
	[JsonProperty] public DateTime LastAccessed;
	[JsonProperty] public Array<string> Assets;

	public static ProjectFile ReadFromFile(string filePath)
	{
		ProjectFile projectFile = null;
		ProjectConfig project = new ProjectConfig();
		var ret = project.Load(filePath);
		if (ret == Error.Ok)
		{
			if (!project.HasSectionKey("header", "config_version"))
				return projectFile;
			if (!project.HasSection("application"))
				return projectFile;
			if (project.GetValue("header", "config_version") == "3" || project.GetValue("header", "config_version") == "4" || project.GetValue("header", "config_version") == "5")
			{
				projectFile = new ProjectFile();
				projectFile.Name = project.GetValue("application", "config/name", "Unnamed");
				projectFile.Description = project.GetValue("application", "config/description", projectFile.Tr("No Description"));
				projectFile.Location = filePath.NormalizePath();
				projectFile.Icon = project.GetValue("application", "config/icon", "res://icon.png");
				if (project.GetValue("header","config_version") == "3" || project.GetValue("header", "config_version") == "4")
					projectFile.RenderingEngine = project.GetValue("rendering", "quality/driver/driver_name", "\"GLES3\"");
				else if (project.GetValue("header", "config_version") == "5")
					projectFile.RenderingEngine = project.GetValue("rendering", "renderer/rendering_method", "\"forward_plus\"");
				else
					projectFile.RenderingEngine = project.GetValue("rendering", "quality/driver/driver_name", "\"GLES3\"");
			}
			else
			{
				GD.PrintErr($"{filePath}: Project Version does not match version 3, 4 or 5.");
			}
		}
		else
		{
			GD.PrintErr($"Failed to load Project file: {filePath}, Error: {ret}");
		}
		return projectFile;
	}

	public static bool ProjectExists(string filePath)
	{
		bool ret = false;

		var path = filePath.GetBaseDir();
		var dir = new Directory();
		ret = dir.DirExists(path);
		if (ret)
		{
			ret = dir.FileExists(filePath);
		}

		return ret;
	}

	public bool HasPlugin(string id)
	{
		return Assets.Contains(id);
	}

	public ProjectFile()
	{
		Icon = "";
		Name = "";
		Description = "";
		Location = "";
		GodotVersion = "";
		CategoryId = -1;
		Favorite = false;
		LastAccessed = DateTime.UtcNow;
	}

	public void UpdateData()
	{
		ProjectConfig pf = new ProjectConfig();
		var ret = pf.Load(Location);
		if (ret == Error.Ok)
		{
			if (pf.GetValue("header", "config_version") == "3" || pf.GetValue("header", "config_version") == "4" || pf.GetValue("header", "config_version") == "5")
			{
				Name = pf.GetValue("application", "config/name");
				Description = pf.GetValue("application", "config/description", Tr("No Description"));
				Icon = pf.GetValue("application", "config/icon", "res://icon.png");
				if (pf.GetValue("header", "config_version") == "3" || pf.GetValue("header", "config_version") == "4")
					RenderingEngine = pf.GetValue("rendering", "quality/driver/driver_name", "\"GLES3\"");
				else if (pf.GetValue("header", "config_version") == "5")
					RenderingEngine = pf.GetValue("rendering", "renderer/rendering_method", "\"forward_plus\"");
				else
					RenderingEngine = pf.GetValue("rendering", "quality/driver/driver_name", "\"GLES3\"");
			}
		}
	}

	public void WriteUpdatedData()
	{
		ProjectConfig pf = new ProjectConfig();
		var ret = pf.Load(Location);
		if (ret == Error.Ok)
		{
			pf.SetValue("application", "config/name", $"\"{Name}\"");
			pf.SetValue("application", "config/description", $"\"{Description}\"");
			pf.SetValue("application", "config/icon", $"\"{Icon}\"");
			if (pf.GetValue("header", "config_version") == "3" || pf.GetValue("header", "config_version") == "4")
				pf.SetValue("rendering", "quality/driver/driver_name", $"\"{RenderingEngine}\"");
			else if (pf.GetValue("header", "config_version") == "5")
				pf.SetValue("rendering", "renderer/rendering_method", $"\"{RenderingEngine}\"");
			else
				pf.GetValue("rendering", "quality/driver/driver_name", $"\"{RenderingEngine}\"");
			pf.Save(Location);
		}
	}
}