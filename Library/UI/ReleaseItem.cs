using Godot;
using System;
using GodotManager.Library.Models;

[SceneTree(root: "Nodes"), Tool, GlobalClass]
public partial class ReleaseItem : MarginContainer
{
    [Notify] public partial EngineRelease Release { get; set; }
    
    [Notify] public partial bool LatestRelease { get; set; }
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
            if (Recommended != null)
                Recommended.Visible = LatestRelease;
        };
        if (Release == null)
            EngineVersion.Text = "Godot 4.5.1";
        else
            EngineVersion.Text = "Godot " + Release.Version.ToString();
        Install.Pressed += HandleInstall;
        if (Recommended != null)
            Recommended.Visible = LatestRelease;
        LatestReleaseChanged += () =>
        {
            if (Recommended != null)
                Recommended.Visible = LatestRelease;
        };
    }

    private void HandleInstall()
    {
        
    }
}
