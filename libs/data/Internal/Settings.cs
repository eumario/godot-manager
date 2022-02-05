using Godot;
using Godot.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Settings : Object {
	[JsonProperty]
	public string ProjectPath;
	[JsonProperty]
	public string DefaultEngine;
	[JsonProperty]
	public string EnginePath;
	[JsonProperty]
	public string CachePath;
	[JsonProperty]
	public string LastView;
	[JsonProperty]
	public string DefaultView;
	[JsonProperty]
	public System.DateTime LastCheck;
	[JsonProperty]
	public bool CheckForUpdates;
	[JsonProperty]
	public System.TimeSpan CheckInterval;
	[JsonProperty]
	public bool CloseManagerOnEdit;
	[JsonProperty]
	public bool SelfContainedEditors;
	[JsonProperty]
	public Array<string> ScanDirs;
	[JsonProperty]
	public Array<Dictionary<string, string>> AssetMirrors;
	[JsonProperty]
	public Array<Dictionary<string, string>> EngineMirrors;

	[JsonProperty]
	public Dictionary<string, string> CurrentAssetMirror;
	[JsonProperty]
	public Dictionary<string, string> CurrentEngineMirror;

	public Settings() {
		ProjectPath = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects");
		DefaultEngine = System.Guid.Empty.ToString();
		EnginePath = "user://versons";
		CachePath = "user://cache";
		LastView = "ListView";
		DefaultView = "ListView";
		CheckForUpdates = true;
		CloseManagerOnEdit = true;
		SelfContainedEditors = true;
		LastCheck = System.DateTime.UtcNow.AddDays(-1);
		CheckInterval = System.TimeSpan.FromDays(1);
		ScanDirs = new Array<string>();
		AssetMirrors = new Array<Dictionary<string, string>>();
		EngineMirrors = new Array<Dictionary<string, string>>();
		CurrentAssetMirror = new Dictionary<string, string>();
		CurrentEngineMirror = new Dictionary<string, string>();
	}

	public void SetupDefaultValues() {
		Dictionary<string, string> data = new Dictionary<string, string>();

		// Scan Directories (Default Project path added)
		ScanDirs.Add(ProjectPath);

		// Asset Library Mirrors
		data["godotengine.org"] = "godotengine.org";
		AssetMirrors.Add(data.Duplicate());
		CurrentAssetMirror = data.Duplicate();
		data.Clear();
		data["localhost"] = "localhost";
		AssetMirrors.Add(data.Duplicate());
		data.Clear();

		// Engine Mirrors
		data["Github"] = "https://github.com/godotengine/godot";
		EngineMirrors.Add(data.Duplicate());
		data.Clear();
		data["Tuxfamily"] = "https://downloads.tuxfamily.org/godotengine/";
		EngineMirrors.Add(data.Duplicate());
		data.Clear();
	}
}