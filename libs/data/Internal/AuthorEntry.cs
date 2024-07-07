using Godot;
using Newtonsoft.Json;

namespace GodotManager.libs.data.Internal
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AuthorEntry : Object
    {
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string AvatarUrl { get; set; }

        public AuthorEntry(string name = "", string avatarUrl = "")
        {
            Name = name;
            AvatarUrl = avatarUrl;
        }

        public bool HasAll() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(AvatarUrl);
    }
}