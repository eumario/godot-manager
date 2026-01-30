using Godot;
using System;
using GodotManager.Library.Models;

[SceneTree(root: "Nodes")]
public partial class EngineLineItem : PanelContainer
{
    private EngineVersion? Engine;
    [OnInstantiate]
    public void Initialize(EngineVersion version)
    {
        Engine = version;
    }

    public override partial void _Ready();

    [GodotOverride]
    public void OnReady()
    {
        if (Engine == null) return;
        var vers = Engine.Release.Version;
        MajMinVersion.Text = $"Godot {vers.Major}.{vers.Minor}";
        FullVersion.Text = $"({vers.Major}.{vers.Minor}.{vers.Build}-{vers.SpecialVersion})";
        Installpath.Text = Engine.StandardInstallPath;
    }
}
