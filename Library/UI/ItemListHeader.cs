using Godot;
using System;

[SceneTree(root: "Nodes")]
public partial class ItemListHeader : Label
{
    public string Header
    {
        get => Text;
        set => Text = value;
    }

    [OnInstantiate]
    public void Initialize(string header)
    {
        Header = header;
    }
}
