using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;

[JsonObject(MemberSerialization.OptIn)]
public class UpdateCheck : Object
{
	[JsonProperty] public DateTime LastCheck { get; set; }
}