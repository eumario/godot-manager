using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Linq;

[JsonObject(MemberSerialization.OptIn)]
public class GithubVersion : Object
{
	[JsonProperty]
	public string Name;	// Github.Release.Name
	[JsonProperty]
	public string Page;	// Github.Release.HtmlUrl
	[JsonProperty]
	public string Notes; // Github.Release.Body
	[JsonProperty]
	public string Author; // Github.Release.Author.Login
	[JsonProperty]
	public string Tarball; // Github.Release.TarballUrl
	[JsonProperty]
	public string Zip; // Github.Release.ZipUrl
	[JsonProperty]
	public VersionUrls Standard; // See VersionUrls
	[JsonProperty]
	public VersionUrls Mono; // See VersionUrls


	void GatherUrls(Github.Release release) {
		string[] fields = new string[] {
			"Win32", "Win64", "Linux32", "Linux64", "OSX",
			"Templates", "Headless", "Server"
		};
		string[] standard_match = new string[] {
			"win32", "win64", "x11.32",
			"x11.64", "osx", "export_templates.tpz",
			"linux_headless.64", "linux_server.64"
		};
		string[] standard_not_match = new string[] {
			"mono_win32", "mono_win64", "",
			"", "mono_osx", "mono_export_templates.tpz",
			"", ""
		};
		string[] mono_match = new string[] {
			"mono_win32", "mono_win64", "mono_x11_32",
			"mono_x11_64", "mono_osx", "mono_export_templates.tpz",
			"mono_linux_headless_64", "mono_linux_server_64"
		};

		VersionUrls standard = new VersionUrls();
		VersionUrls mono = new VersionUrls();
		for (int i = 0; i < standard_match.Length; i++) {
			var t = from asset in release.Assets
					where asset.Name.FindN(standard_match[i]) != -1 && asset.Name.FindN(standard_not_match[i]) == -1
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