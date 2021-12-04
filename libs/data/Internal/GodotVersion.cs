using Godot;
using Godot.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GodotVersion {
	[JsonProperty]
	public string Id; // This will be a UUID
	[JsonProperty]
	public string Tag; // This will be used to display to the user
	[JsonProperty]
	public string Location; // Location of where Godot is
	[JsonProperty]
	public string Url;	// URL downloaded from (Will match Location for Custom)
	[JsonProperty]
	public System.DateTime DownloadedDate; // Date Downloaded (Added for Godot)
	[JsonProperty]
	public bool HideConsole;	// If we should hide the console for Godot Editor.

	public GodotVersion() {
		Id = System.Guid.Empty.ToString();
		Tag = "";
		Location = "";
		Url = "";
		DownloadedDate = System.DateTime.MinValue;
		HideConsole = false;
	}
}