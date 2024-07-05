using Godot;
using Godot.Collections;
using GodotManager.libs.data.Internal;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CentralStoreData : Object {
	[JsonProperty] public Array<ProjectFile> Projects;
	[JsonProperty] public Array<GodotVersion> Versions;
	[JsonProperty] public Array<GithubVersion> GHVersions;
	[JsonProperty] public Array<MirrorSite> Mirrors;
	[JsonProperty] public Dictionary<int, Array<MirrorVersion>> MRVersions;
	[JsonProperty] public Array<Category> Categories;
	[JsonProperty] public Array<int> PinnedCategories;
	[JsonProperty] public Array<AssetPlugin> Plugins;
	[JsonProperty] public Array<AssetProject> Templates;
	[JsonProperty] public Array<CustomEngineDownload> CustomEngines;
	[JsonProperty] public Array<AuthorEntry> AuthorEntries;
	[JsonProperty] public Settings Settings;

	public CentralStoreData() {
		Projects = new Array<ProjectFile>();
		Versions = new Array<GodotVersion>();
		GHVersions = new Array<GithubVersion>();
		Mirrors = new Array<MirrorSite>();
		MRVersions = new Dictionary<int, Array<MirrorVersion>>();
		Categories = new Array<Category>();
		PinnedCategories = new Array<int>();
		Plugins = new Array<AssetPlugin>();
		Templates = new Array<AssetProject>();
		CustomEngines = new Array<CustomEngineDownload>();
		AuthorEntries = new Array<AuthorEntry>();
		Settings = new Settings();
	}
}