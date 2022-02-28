using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;

namespace Github {

	[JsonObject(MemberSerialization.OptIn)]
	public class Release : Object {
		[JsonProperty]
		public string Url;
		[JsonProperty]
		public string HtmlUrl;
		[JsonProperty]
		public string AssetsUrl;
		[JsonProperty]
		public string UploadUrl;
		[JsonProperty]
		public string TarballUrl;
		[JsonProperty]
		public string ZipballUrl;
		[JsonProperty]
		public int Id;
		[JsonProperty]
		public string NodeId;
		[JsonProperty]
		public string TagName;
		[JsonProperty]
		public string TargetCommitish;
		[JsonProperty]
		public string Name;
		[JsonProperty]
		public string Body;
		[JsonProperty]
		public bool Draft;
		[JsonProperty]
		public bool PreRelease;
		[JsonProperty]
		public DateTime CreatedAt;
		[JsonProperty]
		public DateTime PublishedAt;
		[JsonProperty]
		public Author Author;
		[JsonProperty]
		public Array<Asset> Assets;

		public static Release FromJson(string data) {
			return JsonConvert.DeserializeObject<Release>(data,DefaultSettings.defaultJsonSettings);
		}
	}
}