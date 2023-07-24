using System;
using System.IO;
using Godot;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;
using ICSharpCode.SharpZipLib.Zip;

namespace GodotManager.Library.Managers;

public class InstallManager
{
    private static InstallManager _instance;
    public static InstallManager Instance => _instance ??= new InstallManager();
    public delegate void InstallCompletedEventHandler(GodotLineItem item, GodotVersion gve);

    public event InstallCompletedEventHandler InstallCompleted;

    public void InstallVersion(GodotLineItem item, byte[] buffer)
    {
        var url = item.GithubVersion?.GetDownloadUrl(item.IsMono) ?? item.TuxfamilyVersion.GetDownloadUrl(item.IsMono);
        var fname = Path.GetFileName(url);
        var cacheFolder = Database.Settings.CachePath.Join("godot", fname);
        
        if (!Directory.Exists(Database.Settings.CachePath.Join("godot")))
            Directory.CreateDirectory(Database.Settings.CachePath.Join("godot"));
        
        var tag = item.GithubVersion?.GetTagName(item.IsMono) ?? item.TuxfamilyVersion.GetTagName(item.IsMono);
        var installFolder = Database.Settings.EnginePath.Join(tag);
        
        if (!Directory.Exists(Database.Settings.EnginePath))
            Directory.CreateDirectory(Database.Settings.EnginePath);
        
        File.WriteAllBytes(cacheFolder, buffer); 
        
        var zip = new FastZip();
        if (item.IsMono)
        {
            zip.ExtractZip(cacheFolder, Database.Settings.EnginePath, "");
            var extractedPath = Database.Settings.EnginePath.Join(Path.GetFileNameWithoutExtension(cacheFolder));
            Directory.Move(extractedPath, installFolder);
        }
        else
        {
            zip.ExtractZip(cacheFolder, installFolder, "");
            
        }

        var gdVers = new GodotVersion
        {
            Location = installFolder,
            GithubVersion = item.GithubVersion,
            TuxfamilyVersion = item.TuxfamilyVersion,
            CacheLocation = cacheFolder,
            DownloadDate = DateTime.Now,
            Tag = tag,
            Url = url,
            IsMono = item.IsMono,
            HideConsole = Database.Settings.NoConsole
        };

        switch (Platform.Get())
        {
            case PlatformType.Windows:
                gdVers.ExecutableName = Path.GetFileNameWithoutExtension(fname) + (item.IsMono ? ".exe" : "");
                break;
            case PlatformType.MacOS:
                gdVers.ExecutableName = "Godot";
                break;
            case PlatformType.LinuxBSD:
                var za = new ZipFile(cacheFolder);
                foreach (ZipEntry entry in za)
                {
                    if ((entry.Name.EndsWith(".64") || entry.Name.EndsWith("x86_64")) && entry.Name.StartsWith("Godot"))
                    {
                        gdVers.ExecutableName = entry.Name;
                        break;
                    }

                    if (!entry.Name.EndsWith(".32") || !entry.Name.StartsWith("Godot")) continue;
                    gdVers.ExecutableName = entry.Name;
                    break;
                }
                break;
        }

        if (Platform.Get() == PlatformType.LinuxBSD || Platform.Get() == PlatformType.MacOS)
            gdVers.GetExecutablePath().Chmod(0755);

        if (Platform.Get() == PlatformType.MacOS)
            gdVers.GetExecutablePath().XAttr("-cr");

        Util.RunInMainThread(() => InstallCompleted?.Invoke(item, gdVers));
    }
}