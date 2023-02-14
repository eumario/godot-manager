using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Controls;

[Tool]
public partial class ItemListWithButtons : PanelContainer
{
	#region Signals

	[Signal]
	public delegate void ItemAddEventHandler();

	[Signal]
	public delegate void ItemEditEventHandler();

	[Signal]
	public delegate void ItemRemoveEventHandler();

	[Signal]
	public delegate void ItemSelectedEventHandler(int i);
	#endregion
	
	#region Node Paths

	[NodePath] private VBoxContainer _horizontalButtons;
	[NodePath] private HBoxContainer _verticalButtons;
	
	[NodePath] private Button _addItemVertical;
	[NodePath] private Button _editItemVertical;
	[NodePath] private Button _removeItemVertical;

	[NodePath] private Button _addItemHorizontal;
	[NodePath] private Button _editItemHorizontal;
	[NodePath] private Button _removeItemHorizontal;

	[NodePath] private ItemList _itemList;
	#endregion
	
	#region Private Variables
	private bool _disabled = false;
	private bool _vertical = true;
	private bool _flatButtons = false;
	#endregion
	
	#region Public Variables

	[Export]
	public bool Disabled
	{
		get => _disabled;
		set
		{
			_disabled = value;
			if (_itemList != null)
			{
				for (var i = 0; i < _itemList.ItemCount; i++)
				{
					_itemList.SetItemDisabled(i, value);
				}
				GetTree().SetGroup("ILWB_buttons", "disabled", value);
			}
		}
	}

	[Export]
	public bool Vertical
	{
		get => _vertical;
		set
		{
			_vertical = value;
			if (_itemList != null)
			{
				_verticalButtons.Visible = value;
				_horizontalButtons.Visible = !value;
			}
		}
	}

	[Export]
	public bool FlatButtons
	{
		get => _flatButtons;
		set
		{
			_flatButtons = value;
			if (_verticalButtons != null)
			{
				foreach (var button in GetTree().GetNodesInGroup("ILWB_buttons"))
				{
					((Button)button).Flat = value;
				}
			}
		}
	}

	public int ItemCount => _itemList.ItemCount;
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		Disabled = _disabled;
		Vertical = _vertical;
		FlatButtons = _flatButtons;
		_addItemHorizontal.Pressed += () => EmitSignal(nameof(ItemAdd));
		_addItemVertical.Pressed += () => EmitSignal(nameof(ItemAdd));
		
		_editItemHorizontal.Pressed += () => EmitSignal(nameof(ItemEdit));
		_editItemVertical.Pressed += () => EmitSignal(nameof(ItemEdit));
		
		_removeItemHorizontal.Pressed += () => EmitSignal(nameof(ItemRemove));
		_removeItemVertical.Pressed += () => EmitSignal(nameof(ItemRemove));
		
		_itemList.ItemSelected += (i) => EmitSignal(nameof(ItemSelected), i);
		// Rest of Initialization Functions
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions

	public void AddItem(string text) => _itemList.AddItem(text);
	public void SetItemText(int index, string text) => _itemList.SetItemText(index, text);
	public void SetItemMetadata(int index, Variant data) => _itemList.SetItemMetadata(index, data);
	public int[] GetSelectedItems() => _itemList.GetSelectedItems();
	public Variant GetItemMetadata(int index) => _itemList.GetItemMetadata(index);
	public int GetItemCount() => _itemList.ItemCount;
	public string GetItemText(int index) => _itemList.GetItemText(index);
	public void RemoveItem(int index) => _itemList.RemoveItem(index);
	public void MoveItem(int index, int to) => _itemList.MoveItem(index, to);
	public void Clear() => _itemList.Clear();

	public int GetSelected()
	{
		int[] values = GetSelectedItems();
		if (values.Length == 0)
			return -1;
		return values[0];
	}
	#endregion
}