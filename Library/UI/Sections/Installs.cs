using Godot;
using System;
using System.Linq;
using GodotManager;
using GodotManager.Library.UI.Dialog;
using GodotManager.Library.Util;

namespace GodotManager.Library.UI.Sections;

[SceneTree(root: "Nodes")]
public partial class Installs : PanelContainer
{
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        InstallEngine.Pressed += () =>
        {
            var dlg = InstallGodotEditor.Instantiate();
            dlg.NewInstallCompleted += () =>
            {
                RefreshInstalls();
            };
            MainWindow.GetInstance()!.AddChild(dlg);
        };
        RefreshInstalls();
    }

    private void RefreshInstalls()
    {
        var context = MainWindow.GetInstance().Context;
        InstallList.QueueFreeAllChildren();
        ItemListHeader? lastHeader = null;
        foreach (var engine in context.EngineVersions.OrderByDescending(e => e.Release.Version))
        {
            if (lastHeader == null || $"Godot {engine.Release.Version.Major}" != lastHeader.Header)
            {
                lastHeader = ItemListHeader.Instantiate($"Godot {engine.Release.Version.Major}");
                InstallList.AddChild(lastHeader);
            }

            var line = EngineLineItem.Instantiate(engine);
            InstallList.AddChild(line);
        }
    }
}
