using Godot;
using System;

namespace GodotManager.Library.UI;

[GlobalClass, Tool, SceneTree(root: "Nodes")]
public partial class ProjectLineItem : PanelContainer
{
    private const string StarGlyph = "󰓎";
    private const string LinkGlyph = "󰌷";
    private const string UnlinkGlyph = "󰌸";

    [OnInstantiate]
    public void Initialize(string projectFile)
    {
        
    }
    
    [GodotOverride]
    public void OnReady()
    {
        
    }

    public override partial void _Ready();
}
