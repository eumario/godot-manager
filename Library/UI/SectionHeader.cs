using Godot;
using System;

[SceneTree(root: "Nodes"), Tool, GlobalClass]
public partial class SectionHeader : Control
{
    [Notify, Export] public partial string Header { get; set; }

    [OnInstantiate]
    public void Initialize(string text)
    {
        Header = text;
    }
    
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        HeaderChanged += () =>
        {
            HeaderText.Text = Header;
        };
        HeaderText.Text = Header;
    }
}
