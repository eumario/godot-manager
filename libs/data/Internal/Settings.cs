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
	[JsonProperty] public DateTime LastMirrorCheck;
	[JsonProperty] public Dictionary<int, UpdateCheck> LastUpdateMirrorCheck;
	[JsonProperty] public bool CheckForUpdates;
	[JsonProperty] public TimeSpan CheckInterval;
	[JsonProperty] public bool UseSystemTitlebar;
	[JsonProperty] public bool UseLastMirror;
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
#if GODOT_X11 || GODOT_LINUXBSD
	[JsonProperty] public bool ShortcutMade;
	[JsonProperty] public bool ShortcutRoot;
#endif

	[JsonProperty] public Dictionary<string, string> CurrentAssetMirror;
	[JsonProperty] public int LastEngineMirror;

	[JsonProperty] public int LocalAddonCount;

	public bool FirstTimeRun = false;

	public Settings() {
		ProjectPath = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects").NormalizePath();
		DefaultEngine = Guid.Empty.ToString();
		EnginePath = "user://versions";
		CachePath = "user://cache";
		LastView = Tr("List View");
		DefaultView = Tr("List View");
		CheckForUpdates = true;
		CloseManagerOnEdit = true;
		SelfContainedEditors = true;
		EnableAutoScan = false;
		FavoritesToggled = false;
		UncategorizedToggled = false;
		NoConsole = true;
		LastCheck = DateTime.UtcNow.AddDays(-1);
		LastMirrorCheck = DateTime.UtcNow.AddDays(-1);
		LastUpdateMirrorCheck = new Dictionary<int, UpdateCheck>();
		CheckInterval = TimeSpan.FromDays(1);
		UseSystemTitlebar = false;
		UseLastMirror = false;
		ScanDirs = new Array<string>();
		UseProxy = false;
		ProxyHost = "localhost";
		ProxyPort = 8000;
		AssetMirrors = new Array<Dictionary<string, string>>();
		ShortcutMade = false;
		ShortcutRoot = false;
		CurrentAssetMirror = new Dictionary<string, string>();
		LastEngineMirror = 0;
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
	}
}