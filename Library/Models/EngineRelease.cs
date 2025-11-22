using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using GodotManager.Library.Util;
using Octokit;
namespace GodotManager.Library.Models;

public class EngineRelease
{
    #region Matching Constants
    
    private static readonly PlatformType[] BuildConfigs =
    [
        PlatformType.Windows32, PlatformType.Windows64, PlatformType.Linux32, PlatformType.Linux32, PlatformType.Linux64, PlatformType.Linux64,
        PlatformType.Mac, PlatformType.Mac, PlatformType.GenericTemplate, PlatformType.GenericHeadless, PlatformType.GenericServer
    ];

    private static readonly string[] StandardMatch =
    [
        "win32", "win64", "linux.x86_32", "x11.32", "linux.x86_64", "x11.64", "osx", "macos.universal", "export_templates.tpz", "linux_headless.64", "linux_server.64"
    ];

    private static readonly string[] ExcludeMatch =
    [
        "mono_win32", "mono_win64", "mono_linux_x86_32", "mono_x11_32", "mono_linux_x86_64", "mono_x11_64", "IgnoredString", "IgnoredString", "mono_osx", "mono_macos.universal", "mono_export_templates.tpz",
        "IgnoredString", "IgnoredString"
    ];

    private static readonly string[] DotnetMatch =
    [
        "mono_win32", "mono_win64", "mono_linux_x86_32", "mono_x11_32", "mono_linux_x86_64", "mono_x11_64", "mono_osx", "mono_macos.universal", "mono_export_templates.tpz",
        "mono_linux_headless_64", "mono_linux_server_64"
    ];
    #endregion
    
    #region Database/Class Fields
    public int Id { get; set; }
    public long ReleaseId { get; set; }
    public string Repo { get; set; }
    [NotMapped]
    public Release Release { get; set; }
    public SemanticVersion Version { get; set; }
    public Dictionary<PlatformType, VersionUrl> StandardUrls { get; set; } = [];
    public Dictionary<PlatformType, VersionUrl> DotnetUrls { get; set; } = [];
    public VersionUrl Sha512Sum { get; set; } = new VersionUrl("", 0);
    #endregion

    #region Helper Fields

    [NotMapped]
    public string StandardUrl => Platform.Get() == PlatformType.Unsupported ? "" :
        StandardUrls.ContainsKey(Platform.Get()) ? StandardUrls[Platform.Get()].Url : "";

    [NotMapped]
    public string DotnetUrl => Platform.Get() == PlatformType.Unsupported ? "" :
        DotnetUrls.ContainsKey(Platform.Get()) ? DotnetUrls[Platform.Get()].Url : "";

    [NotMapped]
    public string TemplateUrl => StandardUrls.ContainsKey(PlatformType.GenericTemplate)
        ? StandardUrls[PlatformType.GenericTemplate].Url : "";

    [NotMapped]
    public string DotnetTemplateUrl => DotnetUrls.ContainsKey(PlatformType.GenericTemplate)
        ? DotnetUrls[PlatformType.GenericTemplate].Url : "";

    [NotMapped]
    public int StandardSize => Platform.Get() == PlatformType.Unsupported ? 0 :
        StandardUrls.ContainsKey(Platform.Get()) ? StandardUrls[Platform.Get()].Size : 0;

    [NotMapped]
    public int DotnetSize => Platform.Get() == PlatformType.Unsupported ? 0 :
        DotnetUrls.ContainsKey(Platform.Get()) ? DotnetUrls[Platform.Get()].Size : 0;

    [NotMapped]
    public int TemplateSize => StandardUrls.ContainsKey(PlatformType.GenericTemplate)
        ? StandardUrls[PlatformType.GenericTemplate].Size : 0;

    [NotMapped]
    public int DotnetTemplateSize => DotnetUrls.ContainsKey(PlatformType.GenericTemplate)
        ? DotnetUrls[PlatformType.GenericTemplate].Size : 0;
    #endregion

    #region Public API

    public bool IsGodot3() => Version.Version.Major == 3;
    public bool IsGodot4() => Version.Version.Major == 4;
    
    public string GetDownloadUrl(bool isDotnet = false) => isDotnet ? DotnetUrl : StandardUrl;
    public int GetDownloadSize(bool isDotnet = false) => isDotnet ? DotnetSize : StandardSize;

    public string GetTagName(bool showDotnet = false)
    {
        var tagBuilder = new StringBuilder();
        var tag = Version.Version.Major < 4 ? "mono" : "dotnet";
        tagBuilder.Append($"Godot-{Version.Version.Major}.{Version.Version.Minor}.{Version.Version.Build}");
        if (Version.Version.Revision > 0) tagBuilder.Append($".{Version.Version.Revision}");
        tagBuilder.Append($"-{Version.SpecialVersion}");
        if (showDotnet) tagBuilder.Append($"{tag}");
        return tagBuilder.ToString();
    }

    public string GetHumanReadableVersion(bool showDotnet = false)
    {
        var tagBuilder = new StringBuilder();
        tagBuilder.Append("Godot v");
        tagBuilder.Append(Version.ToNormalizedStringNoSpecial());
        tagBuilder.Append($" ({Version.SpecialVersion}");
        tagBuilder.Append(showDotnet ? (Version.Version.Major == 4 ? " Dotnet" : " Mono") : string.Empty);
        tagBuilder.Append(")");
        return tagBuilder.ToString();
    }

    public void GatherUrls()
    {
        for (var i = 0; i < StandardMatch.Length; i++)
        {
            var t = from asset in Release.Assets
                where asset.Name.Contains(StandardMatch[i]) && !asset.Name.Contains(ExcludeMatch[i])
                select asset;

            var ghAsset = t.FirstOrDefault();
            if (ghAsset is not null)
            {
                StandardUrls[BuildConfigs[i]] = new VersionUrl(ghAsset.BrowserDownloadUrl, ghAsset.Size);
            }

            t = from asset in Release.Assets
                where asset.Name.Contains(DotnetMatch[i])
                select asset;
            
            ghAsset = t.FirstOrDefault();
            if (ghAsset is not null)
            {
                DotnetUrls[BuildConfigs[i]] = new VersionUrl(ghAsset.BrowserDownloadUrl, ghAsset.Size);
            }
        }

        var x = from asset in Release.Assets
            where asset.Name.Contains("SHA512-SUMS.txt")
            select asset;

        var sha = x.FirstOrDefault();
        if (sha is not null)
        {
            Sha512Sum = new VersionUrl(sha.BrowserDownloadUrl, sha.Size);
        }
    }

    public static EngineRelease FromRelease(Release release, string repo)
    {
        var er = new EngineRelease();
        er.Repo = repo;
        er.ReleaseId = release.Id;
        er.Release = release;
        er.Version = SemanticVersion.Parse(release.TagName);
        er.GatherUrls();
        return er;
    }
    #endregion
}