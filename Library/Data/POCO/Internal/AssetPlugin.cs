using System;
using System.Collections.Generic;
using GodotManager.Library.Data.POCO.AssetLib;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class AssetPlugin
{
    [BsonId]
    public int Id { get; set; }
    public Asset Asset { get; set; }
    public string Location { get; set; }
    public List<string> InstallFiles { get; set; }
}
