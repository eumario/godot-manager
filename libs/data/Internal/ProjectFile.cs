using Godot;
using Newtonsoft.Json;

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
	public System.DateTime LastAccessed;

	public static ProjectFile ReadFromFile(string filePath) {
		ProjectFile projectFile = null;
		ConfigFile project = new ConfigFile();
		var ret = project.Load(filePath);
		if (ret == Error.Ok) {
			if ((int)project.GetValue("","config_version") == 4) {
				projectFile = new ProjectFile();
				projectFile.Name = (string)project.GetValue("application", "config/name");
				if (project.HasSectionKey("application","config/description"))
					projectFile.Description = (string)project.GetValue("application", "config/description");
				else
					projectFile.Description = "No Description";
				projectFile.Location = filePath.NormalizePath();
				if (project.HasSectionKey("application","config/icon"))
					projectFile.Icon = (string)project.GetValue("application", "config/icon");
				else
					projectFile.Icon = "res://icon.png";
			} else {
				GD.PrintErr($"Project Version does not match version 4.");
			}
		} else {
			GD.PrintErr($"Failed to load Project file: {filePath}, Error: {ret}");
		}
		return projectFile;
	}

	public ProjectFile() {
		Icon = "";
		Name = "";
		Description = "";
		Location = "";
		GodotVersion = "";
		CategoryId = -1;
		Favorite = false;
		LastAccessed = System.DateTime.UtcNow;
	}

	public void UpdateData() {
		ConfigFile pf = new ConfigFile();
		var ret = pf.Load(Location);
		if (ret == Error.Ok) {
			if ((int)pf.GetValue("","config_version") == 4) {
				this.Name = (string)pf.GetValue("application", "config/name");
				if (pf.HasSectionKey("application","config/description"))
					this.Description = (string)pf.GetValue("application", "config/description");
				if (pf.HasSectionKey("application","config/icon"))
					this.Icon = (string)pf.GetValue("application","config/icon");
			}
		}
	}
}