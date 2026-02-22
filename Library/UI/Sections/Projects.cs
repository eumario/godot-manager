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
        CreateProject.Pressed += () =>
        {
            var dlg = NewProject.Instantiate();
            MainWindow.GetInstance()!.AddChild(dlg);
        };
    }

    public void Clear()
    {
        ProjectList.Clear();
    }
}
