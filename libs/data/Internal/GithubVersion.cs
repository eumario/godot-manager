using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Linq;

[JsonObject(MemberSerialization.OptIn)]
public class GithubVersion : Object
{
	[JsonProperty] public string Name;	// Github.Release.Name
	[JsonProperty] public string Page;	// Github.Release.HtmlUrl
	[JsonProperty] public string Notes; // Github.Release.Body
	[JsonProperty] public string Author; // Github.Release.Author.Login
	[JsonProperty] public string Tarball; // Github.Release.TarballUrl
	[JsonProperty] public string Zip; // Github.Release.ZipUrl
	[JsonProperty] public VersionUrls Standard; // See VersionUrls
	[JsonProperty] public VersionUrls Mono; // See VersionUrls

	public string PlatformDownloadURL {
		get {
			switch(Platform.OperatingSystem) {
				case "Windows":
				case "UWP (Windows 10)":
					return (Platform.Bits == "32") ? Standard.Win32 : Standard.Win64;
				case "Linux (or BSD)":
					return (Platform.Bits == "32") ? Standard.Linux32 : Standard.Linux64;
				case "macOS":
					return Standard.OSX;
				default:
					return "";
			}
		}
	}

	public string PlatformMonoDownloadURL {
		get {
			switch(Platform.OperatingSystem) {
				case "Windows":
				case "UWP (Windows 10)":
					return (Platform.Bits == "32") ? Mono.Win32 : Mono.Win64;
				case "Linux (or BSD)":
					return (Platform.Bits == "32") ? Mono.Linux32 : Mono.Linux64;
				case "macOS":
					return Mono.OSX;
				default:
					return "";
			}
		}
	}

	public int PlatformDownloadSize {
		get {
			switch(Platform.OperatingSystem) {
				case "Windows":
				case "UWP (Windows 10)":
					return (Platform.Bits == "32") ? Standard.Win32_Size : Standard.Win64_Size;
				case "Linux (or BSD)":
					return (Platform.Bits == "32") ? Standard.Linux32_Size : Standard.Linux64_Size;
				case "macOS":
					return Standard.OSX_Size;
				default:
					return -1;
			}
		}
	}

	public int PlatformMonoDownloadSize {
		get {
			switch(Platform.OperatingSystem) {
				case "Windows":
				case "UWP (Windows 10)":
					return (Platform.Bits == "32") ? Mono.Win32_Size : Mono.Win64_Size;
				case "Linux (or BSD)":
					return (Platform.Bits == "32") ? Mono.Linux32_Size : Mono.Linux64_Size;
				case "macOS":
					return Mono.OSX_Size;
				default:
					return -1;
			}
		}
	}


	void GatherUrls(Github.Release release) {
		string[] fields = new string[] {
			"Win32", "Win64", "Linux32", "Linux64", "OSX",
			"Templates", "Headless", "Server"
		};
		//Godot_v3.4-stable_x11.32.zip, Godot_v3.4-stable_x11.64.zip
		string[] standard_match = new string[] {
			"win32", "win64", "x11.32", "x11.64", "osx", 
			"export_templates.tpz", "linux_headless.64", "linux_server.64"
		};
		string[] standard_not_match = new string[] {
			"mono_win32", "mono_win64", "JavaDaHutForYou", "JavaDaHutForYou", "mono_osx",
			"mono_export_templates.tpz", "JavaDaHutForYou", "JavaDaHutForYou"
		};
		string[] mono_match = new string[] {
			"mono_win32", "mono_win64", "mono_x11_32", "mono_x11_64", "mono_osx",
			"mono_export_templates.tpz", "mono_linux_headless_64", "mono_linux_server_64"
		};

		VersionUrls standard = new VersionUrls();
		VersionUrls mono = new VersionUrls();
		for (int i = 0; i < standard_match.Length; i++) {
			var t = from asset in release.Assets
					where asset.Name.FindN(standard_match[i]) > -1 && asset.Name.FindN(standard_not_match[i]) == -1
					select asset;
			if (t.FirstOrDefault() is Github.Asset ghAsset) {
				standard[fields[i]] = ghAsset.BrowserDownloadUrl;
				standard[$"{fields[i]}_Size"] = ghAsset.Size;
			}
			t = from asset in release.Assets
				where asset.Name.FindN(mono_match[i]) != -1
				select asset;
			if (t.FirstOrDefault() is Github.Asset mghAsset) {
				mono[fields[i]] = mghAsset.BrowserDownloadUrl;
				mono[$"{fields[i]}_Size"] = mghAsset.Size;
			}
		}
		Standard = standard;
		Mono = mono;
	}


	public static GithubVersion FromAPI(Github.Release release) {
		GithubVersion api = new GithubVersion();
		api.Name = release.Name;
		api.Page = release.HtmlUrl;
		api.Notes = release.Body;
		api.Author = release.Author.Login;
		api.Tarball = release.TarballUrl;
		api.Zip = release.ZipballUrl;
		api.GatherUrls(release);
		return api;
	}
}