using Godot;
using System;

namespace GodotManager.Library.UI;

[SceneTree(root: "Nodes")]
public partial class Header : PanelContainer
{
    public override partial void _Ready();
    [GodotOverride]
    public void OnReady()
    {
        Settings.Pressed += ShowSettings;
    }

    void ShowSettings()
    {
        GD.Print("Show Settings");
    }
}
