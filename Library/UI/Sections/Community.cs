using Godot;
using System;
using Godot.Collections;

[GlobalClass, SceneTree(root: "Nodes")]
public partial class Community : PanelContainer
{
    [Notify, Export] public partial Array<ScrollContainer> TabWindows { get; set; }
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        NavBar.TabChanged += lindx =>
        {
            var indx = (int)lindx;
            if (TabWindows[indx] != null)
            {
                HideSections();
                TabWindows[indx].Show();
            }
        };
        HideSections();
        TabWindows[0].Show();
    }

    private void HideSections()
    {
        foreach (var node in TabWindows)
            node?.Hide();
    }

    public void ClearNews()
    {
        foreach (var node in NewsItemList.GetChildren())
            node.QueueFree();
    }
}
