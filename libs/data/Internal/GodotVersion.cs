using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;
using Guid = System.Guid;

[JsonObject(MemberSerialization.OptIn)]
public class GodotVersion : Object {
	[JsonProperty]
	public string Id; // This will be a UUID
	[JsonProperty]
	public string Tag; // This will be used to display to the user
	[JsonProperty]
	public bool IsMono; // This is used to determine if the file downloaded is Mono
	[JsonProperty]
	public string Location; // Location of where Godot is
	[JsonProperty]
	public string ExecutableName; // Name of the Final Executable
	[JsonProperty]
	public string CacheLocation; // Location of where the cache file is.
	[JsonProperty]
	public string Url;	// URL downloaded from (Will match Location for Custom)
	[JsonProperty]
	public DateTime DownloadedDate; // Date Downloaded (Added for Godot)
	[JsonProperty]
	public bool HideConsole;	// If we should hide the console for Godot Editor.
	[JsonProperty]
	public GithubVersion GithubVersion;
	[JsonProperty]
	public TuxfamilyVersion TuxfamilyVersion;

	public GodotVersion() {
		Id = Guid.Empty.ToString();
		Tag = "";
		Location = "";
		Url = "";
		DownloadedDate = DateTime.MinValue;
		HideConsole = false;
	}

	public string GetDisplayName() {
		return $"Godot {Tag + (IsMono ? " - Mono" : "")}";
	}

	public string GetExecutablePath() {
		string exe_path = "";
#if GODOT_MACOS || GODOT_OSX
		exe_path = Location.Join((IsMono ? "Godot_mono.app" : "Godot.app"), "Contents", "MacOS", ExecutableName).NormalizePath();
#else
		exe_path = Location.Join(ExecutableName).NormalizePath();
#endif
		return exe_path;
	}
}