using Godot;
using Godot.Collections;
using Newtonsoft.Json;

public class MirrorVersion : Object
{
	[JsonProperty] public int Id { get; set; } = 0;
	[JsonProperty] public int MirrorId { get; set; } = 0;

	[JsonProperty] public string Version { get; set; } = string.Empty;
	[JsonProperty] public string BaseLocation { get; set; } = string.Empty;

	[JsonProperty] public string OSX32 { get; set; } = string.Empty;
	[JsonProperty] public string OSX64 { get; set; } = string.Empty;
	[JsonProperty] public string OSXarm64 { get; set; } = string.Empty;
	[JsonProperty] public string Win32 { get; set; } = string.Empty;
	[JsonProperty] public string Win64 { get; set; } = string.Empty;
	[JsonProperty] public string Linux32 { get; set; } = string.Empty;
	[JsonProperty] public string Linux64 { get; set; } = string.Empty;
	[JsonProperty] public string Source { get; set; } = string.Empty;

	[JsonProperty] public int OSX32_Size { get; set; } = 0;
	[JsonProperty] public int OSX64_Size { get; set; } = 0;
	[JsonProperty] public int OSXarm64_Size { get; set; } = 0;
	[JsonProperty] public int Win32_Size { get; set; } = 0;
	[JsonProperty] public int Win64_Size { get; set; } = 0;
	[JsonProperty] public int Linux32_Size { get; set; } = 0;
	[JsonProperty] public int Linux64_Size { get; set; } = 0;
	[JsonProperty] public int Source_Size { get; set; } = 0;

	[JsonProperty] public Array<string> Tags { get; set; } = new Array<string>();
}