using Godot;
using System;

[SceneTree(root: "Nodes")]
public partial class Tag : PanelContainer
{
    [OnInstantiate]
    public void Initialize(string name)
    {
        TagName.Text = name;
    }
}
