using System;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Utility;
using GodotManager.Scenes;

// namespace

namespace GodotManager.Library.Components.Controls;

[Tool]
public partial class BrowseLine : Control
{
	#region Signals
	#endregion
	
	#region Quick Create
	public static BrowseLine FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Controls/BrowseLine.tscn");
		return scene.Instantiate<BrowseLine>();
	}
	#endregion
	
	#region Node Paths

	[NodePath] private Label _textLabel;
	[NodePath] private LineEdit _input;
	[NodePath] private Button _browse;
	[NodePath] private Button _default;
	[NodePath] private Control _spacer1;
	[NodePath] private Control _spacer2;
	#endregion
	
	#region Private Variables

	private string _displayText;
	private string _defaultValue;
	private int _textWidth;
	private bool _useDefault;
	private bool _useLabel;
	#endregion
	
	#region Public Variables

	[Export]
	public string DisplayText
	{
		get => _displayText;
		set
		{
			_displayText = value;
			if (_textLabel == null) return;
			_textLabel.Text = value;
		}
	}

	[Export]
	public string DefaultValue
	{
		get => _defaultValue;
		set
		{
			_defaultValue = value;
			if (_input != null)
				_input.Text = value;
			if (_default == null) return;
			_default.Visible = !string.IsNullOrEmpty(value);
		}
	}

	public string Text
	{
		get => _input.Text;
		set
		{
			_input.Text = value;
		}
	}

	[Export]
	public int LabelWidth
	{
		get => _textWidth;
		set
		{
			_textWidth = value;
			if (_textLabel == null) return;
			_textLabel.CustomMinimumSize = new Vector2I(value, 25);
		}
	}

	[Export]
	public bool UseDefault
	{
		get => _useDefault;
		set
		{
			_useDefault = value;
			if (_default == null) return;
			_default.Visible = value;
		}
	}

	[Export]
	public bool UseLabel
	{
		get => _useLabel;
		set
		{
			_useLabel = value;
			if (_textLabel == null) return;
			_textLabel.Visible = value;
			_spacer1.Visible = value;
			_spacer2.Visible = value;
		}
	}

	public string Result => _input.Text;
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		// Rest of Initialization Functions
		DisplayText = _displayText;
		DefaultValue = _defaultValue;
		LabelWidth = _textWidth;
		UseDefault = _useDefault;
		UseLabel = _useLabel;

		_browse.Pressed += async () =>
		{
			MainWindow.BrowseFolderDialog("Locate Project Root Folder", UseDefault ? DefaultValue : "", "", true,
				DisplayServer.FileDialogMode.OpenDir, new string[] {}, Callable.From<bool, string[], int>(HandleBrowseDialog));
		};
		
		_default.Pressed += () => _input.Text = DefaultValue;
	}
	#endregion
	
	#region Event Handlers

	private void HandleBrowseDialog(bool status, string[] selectedPaths, int filterIndex)
	{
		if (selectedPaths.Length >= 1)
			_input.Text = selectedPaths[0];
	}
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	public string GetFolder() => _input.Text;
	#endregion
}