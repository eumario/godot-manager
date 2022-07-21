using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace GodotManager.Data.JsonDB.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class AssetProject : Object
	{
		[JsonProperty] public AssetLib.Asset Asset;
		[JsonProperty] public string Location;
	}
}