using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotManager.Library.Managers.UndoManager;

public class HistoryManager
{
    private static Queue<IUndoItem> _queue = new Queue<IUndoItem>();

    public static void Push(IUndoItem item)
    {
        ((SceneTree)Engine.GetMainLoop()).Root.GetNode<SignalBus>("/root/SignalBus")
            .EmitSignal(SignalBus.SignalName.SettingsChanged);
        GD.Print($"Queue Count Before: {Count}");
        _queue.Enqueue(item);
        GD.Print($"Queue Count After: {Count}");
    }

    public static IUndoItem Pop() => _queue.Dequeue();
    public static bool Empty() => _queue.Count == 0;

    public static int Count => _queue.Count;

    public static void Apply()
    {
        while (!Empty())
            Pop().Apply();
    }

    public static void Undo()
    {
        var items = _queue.ToArray();
        var index = Count;
        foreach (var item in items.Reverse())
        {
            item.Undo();
            index--;
        }
        _queue.Clear();
    }
}