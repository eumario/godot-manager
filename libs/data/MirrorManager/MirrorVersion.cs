using Godot;
using Godot.Collections;
using FPath = System.IO.Path;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
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

	public string PlatformDownloadURL {
		get {
			switch(Platform.OperatingSystem) {
				case "Windows":
				case "UWP (Windows 10)":
					return FPath.Combine(BaseLocation, (Platform.Bits == "32") ? Win32 : Win64);
				case "Linux (or BSD)":
					return FPath.Combine(BaseLocation, (Platform.Bits == "32") ? Linux32 : Linux64);
				case "macOS":
					return FPath.Combine(BaseLocation, (Platform.Bits == "64") ? OSX64 : OSX32);
				default:
					return "";
			}
		}
	}

	public string PlatformZipFile {
		get {
			switch(Platform.OperatingSystem) {
				case "Windows":
				case "UWP (Windows 10)":
					return (Platform.Bits == "32") ? Win32 : Win64;
				case "Linux (or BSD)":
					return (Platform.Bits == "32") ? Linux32 : Linux64;
				case "macOS":
					return (Platform.Bits == "64") ? OSX64 : OSX32;
				default:
					return "";
			}
		}
	}

	public int PlatformDownloadSize
	{
		get {
			switch(Platform.OperatingSystem) {
				case "Windows":
				case "UWP (Windows 10)":
					return (Platform.Bits == "32") ? Win32_Size : Win64_Size;
				case "Linux (or BSD)":
					return (Platform.Bits == "32") ? Linux32_Size : Linux64_Size;
				case "macOS":
					return (Platform.Bits == "64") ? OSX64_Size : OSX32_Size;
				default:
					return -1;
			}
		}
	}
}