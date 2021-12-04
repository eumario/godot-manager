using Godot;
using Godot.Collections;
using Newtonsoft.Json;

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


	public static GithubVersion FromAPI(Github.Release release) {
		GithubVersion api = new GithubVersion();
		return api;
	}
}