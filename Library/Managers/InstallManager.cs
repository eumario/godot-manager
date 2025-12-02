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

    private List<InstallPack> _packs = [];
    
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
    
    #region Public API

    public void QueueInstall(string tag, string path, string dest)
    {
        var pack = _packs.FirstOrDefault(x => x.Tag == tag);
        if (pack == null)
        {
            pack = new InstallPack();
            pack.Tag = tag;
            _packs.Add(pack);
        }

        pack.Sources.Add(path);
        pack.Dests.Add(dest);
    }

    public void BeginInstall(string tag)
    {
        
    }
    #endregion
}