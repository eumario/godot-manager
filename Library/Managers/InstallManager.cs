#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using GodotManager.Library.Models;

namespace GodotManager.Library.Managers;

public partial class InstallManager : Node
{
    #region Singleton
    private static InstallManager? _instance;
    
    public static InstallManager Instance
    {
        get
        {
            _instance ??= new InstallManager();
            if (!_instance.IsInsideTree())
            {
                ((SceneTree)Engine.GetMainLoop()).Root.AddChild(_instance);
            }
            return _instance;
        }
    }
    #endregion
    
    #region Signals

    [Signal]
    public delegate void QueueTagInstallEventHandler(InstallPack pack);

    [Signal]
    public delegate void StartTagInstallEventHandler(string tag);

    [Signal]
    public delegate void InstallProgressChangedEventHandler(string tag, double progress);

    [Signal]
    public delegate void InstallCompletedEventHandler(string tag, int step);

    [Signal]
    public delegate void InstallTagCompletedEventHandler(string tag);
    #endregion
    
    #region Private Variables
    private readonly Queue<InstallPack> _packs = [];
    #endregion
    
    #region Public Variables

    public int QueueSize => _packs.Count;
    public InstallPack? CurrentPack => _packs.Peek();
    public bool QueueEmpty => _packs.Count == 0;
    #endregion
    
    #region Godot Overrides
    public override partial void _Process(double delta);

    [GodotOverride]
    public void OnProcess(double delta)
    {
        if (QueueEmpty) return;
        if (!CurrentPack.IsReady) return;
        BeginInstall();
    }
    #endregion

    #region Public API

    public void QueueInstall(string tag, string path, string dest)
    {
        var tags = _packs.ToList();
        InstallPack? pack = tags.FirstOrDefault(x => x.Tag == tag);
        if (pack == null)
        {
            pack = new InstallPack();
            pack.IsReady = false;
            pack.Tag = tag;
            EmitSignal(SignalName.QueueTagInstall, pack);
            _packs.Enqueue(pack);
        }

        pack.Sources.Add(path);
        pack.Dests.Add(dest);

    }
    #endregion

    #region Private Functions
    private void BeginInstall()
    {
        for (var step = 0; step < CurrentPack.TotalSteps; step++)
        {
            var src = CurrentPack.Sources[step];
            var dest = CurrentPack.Dests[step];
            if (src == "")
            {
                DirAccess.MakeDirRecursiveAbsolute(dest.GetBaseDir());
                File.WriteAllText(dest, "");
            }
            else if (src.EndsWith(".zip"))
            {
                InstallEditor(src, dest);
            }
            else if (src.EndsWith(".tpz"))
            {
                InstallTemplates(src, dest);
            }
            else
            {
                GD.Print($"Unknown file provided! {src} -> {dest}");
            }
        }

        CurrentPack.IsReady = false;
    }

    private void InstallEditor(string src, string dest)
    {
        ZipReader zr = new ZipReader();
        if (zr.Open(src) != Error.Ok)
        {
            GD.Print($"Failed to open source zip file: {src}");
            return;
        }

        var files = 0;
        var ignoreDir = "";
        foreach (var file in zr.GetFiles())
        {
            if (file.EndsWith("/") && files == 0)
            {
                // Root for C# Dotnet/Mono build of Godot Editor
                ignoreDir = file;
            }

            if (file == ignoreDir) continue;
            if (file.EndsWith("/"))
            {
                DirAccess.MakeDirRecursiveAbsolute(dest.PathJoin(file.Replace(ignoreDir,"")));
            }
            else
            {
                if (!DirAccess.DirExistsAbsolute(dest))
                    DirAccess.MakeDirRecursiveAbsolute(dest);
                var data = zr.ReadFile(file);
                File.WriteAllBytes(ignoreDir != "" ? dest.PathJoin(file.Replace(ignoreDir, "")) : dest.PathJoin(file),
                    data);
            }

            files++;
        }
    }

    private void InstallTemplates(string src, string dest)
    {
        ZipReader zr = new ZipReader();
        if (zr.Open(src) != Error.Ok)
        {
            GD.Print($"Failed to open source zip file: {src}");
            return;
        }

        var version = CurrentPack.Tag.Split("-").Join(".");
        if (src.Contains("_mono_export"))
            version += ".mono";
        
        foreach (var file in zr.GetFiles())
        {
            var destFile = dest.PathJoin(file.Replace("template", version));
            if (!DirAccess.DirExistsAbsolute(destFile.GetBaseDir()))
                DirAccess.MakeDirRecursiveAbsolute(destFile.GetBaseDir());
            var data = zr.ReadFile(file);
            File.WriteAllBytes(destFile, data);
        }
    }
    #endregion
}