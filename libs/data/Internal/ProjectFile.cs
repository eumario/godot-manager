using Godot;
using Newtonsoft.Json;
using DateTime = System.DateTime;

[JsonObject(MemberSerialization.OptIn)]
public class ProjectFile : Godot.Object {
	[JsonProperty]
	public string Icon;
	[JsonProperty]
	public string Name;
	[JsonProperty]
	public string Description;
	[JsonProperty]
	public string Location;
	[JsonProperty]
	public string GodotVersion;
	[JsonProperty]
	public int CategoryId;
	[JsonProperty]
	public bool Favorite;
	[JsonProperty]
	public DateTime LastAccessed;

	public static ProjectFile ReadFromFile(string filePath) {
		ProjectFile projectFile = null;
		ConfigFile project = new ConfigFile();
		var ret = project.Load(filePath);
		if (ret == Error.Ok) {
			if (!project.HasSectionKey("","config_version"))
				return projectFile;
			if (!project.HasSection("application"))
				return projectFile;
			if (!project.HasSectionKey("application","config/name"))
				return projectFile;
			if ((int)project.GetValue("","config_version") == 4) {
				projectFile = new ProjectFile();
				projectFile.Name = (string)project.GetValue("application", "config/name");
				projectFile.Description = (string)project.GetValue("application", "config/description", "No Description");
				projectFile.Location = filePath.NormalizePath();
				projectFile.Icon = (string)project.GetValue("application", "config/icon", "res://icon.png");
			} else {
				GD.PrintErr($"{filePath}: Project Version does not match version 4.");
			}
		} else {
			GD.PrintErr($"Failed to load Project file: {filePath}, Error: {ret}");
		}
		return projectFile;
	}

	public static bool ProjectExists(string filePath) {
		bool ret = false;

		var path = filePath.GetBaseDir();
		var dir = new Directory();
		ret = dir.DirExists(path);
		if (ret) {
			ret = dir.FileExists(filePath);
		}

		return ret;
	}

	public ProjectFile() {
		Icon = "";
		Name = "";
		Description = "";
		Location = "";
		GodotVersion = "";
		CategoryId = -1;
		Favorite = false;
		LastAccessed = DateTime.UtcNow;
	}

	public void UpdateData() {
		ConfigFile pf = new ConfigFile();
		var ret = pf.Load(Location);
		if (ret == Error.Ok) {
			if ((int)pf.GetValue("","config_version") == 4) {
				this.Name = (string)pf.GetValue("application", "config/name");
				this.Description = (string)pf.GetValue("application", "config/description", "No Description");
				this.Icon = (string)pf.GetValue("application","config/icon", "res://icon.png");
			}
		}
	}

	public void WriteUpdatedData() {
		ConfigFile pf = new ConfigFile();
		var ret = pf.Load(Location);
		if (ret == Error.Ok) {
			pf.SetValue("application", "config/name", this.Name);
			pf.SetValue("application", "config/description", this.Description);
			pf.SetValue("application", "config/icon", this.Icon);
			pf.Save(Location);
		}
	}
}