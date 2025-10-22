using Godot;
using System;
using GodotManager;

[SceneTree(root: "Nodes")]
public partial class Downloads : PanelContainer
{
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        CloseButton.Pressed += () =>
        {
            CloseButton.ReleaseFocus();
            MainWindow.GetInstance()!.HideDownloads();
        };
    }
}
