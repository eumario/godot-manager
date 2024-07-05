using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class AuthorEntry(string name = "", string avatarUrl = "")
{
    [BsonId]
    public int Id { get; set; }
    public string Name { get; set; } = name;
    public string AvatarUrl { get; set; } = avatarUrl;

    public bool HasAll() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(AvatarUrl);
}