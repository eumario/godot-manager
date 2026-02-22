using System.Linq;
using Godot;
using GodotManager;
using GodotManager.Library.Util;
using GodotManager.Library.Database;

[SceneTree(root: "Nodes")]
public partial class NewProject : PanelContainer
{
    private AppContext _appContext;
    public override partial void _Ready();

    [GodotOverride]
    private void OnReady()
    {
        _appContext = MainWindow.GetInstance()!.Context;
        EngineVersion.Clear();
        foreach (var version in _appContext.EngineVersions.OrderByDescending(e => e.Release.Version))
            EngineVersion.AddItem(version.VersionTag, version.Id);

        CloseButton.Pressed += QueueFree;
    }
    

    [OnInstantiate]
    private void OnInitialize()
    {
        
    }
}
