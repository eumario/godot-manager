using Godot;
using Godot.Collections;
using Newtonsoft.Json;

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
	public string CacheLocation; // Location of where the cache file is.
	[JsonProperty]
	public string Url;	// URL downloaded from (Will match Location for Custom)
	[JsonProperty]
	public System.DateTime DownloadedDate; // Date Downloaded (Added for Godot)
	[JsonProperty]
	public bool HideConsole;	// If we should hide the console for Godot Editor.
	[JsonProperty]
	public GithubVersion GithubVersion;
	[JsonProperty]
	public TuxfamilyVersion TuxfamilyVersion;

	public GodotVersion() {
		Id = System.Guid.Empty.ToString();
		Tag = "";
		Location = "";
		Url = "";
		DownloadedDate = System.DateTime.MinValue;
		HideConsole = false;
	}
}