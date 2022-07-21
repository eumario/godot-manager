using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace GodotManager.Data.JsonDB.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class AssetPlugin : Object
	{
		[JsonProperty] public AssetLib.Asset Asset;
		[JsonProperty] public string Location;
		[JsonProperty] public Array<string> InstallFiles;
	}
}