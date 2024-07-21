using System;
using System.Collections.Generic;
using GodotManager.Library.Data.POCO.AssetLib;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class AssetPlugin : AssetGeneral
{
    [BsonId]
    public int Id { get; set; }
    public List<string> InstallFiles { get; set; }
}
