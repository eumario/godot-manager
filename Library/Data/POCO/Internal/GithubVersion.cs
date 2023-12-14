using System;
using System.Linq;
using System.Text;
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
    public string Repo { get; set; }
    public Release Release { get; set; }
    public VersionUrls Standard { get; set; }
    public VersionUrls CSharp { get; set; }
    
    public string Sha512Sums { get; set; }
    public int Sha512Size { get; set; }

    private SemanticVersion _semVersion = null;
    [BsonIgnore]
    public SemanticVersion SemVersion => _semVersion ??= SemanticVersion.Parse(Release.TagName);

    private readonly string[] fields = new string[]
    {
        "Win32", "Win64", "Linux32", "Linux32", "Linux64", "Linux64", "OSX", "OSX", "Templates", "Headless", "Server"
    };

    private readonly string[] standard_match = new string[]
    {
        "win32", "win64", "linux.x86_32", "x11.32", "linux.x86_64", "x11.64", "osx", "macos.universal", "export_templates.tpz", "linux_headless.64", "linux_server.64"
    };

    private readonly string[] dont_match = new string[]
    {
        "mono_win32", "mono_win64", "mono_linux_x86_32", "mono_x11_32", "mono_linux_x86_64", "mono_x11_64", "IgnoredString", "IgnoredString", "mono_osx", "mono_macos.universal", "mono_export_templates.tpz",
        "IgnoredString", "IgnoredString"
    };

    private readonly string[] csharp_match = new string[]
    {
        "mono_win32", "mono_win64", "mono_linux_x86_32", "mono_x11_32", "mono_linux_x86_64", "mono_x11_64", "mono_osx", "mono_macos.universal", "mono_export_templates.tpz",
        "mono_linux_headless_64", "mono_linux_server_64"
    };

    [BsonIgnore]
    public string StandardDownloadUrl => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? Standard.Win64.Url : Standard.Win32.Url,
        PlatformType.LinuxBSD => Platform.Is64Bit ? Standard.Linux64.Url : Standard.Linux32.Url,
        PlatformType.MacOS => Standard.OSX.Url,
        _ => string.Empty
    };

    [BsonIgnore]
    public string CSharpDownloadUrl => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? CSharp.Win64.Url : CSharp.Win32.Url,
        PlatformType.LinuxBSD => Platform.Is64Bit ? CSharp.Linux64.Url : CSharp.Linux32.Url,
        PlatformType.MacOS => CSharp.OSX.Url,
        _ => string.Empty
    };

    [BsonIgnore]
    public int StandardArchiveSize => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? Standard.Win64.Size : Standard.Win32.Size,
        PlatformType.LinuxBSD => Platform.Is64Bit ? Standard.Linux64.Size : Standard.Linux32.Size,
        PlatformType.MacOS => Standard.OSX.Size,
        _ => -1
    };
    
    [BsonIgnore]
    public int CSharpArchiveSize => Platform.Get() switch
    {
        PlatformType.Windows => Platform.Is64Bit ? CSharp.Win64.Size : CSharp.Win32.Size,
        PlatformType.LinuxBSD => Platform.Is64Bit ? CSharp.Linux64.Size : CSharp.Linux32.Size,
        PlatformType.MacOS => CSharp.OSX.Size,
        _ => -1
    };

    public string GetDownloadUrl(bool isMono) => isMono ? CSharpDownloadUrl : StandardDownloadUrl;
    public int GetDownloadSize(bool isMono) => isMono ? CSharpArchiveSize : StandardArchiveSize;

    public string GetTagName(bool isMono)
    {
        var tagBuilder = new StringBuilder();
        var csharpTag = SemVersion.Version.Major < 4 ? "mono" : "dotnet";
        tagBuilder.Append($"Godot-{SemVersion.Version.Major}.{SemVersion.Version.Minor}.{SemVersion.Version.Build}");
        if (SemVersion.Version.Revision > 0) tagBuilder.Append($".{SemVersion.Version.Revision}");
        tagBuilder.Append($"-{SemVersion.SpecialVersion}");
        if (isMono) tagBuilder.Append($"-{csharpTag}");
        return tagBuilder.ToString();
    }
    
    public string GetHumanReadableVersion(bool showMono)
    {
        var tagBuilder = new StringBuilder();
        tagBuilder.Append("Godot v");
        tagBuilder.Append(SemVersion.ToNormalizedStringNoSpecial());
        tagBuilder.Append(" (Stable");
        tagBuilder.Append(showMono ? (SemVersion.Version.Major == 4 ? " Dotnet" : " Mono") : string.Empty);
        tagBuilder.Append(")");
        return tagBuilder.ToString();
    }

    public void GatherUrls(Release release = null)
    {
        release ??= Release;
        Release = release;

        VersionUrls standard = new();
        VersionUrls csharp = new();
        for (var i = 0; i < standard_match.Length; i++)
        {
            var t = from asset in release.Assets
                where asset.Name.Contains(standard_match[i]) && !asset.Name.Contains(dont_match[i])
                select asset;

            var ghAsset = t.FirstOrDefault();
            if (ghAsset is not null)
            {
                standard[fields[i]].Url = ghAsset.BrowserDownloadUrl;
                standard[fields[i]].Size = ghAsset.Size;
            }

            t = from asset in release.Assets
                where asset.Name.Contains(csharp_match[i])
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

        var x = from asset in release.Assets
            where asset.Name.Contains("SHA512-SUMS.txt")
            select asset;

        var sha = x.FirstOrDefault();
        if (sha is not null)
        {
            Sha512Sums = sha.BrowserDownloadUrl;
            Sha512Size = sha.Size;
        }
    }

    public static GithubVersion FromRelease(Release release, string repo)
    {
        var version = new GithubVersion();
        version.GatherUrls(release);
        version.Repo = repo;
        return version;
    }
}