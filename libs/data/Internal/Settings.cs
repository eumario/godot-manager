using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using Guid = System.Guid;

[JsonObject(MemberSerialization.OptIn)]
public class Settings : Object {
	[JsonProperty] public string ProjectPath;
	[JsonProperty] public string DefaultEngine;
	[JsonProperty] public string EnginePath;
	[JsonProperty] public string CachePath;
	[JsonProperty] public string LastView;
	[JsonProperty] public string DefaultView;
	[JsonProperty] public DateTime LastCheck;
	[JsonProperty] public bool CheckForUpdates;
	[JsonProperty] public TimeSpan CheckInterval;
	[JsonProperty] public bool CloseManagerOnEdit;
	[JsonProperty] public bool NoConsole;
	[JsonProperty] public bool SelfContainedEditors;
	[JsonProperty] public bool EnableAutoScan;
	[JsonProperty] public bool FavoritesToggled;
	[JsonProperty] public bool UncategorizedToggled;
	[JsonProperty] public bool UseProxy;
	[JsonProperty] public string ProxyHost;
	[JsonProperty] public int ProxyPort;
	[JsonProperty] public Array<string> ScanDirs;
	[JsonProperty] public Array<Dictionary<string, string>> AssetMirrors;
	[JsonProperty] public Array<Dictionary<string, string>> EngineMirrors;

	[JsonProperty] public Dictionary<string, string> CurrentAssetMirror;
	[JsonProperty] public Dictionary<string, string> CurrentEngineMirror;

	[JsonProperty] public int LocalAddonCount;

	public bool FirstTimeRun = false;

	public Settings() {
		ProjectPath = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects").NormalizePath();			// Done
		DefaultEngine = Guid.Empty.ToString();															// Done
		EnginePath = "user://versions";																	// Done
		CachePath = "user://cache";																		// Done
		LastView = Tr("List View");																		// Done
		DefaultView = Tr("List View");																	// Done
		CheckForUpdates = true;																			// Done
		CloseManagerOnEdit = true;																		// Done
		SelfContainedEditors = true;																	// Done
		EnableAutoScan = false;																			// Done
		FavoritesToggled = false;
		UncategorizedToggled = false;
		NoConsole = true;																				// Done
		LastCheck = DateTime.UtcNow.AddDays(-1);														// Done
		CheckInterval = TimeSpan.FromDays(1);															// Done
		ScanDirs = new Array<string>();																	// Done
		UseProxy = false;
		ProxyHost = "localhost";
		ProxyPort = 8000;
		AssetMirrors = new Array<Dictionary<string, string>>();											// Done
		EngineMirrors = new Array<Dictionary<string, string>>();										// Not Implemented (Version 0.2 Target)
		CurrentAssetMirror = new Dictionary<string, string>();											// Semi-Implemented (Version 0.2 Target)
		CurrentEngineMirror = new Dictionary<string, string>();											// Not Implemented (Version 0.2 Target)
		LocalAddonCount = 0;
	}

	public void SetupDefaultValues() {
		FirstTimeRun = true;
		Dictionary<string, string> data = new Dictionary<string, string>();

		// Scan Directories (Default Project path added)
		ScanDirs.Add(ProjectPath);

		// Asset Library Mirrors
		data["name"] = "godotengine.org";
		data["url"] = "https://godotengine.org/asset-library/api/";
		AssetMirrors.Add(data.Duplicate());
		CurrentAssetMirror = data.Duplicate();
		data.Clear();
		data["name"] = "localhost";
		data["url"] = "http://localhost/asset-library/api/";
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