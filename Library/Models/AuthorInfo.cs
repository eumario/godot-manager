namespace GodotManager.Library.Models;

public class AuthorInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    
    public bool HasAll() => !string.IsNullOrEmpty(AvatarUrl) && !string.IsNullOrEmpty(Name);
}