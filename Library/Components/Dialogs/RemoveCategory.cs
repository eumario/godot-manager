using System.Collections.Generic;
using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Dialogs;

[SceneNode("res://Library/Components/Dialogs/RemoveCategory.tscn")]
public partial class RemoveCategory : AcceptDialog
{
	#region Signals

	[Signal]
	public delegate void CategorySelectedEventHandler(string category);
	#endregion
	
	#region Node Paths

	[NodePath] private ItemList _categoryList;
	#endregion
	
	#region Private Variables

	private List<string> _categories;
	#endregion
	
	#region Public Variables

	public List<string> Categories
	{
		get => _categories;
		set
		{
			_categories = value;
			if (_categoryList != null)
			{
				_categoryList.Clear();
				foreach (var val in _categories)
					_categoryList.AddItem(val);
			}
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		DialogHideOnOk = false;
		AddCancelButton("Cancel");
		Categories = _categories;
		Confirmed += () =>
		{
			var indx = _categoryList.GetSelectedItems();
			if (indx.Length == 0)
				return;
			EmitSignal(SignalName.CategorySelected, _categoryList.GetItemText(indx[0]));
			Hide();
			QueueFree();
		};
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}