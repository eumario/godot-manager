using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public class ConfigureResult : Object {
		[JsonProperty] public Array<CategoryResult> Categories;
	}
}