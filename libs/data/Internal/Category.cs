using Godot;
using Godot.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Category : Object {
	[JsonProperty]
	public int Id;
	[JsonProperty]
	public string Name;
	[JsonProperty]
	public bool IsExpanded;
	[JsonProperty]
	public System.DateTime LastAccessed;

	public Category() {
		Id = -1;
		Name = "";
		IsExpanded = true;
		LastAccessed = System.DateTime.UtcNow;
	}
}