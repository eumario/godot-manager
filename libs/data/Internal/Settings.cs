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
	public System.DateTime LastCheck;
	[JsonProperty]
	public System.TimeSpan CheckInterval;
	[JsonProperty]
	public Array<string> ScanDirs;

	public Settings() {
		ProjectPath = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects");
		DefaultEngine = System.Guid.Empty.ToString();
		EnginePath = "user://versons";
		CachePath = "user://cache";
		LastView = "ListView";
		LastCheck = System.DateTime.UtcNow.AddDays(-1);
		CheckInterval = System.TimeSpan.FromDays(1);
		ScanDirs = new Array<string> { OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects") };
	}
}