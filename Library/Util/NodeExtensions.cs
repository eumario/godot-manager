using Godot;

namespace GodotManager.Library.Util;

public static class NodeExtensions
{
    public static void QueueFreeAllChildren(this Node node)
    {
        foreach (var child in node.GetChildren())
            child.QueueFree();
    }
}