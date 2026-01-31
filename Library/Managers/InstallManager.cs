#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Models;
using GodotManager.Library.Util;

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
    private int _counter = 0;
    private int _total = 0;
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
        if (CurrentPack.IsInstalling) return;
        CurrentPack.IsInstalling = true;
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
    private async void BeginInstall()
    {
        this.EmitSignalDeferred(SignalName.StartTagInstall, CurrentPack.Tag);
        var files = ScanZips();
        if (files == -1)
        {
            GD.PushError("Zip Corruption, returning.");
            return;
        }

        _total = files + CurrentPack.TotalSteps;
        _counter = 0;
        this.EmitSignalDeferred(SignalName.InstallProgressChanged, CurrentPack.Tag, _counter);
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
                await InstallEditor(src, dest);
            }
            else if (src.EndsWith(".tpz"))
            {
                await InstallTemplates(src, dest);
            }
            else
            {
                GD.PushError($"Unknown file provided! {src} -> {dest}");
            }

            _counter++;
            this.EmitSignalDeferred(SignalName.InstallProgressChanged, CurrentPack.Tag, (double)_counter / _total * 100.0d);
            this.EmitSignalDeferred(SignalName.InstallCompleted, CurrentPack.Tag, step);
        }
        
        this.EmitSignalDeferred(SignalName.InstallTagCompleted, CurrentPack.Tag);
        _packs.Dequeue();
    }

    private int ScanZips()
    {
        var i = 0;
        var zr = new ZipReader();

        for (var step = 0; step < CurrentPack.TotalSteps; step++)
        {
            if (CurrentPack.Sources[step] == "") continue;
            if (zr.Open(CurrentPack.Sources[step]) != Error.Ok)
            {
                GD.PushError($"Failed to open {CurrentPack.Sources[step]}, possible corruption!");
                return -1;
            }

            var fileCount = zr.GetFiles().Length;
            i += fileCount;
            zr.Close();
            zr = new ZipReader();
        }

        return i;
    }

    private async Task InstallEditor(string src, string dest)
    {
        ZipReader zr = new ZipReader();
        if (zr.Open(src) != Error.Ok)
        {
            GD.PushError($"Failed to open source zip file: {src}");
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
                await File.WriteAllBytesAsync(ignoreDir != "" ? dest.PathJoin(file.Replace(ignoreDir, "")) : dest.PathJoin(file),
                    data);
            }

            files++;
            _counter++;
            this.EmitSignalDeferred(SignalName.InstallProgressChanged, CurrentPack.Tag, (double)_counter / _total * 100.0d);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    private async Task InstallTemplates(string src, string dest)
    {
        ZipReader zr = new ZipReader();
        if (zr.Open(src) != Error.Ok)
        {
            GD.PushError($"Failed to open source zip file: {src}");
            return;
        }

        var version = CurrentPack.Tag.Split("-").Join(".");
        if (src.Contains("_mono_export"))
            version += ".mono";
        
        foreach (var file in zr.GetFiles())
        {
            var destFile = dest.PathJoin(file.Replace("templates", version));
            if (!DirAccess.DirExistsAbsolute(destFile.GetBaseDir()))
                DirAccess.MakeDirRecursiveAbsolute(destFile.GetBaseDir());
            var data = zr.ReadFile(file);
            File.WriteAllBytes(destFile, data);
            
            _counter++;
            this.EmitSignalDeferred(SignalName.InstallProgressChanged, CurrentPack.Tag, (double)_counter / _total * 100.0d);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
    #endregion
}