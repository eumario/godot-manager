using Godot;
using System;

namespace GodotManager.Library.UI.Sections;

[GlobalClass, Tool, SceneTree(root: "Nodes")]
public partial class Projects : PanelContainer
{
    public override partial void _Ready();
    [GodotOverride]
    public void OnReady()
    {
    }
}
