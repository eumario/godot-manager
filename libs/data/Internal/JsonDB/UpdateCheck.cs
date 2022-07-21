using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;

namespace GodotManager.Data.JsonDB.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class UpdateCheck : Object
	{
		[JsonProperty] public DateTime LastCheck { get; set; }
	}
}