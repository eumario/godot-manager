using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;

public class ItemListWithButtons : HBoxContainer
{
    #region Signals
    [Signal] public delegate void add_requested();
    
    [Signal] public delegate void edit_requested();

    [Signal] public delegate void remove_requested();
    #endregion
    
    #region Nodc Paths
    [NodePath("VBMOD/AddItem")] Button _addItem = null;
    [NodePath("VBMOD/EditItem")] Button _editItem = null;
    [NodePath("VBMOD/RemoveItem")] Button _removeItem = null;
    [NodePath("SCML/PC/ItemList")] ItemList _list = null;
    #endregion

    #region Private Variables
    private bool _disabled = false;
    #endregion

    #region Public Properties
    [Export]
    public bool Disabled {
        get => _disabled;
        set {
            _disabled = value;
            if (_addItem != null)
                _addItem.Disabled = value;
            if (_editItem != null)
                _editItem.Disabled = value;
            if (_removeItem != null)
                _removeItem.Disabled = value;
            if (_list != null) {
                for (int i = 0; i < _list.GetItemCount(); i++) {
                    _list.SetItemDisabled(i, value);
                }
            }
        }
    }
    #endregion

    #region Handlers
    public override void _Ready()
    {
        this.OnReady();
        Disabled = _disabled;
    }
    #endregion

    #region Private Functions
    #endregion

    #region Public Functions
    public void AddItem(string text) => _list.AddItem(text);
    public void SetItemText(int index, string text) => _list.SetItemText(index, text);
    public void SetItemMetadata(int idx, object data) => _list.SetItemMetadata(idx, data);
    public int[] GetSelectedItems() => _list.GetSelectedItems();
    public object GetItemMetadata(int idx) => _list.GetItemMetadata(idx);
    public int GetItemCount() => _list.GetItemCount();
    public string GetItemText(int idx) => _list.GetItemText(idx);
    public void RemoveItem(int idx) => _list.RemoveItem(idx);
    public void MoveItem(int idx, int to) => _list.MoveItem(idx, to);
    public void Clear() => _list.Clear();
    public int GetSelected() {
        int[] values = GetSelectedItems();
        if (values.Length == 0)
            return -1;
        return values[0];
    }
    #endregion

    #region Events
    [SignalHandler("pressed", nameof(_addItem))]
    void OnAddItemPressed() => EmitSignal("add_requested");
    [SignalHandler("pressed", nameof(_editItem))]
    void OnEditItemPressed() => EmitSignal("edit_requested");
    [SignalHandler("pressed", nameof(_removeItem))]
    void OnRemoveItemPressed() => EmitSignal("remove_requested");
    #endregion

}
