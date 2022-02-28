using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using Guid = System.Guid;

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
	public DateTime LastCheck;
	[JsonProperty]
	public bool CheckForUpdates;
	[JsonProperty]
	public TimeSpan CheckInterval;
	[JsonProperty]
	public bool CloseManagerOnEdit;
	[JsonProperty]
	public bool NoConsole;
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
		ProjectPath = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects").NormalizePath();
		DefaultEngine = Guid.Empty.ToString();
		EnginePath = "user://versions";
		CachePath = "user://cache";
		LastView = "List View";
		DefaultView = "List View";
		CheckForUpdates = true;
		CloseManagerOnEdit = true;
		SelfContainedEditors = true;
		NoConsole = true;
		LastCheck = DateTime.UtcNow.AddDays(-1);
		CheckInterval = TimeSpan.FromDays(1);
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
		data["name"] = "godotengine.org";
		data["url"] = "https://godotengine.org";
		AssetMirrors.Add(data.Duplicate());
		CurrentAssetMirror = data.Duplicate();
		data.Clear();
		data["name"] = "localhost";
		data["url"] = "http://localhost";
		AssetMirrors.Add(data.Duplicate());
		data.Clear();

		// Engine Mirrors
		data["name"] = "Github";
		data["url"] = "https://github.com/godotengine/godot";
		EngineMirrors.Add(data.Duplicate());
		CurrentEngineMirror = data.Duplicate();
		data.Clear();
		data["name"] = "Tuxfamily";
		data["url"] = "https://downloads.tuxfamily.org/godotengine/";
		EngineMirrors.Add(data.Duplicate());
		data.Clear();
	}
}