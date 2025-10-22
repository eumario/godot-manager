using Godot;
using System;
using GodotManager;

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
            MainWindow.GetInstance()!.AddChild(dlg);
        };
    }
}
