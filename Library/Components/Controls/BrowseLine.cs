using System;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Utility;
using NativeFileDialogSharp;

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
			// var res = await UI.BrowseFolder("Locate Project Root Folder", UseDefault ? DefaultValue : "");
			// if (!string.IsNullOrEmpty(res))
			// 	_input.Text = res.Replace("/","\\");
			var res = Dialog.FolderPicker(UseDefault ? DefaultValue : null);
			if (res.IsCancelled) return;
			_input.Text = OperatingSystem.IsWindows() ? res.Path.Replace("/", "\\") : res.Path.Replace("\\", "/");
		};
		
		_default.Pressed += () => _input.Text = DefaultValue;
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	public string GetFolder() => _input.Text;
	#endregion
}