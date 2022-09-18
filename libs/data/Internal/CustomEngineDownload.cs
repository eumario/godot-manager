using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

[JsonObject(MemberSerialization.OptIn)]
public class CustomEngineDownload : Object
{
    [JsonProperty] public int Id;
    [JsonProperty] public string Name;
    [JsonProperty] public string Url;
    [JsonProperty] public bool NightlyBuild;
    [JsonProperty] public TimeSpan Interval;
    [JsonProperty] public string TagName;

    public CustomEngineDownload()
    {
        Id = -1;
        Name = "";
        Url = "";
        NightlyBuild = false;
        Interval = TimeSpan.Zero;
        TagName = "";
    }
}