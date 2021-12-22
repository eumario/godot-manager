using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public class QueryResult : Object {
		[JsonProperty]
		public Array<AssetResult> Result;
		[JsonProperty]
		public int Page;
		[JsonProperty]
		public int Pages;
		[JsonProperty]
		public int PageLength;
		[JsonProperty]
		public int TotalItems;
	}
}