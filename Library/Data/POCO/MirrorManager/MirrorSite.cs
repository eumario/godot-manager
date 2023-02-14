using System;
using LiteDB;

namespace GodotManager.Library.Data.POCO.MirrorManager;

public class MirrorSite
{
    [BsonId]
    public int Id;
    public string Name;
    public string BaseUrl;
    public int UpdateInterval;
    public DateTime LastUpdated;
}