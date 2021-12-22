using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public class AssetResult : Object {
		[JsonProperty]
		public string AssetId;
		[JsonProperty]
		public string Title;
		[JsonProperty]
		public string Author;
		[JsonProperty]
		public string AuthorId;
		[JsonProperty]
		public string Category;
		[JsonProperty]
		public string GodotVersion;
		[JsonProperty]
		public string Rating;
		[JsonProperty]
		public string Cost;
		[JsonProperty]
		public string SupportLevel;
		[JsonProperty]
		public string IconUrl;
		[JsonProperty]
		public string Version;
		[JsonProperty]
		public string VersionString;
		[JsonProperty]
		public string ModifyDate;
	}
}