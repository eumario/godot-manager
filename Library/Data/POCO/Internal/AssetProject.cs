using System;
using GodotManager.Library.Data.POCO.AssetLib;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class AssetProject
{
    [BsonId]
    public int Id { get; set; }
    public Asset Asset { get; set; }
    public string Location { get; set; }
}