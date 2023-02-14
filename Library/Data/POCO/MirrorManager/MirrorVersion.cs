using System;
using System.Collections.Generic;
using System.IO;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;
using LiteDB;

namespace GodotManager.Library.Data.POCO.MirrorManager;

public class MirrorVersion
{
    [BsonId]
    public int Id;
    public int MirrorId;

    public string Version;
    public string BaseLocation;
    public VersionUrls Info;

    public List<string> Tags;

    public string DownloadUrl => Platform.Get() switch
    {
        PlatformType.Windows => Path.Combine(BaseLocation, Platform.Is64Bit ? Info.Win64.Url : Info.Win32.Url),
        PlatformType.LinuxBSD => Path.Combine(BaseLocation, Platform.Is64Bit ? Info.Linux64.Url : Info.Linux32.Url),
        PlatformType.MacOS => Path.Combine(BaseLocation, Info.OSX.Url),
        _ => ""
    };

    public int DownloadSize => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? Info.Win64.Size : Info.Win32.Size,
        PlatformType.LinuxBSD => Platform.Is64Bit ? Info.Linux64.Size : Info.Linux32.Size,
        PlatformType.MacOS => Info.OSX.Size,
        _ => -1
    };

    public string ZipFile => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? Info.Win64.Url : Info.Win32.Url,
        PlatformType.LinuxBSD => Platform.Is64Bit ? Info.Linux64.Url : Info.Linux32.Url,
        PlatformType.MacOS => Info.OSX.Url,
        _ => ""
    };
}
