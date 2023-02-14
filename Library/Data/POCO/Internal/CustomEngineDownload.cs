using System;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class CustomEngineDownload
{
    [BsonId]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public bool NightlyBuild { get; set; }
    public TimeSpan Interval { get; set; }
    public string TagName { get; set; }
    public int DownloadSize { get; set; }
}