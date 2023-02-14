using System.Collections.Generic;
using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Dialogs;

[Tool]
public partial class ListSelectDialog : AcceptDialog
{
	#region Signals

	[Signal]
	public delegate void OptionSelectedEventHandler(int option);
	#endregion
	
	#region Quick Create
	public static ListSelectDialog FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/ListSelectDialog.tscn");
		return scene.Instantiate<ListSelectDialog>();
	}
	#endregion
	
	#region Node Paths

	[NodePath] private Label _message;
	[NodePath] private OptionButton _optionList;
	#endregion
	
	#region Private Variables

	private string _msgText;
	private List<string> _options;
	#endregion
	
	#region Public Variables

	public string Message
	{
		get => _msgText;
		set
		{
			_msgText = value;
			if (_message != null)
				_message.Text = value;
		}
	}

	public List<string> Options
	{
		get => _options;
		set
		{
			_options = value;
			if (_optionList != null)
			{
				_optionList.Clear();
				foreach (var item in value)
				{
					_optionList.AddItem(item);
				}
			}
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		Message = _msgText;
		Options = _options;
		AddCancelButton("Cancel");
		Confirmed += () =>
		{
			EmitSignal(nameof(OptionSelected), _optionList.Selected);
		};
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions

	public void AddItem(string option)
	{
		_options ??= new List<string>();
		_options.Add(option);
	}

	public string GetItemText(int indx)
	{
		if (indx < 0 || indx > _options.Count)
			return "";
		return _options[indx];
	}

	public void ClearItems()
	{
		_options = new List<string>();
		_optionList.Clear();
	}
	#endregion
}