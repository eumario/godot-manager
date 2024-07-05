using Godot;

namespace GodotManager.libs.data.Internal
{
    public class AuthorEntry : Object
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }

        public AuthorEntry(string name = "", string avatarUrl = "")
        {
            Name = name;
            AvatarUrl = avatarUrl;
        }

        public bool HasAll() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(AvatarUrl);
    }
}