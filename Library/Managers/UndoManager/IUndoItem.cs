namespace GodotManager.Library.Managers.UndoManager;

public interface IUndoItem
{
    void Apply();
    void Undo();
}