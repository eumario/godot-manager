using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public class Preview : Object {
		[JsonProperty]
		public string PreviewId;
		[JsonProperty]
		public string Type;
		[JsonProperty]
		public string Link;
		[JsonProperty]
		public string Thumbnail;
	}
}