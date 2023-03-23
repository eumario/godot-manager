using System;
using System.Linq;
using Godot;
using GodotManager.Library.Utility;
using LiteDB;
using Octokit;
using Environment = Godot.Environment;

namespace GodotManager.Library.Data.POCO.Internal;

public class GithubVersion
{
    [BsonId]
    public int Id { get; set; }
    public Release Release { get; set; }
    public VersionUrls Standard { get; set; }
    public VersionUrls CSharp { get; set; }
    
    private readonly string[] fields = new string[]
    {
        "Win32", "Win64", "Linux32", "Linux64", "OSX", "Templates", "Headless", "Server"
    };

    private readonly string[] standard_match = new string[]
    {
        "win32", "win64", "x11.32", "x11.64", "osx", "export_templates.tps", "linux_headless.64", "linux_server.64"
    };

    private readonly string[] dont_match = new string[]
    {
        "mono_win32", "mono_win64", "IgnoredString", "IgnoredString", "mono_osx", "mono_export_templates.tpz",
        "IgnoredString", "IgnoredString"
    };

    private readonly string[] csharp_match = new string[]
    {
        "mono_win32", "mono_win64", "mono_x11_32", "mono_x11_64", "mono_osx", "mono_export_templates.tpz",
        "mono_linux_headless_64", "mono_linux_server_64"
    };

    public string StandardDownloadUrl => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? Standard.Win64.Url : Standard.Win32.Url,
        PlatformType.LinuxBSD => Platform.Is64Bit ? Standard.Linux64.Url : Standard.Linux32.Url,
        PlatformType.MacOS => Standard.OSX.Url,
        _ => string.Empty
    };

    public string CSharpDownloadUrl => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? CSharp.Win64.Url : CSharp.Win32.Url,
        PlatformType.LinuxBSD => Platform.Is64Bit ? CSharp.Linux64.Url : CSharp.Linux32.Url,
        PlatformType.MacOS => CSharp.OSX.Url,
        _ => string.Empty
    };

    public int StandardArchiveSize => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? Standard.Win64.Size : Standard.Win32.Size,
        PlatformType.LinuxBSD => Platform.Is64Bit ? Standard.Linux64.Size : Standard.Linux32.Size,
        PlatformType.MacOS => Standard.OSX.Size,
        _ => -1
    };
    
    public int CSharpArchiveSize => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? CSharp.Win64.Size : CSharp.Win32.Size,
        PlatformType.LinuxBSD => Platform.Is64Bit ? CSharp.Linux64.Size : CSharp.Linux32.Size,
        PlatformType.MacOS => CSharp.OSX.Size,
        _ => -1
    };

    void GatherUrls(Release release = null)
    {
        release ??= Release;
        
        VersionUrls standard = new();
        VersionUrls csharp = new();
        for (var i = 0; i < standard_match.Length; i++)
        {
            var t = from asset in release.Assets
                where asset.Name.FindN(standard_match[i]) > -1 && asset.Name.FindN(dont_match[i]) == -1
                select asset;

            var ghAsset = t.FirstOrDefault();
            if (ghAsset is not null)
            {
                standard[fields[i]].Url = ghAsset.BrowserDownloadUrl;
                standard[fields[i]].Size = ghAsset.Size;
            }

            t = from asset in release.Assets
                where asset.Name.Find(csharp_match[i]) != -1
                select asset;
            ghAsset = t.FirstOrDefault();
            if (ghAsset is not null)
            {
                csharp[fields[i]].Url = ghAsset.BrowserDownloadUrl;
                csharp[fields[i]].Size = ghAsset.Size;
            }
        }

        Standard = standard;
        CSharp = csharp;
    }

    public GithubVersion(Release release)
    {
        Release = release;
        GatherUrls();
    }

    public GithubVersion()
    {
    }
}