using System;

namespace GodotManager.Library.Managers.UndoManager;

public class UndoItem<T> : IUndoItem
{
    private readonly T _oldValue;
    private readonly T _newValue;
    private readonly Action<T> _apply;
    private readonly Action<T> _undo;
    
    public UndoItem(T oldValue, T newValue, Action<T> apply, Action<T> undo)
    {
        _oldValue = oldValue;
        _newValue = newValue;
        _apply = apply;
        _undo = undo;
    }

    public void Apply() => _apply.Invoke(_newValue);
    public void Undo() => _undo.Invoke(_oldValue);
}