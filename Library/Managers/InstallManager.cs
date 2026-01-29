#nullable enable
using System.Collections.Generic;
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
        
    }
    #endregion
}