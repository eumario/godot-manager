using Godot;
using Godot.Collections;
using DateTime = System.DateTime;
using Newtonsoft.Json;

public class MirrorSite : Object
{
	[JsonProperty] public int Id { get; set; } = 0;
	[JsonProperty] public string Name { get; set; } = string.Empty;
	[JsonProperty] public string BaseUrl { get; set; } = string.Empty;
	[JsonProperty] public int UpdateInterval { get; set; } = 24;
	[JsonProperty] public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
