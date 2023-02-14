using System;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class AssetMirror
{
    [BsonId]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}