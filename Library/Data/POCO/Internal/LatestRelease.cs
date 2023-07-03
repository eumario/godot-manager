using System;
using System.Text.Json.Serialization;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class LatestRelease
{
    [BsonId]
    public Guid Id { get; set; }
    public int Major { get; set; }
    public Guid Release { get; set; }
    public Guid Prerelease { get; set; }
}