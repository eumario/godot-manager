using System;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class Category
{
    [BsonId]
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsPinned { get; set; }
    public DateTime LastAccessed { get; set; }
}