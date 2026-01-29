using Godot;

namespace GodotManager.Library.Util;

public static class NodeExtensions
{
    public static void QueueFreeAllChildren(this Node node)
    {
        foreach (var child in node.GetChildren())
            child.QueueFree();
    }

    public static void EmitSignalDeferred(this Node node, StringName signal, params Variant[] args)
    {
        Callable.From(() => node.EmitSignal(signal, args)).CallDeferred();
    }
}