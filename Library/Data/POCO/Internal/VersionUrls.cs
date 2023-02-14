using System;
using LiteDB;

namespace GodotManager.Library.Data.POCO.Internal;

public class VersionUrls
{
    public VersionUrl Win32 { get; set; }
    public VersionUrl Win64 { get; set; }
    public VersionUrl Linux32 { get; set; }
    public VersionUrl Linux64 { get; set; }
    public VersionUrl OSX { get; set; }
    public VersionUrl Templates { get; set; }
    public VersionUrl Headless { get; set; }
    public VersionUrl Server { get; set; }

    public VersionUrl this[string key] =>  key switch
    {
        "Win32" => Win32,
        "Win64" => Win64,
        "Linux32" => Linux32,
        "Linux64" => Linux64,
        "OSX" => OSX,
        "Templates" => Templates,
        "Headless" => Headless,
        "Server" => Server,
        _ => null
    };
}