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
	public int GodotVersion;
	[JsonProperty]
	public int CategoryId;
	[JsonProperty]
	public bool Favorite;
	[JsonProperty]
	public System.DateTime LastAccessed;

	public static ProjectFile ReadFromFile(string filePath) {
		ProjectFile projectFile = null;
		ConfigFile project = new ConfigFile();
		project.Load(filePath);
		if ((int)project.GetValue("","config_version") == 4) {
			projectFile = new ProjectFile();
			projectFile.Name = (string)project.GetValue("application", "config/name");
			projectFile.Description = (string)project.GetValue("application", "config/description");
			projectFile.Location = filePath.NormalizePath();
			projectFile.Icon = (string)project.GetValue("application", "config/icon");
		}

		return projectFile;
	}

	public ProjectFile() {
		Icon = "";
		Name = "";
		Description = "";
		Location = "";
		GodotVersion = -1;
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
				this.Description = (string)pf.GetValue("application", "config/description");
				this.Icon = (string)pf.GetValue("application","config/icon");
			}
		}
	}
}