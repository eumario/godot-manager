using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public class Asset : Object {
		[JsonProperty]
		public string AssetId;
		[JsonProperty]
		public string Type;
		[JsonProperty]
		public string Title;
		[JsonProperty]
		public string Author;
		[JsonProperty]
		public string AuthorId;
		[JsonProperty]
		public string Version;
		[JsonProperty]
		public string VersionString;
		[JsonProperty]
		public string Category;
		[JsonProperty]
		public string CategoryId;
		[JsonProperty]
		public string GodotVersion;
		[JsonProperty]
		public string Rating;
		[JsonProperty]
		public string Cost;
		[JsonProperty]
		public string Description;
		[JsonProperty]
		public string SupportLevel;
		[JsonProperty]
		public string DownloadProvider;
		[JsonProperty]
		public string DownloadCommit;
		[JsonProperty]
		public string BrowseUrl;
		[JsonProperty]
		public string IssuesUrl;
		[JsonProperty]
		public string IconUrl;
		[JsonProperty]
		public string Searchable;
		[JsonProperty]
		public string ModifyDate;
		[JsonProperty]
		public string DownloadUrl;
		[JsonProperty]
		public Array<Preview> Previews;
		[JsonProperty]
		public string DownloadHash;
	}
}