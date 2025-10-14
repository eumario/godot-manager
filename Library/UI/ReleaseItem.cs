using Godot;
using System;
using GodotManager.Library.Models;

[SceneTree(root: "Nodes"), Tool, GlobalClass]
public partial class ReleaseItem : MarginContainer
{
    [Notify] public partial EngineRelease Release { get; set; }
    [OnInstantiate]
    public void Initialize(EngineRelease release)
    {
        Release = release;
    }
    
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        ReleaseChanged += () =>
        {
            EngineVersion.Text = "Godot " + Release.Version.ToString();
            Recommended.Visible = false; // Need to check if release is latest?
        };
        EngineVersion.Text = "Godot " + Release.Version.ToString();
        Install.Pressed += HandleInstall;
    }

    private void HandleInstall()
    {
        
    }
}
