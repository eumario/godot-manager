using Godot;
using System;
using GodotManager.Library.Models;
using GodotManager.Library.Util;

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
        Tags.QueueFreeAllChildren();
        var tag = Tag.Instantiate("Editor");
        Tags.AddChild(tag);
        if (Engine.DotnetEditor != "")
        {
            tag = Tag.Instantiate(Engine.IsGodot4 ? "Dotnet" : "Mono");
            Tags.AddChild(tag);
        }

        if (Engine.HasTemplate)
        {
            tag = Tag.Instantiate("Standard Templates");
            Tags.AddChild(tag);
        }

        if (Engine.HasDotnetTemplate)
        {
            tag = Tag.Instantiate(Engine.IsGodot4 ? "Dotnet Templates" : "Mono Templates");
            Tags.AddChild(tag);
        }
        
    }
}
