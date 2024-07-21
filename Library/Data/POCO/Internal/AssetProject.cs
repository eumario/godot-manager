using System;
using GodotManager.Library.Data.POCO.AssetLib;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class AssetProject : AssetGeneral
{
    [BsonId]
    public int Id { get; set; }
}