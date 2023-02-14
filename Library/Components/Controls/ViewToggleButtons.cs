using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;

namespace GodotManager.Library.Components.Controls;

[Tool]
public partial class ViewToggleButtons : PanelContainer
{
	#region Signals
	[Signal] public delegate void ViewChangedEventHandler(int index);
	#endregion

	#region Exports
	[Export(PropertyHint.File)] private Array<Texture2D> Icons = null;
	[Export] private Array<string> HelpText = null;
	#endregion

	#region Node Paths
	[NodePath] private HBoxContainer Buttons = null;
	#endregion

	#region Private Variables
	private ButtonGroup _buttonGroup;
	#endregion
	
	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		Icons ??= new Array<Texture2D>();
		HelpText ??= new Array<string>();
		_buttonGroup = new ButtonGroup();
		
		var highlight = new StyleBoxFlat()
		{
			BgColor = new Color(Colors.WhiteSmoke, 0.4f)
		};
		var empty = new StyleBoxEmpty();

		for (var i = 0; i < Icons.Count; i++)
		{
			var btn = new Button();
			btn.Flat = true;
			btn.CustomMinimumSize = new Vector2(24, 24);
			btn.ExpandIcon = true;
			btn.Icon = Icons[i];
			btn.ToggleMode = true;
			btn.TooltipText = HelpText[i];
			btn.ButtonGroup = _buttonGroup;
			btn.AddThemeStyleboxOverride("hover", highlight);
			btn.AddThemeStyleboxOverride("focus", empty);
			Buttons.AddChild(btn);
			if (i != Icons.Count - 1)
			{
				var sep = new VSeparator();
				sep.CustomMinimumSize = new Vector2(2, 10);
				sep.SizeFlagsVertical = SizeFlags.ShrinkCenter;
				Buttons.AddChild(sep);
			}

			var index = i;
			btn.Pressed += () =>
			{
				foreach (var groupButton in _buttonGroup.GetButtons())
					groupButton.SelfModulate = groupButton.ButtonPressed ? Colors.Green : Colors.White;
				EmitSignal(nameof(ViewChanged), index);
			};
		}

		_buttonGroup.GetButtons()[0].ButtonPressed = true;
		_buttonGroup.GetButtons()[0].SelfModulate = Colors.Green;
	}
	#endregion

	#region Public Functions
	public void SetView(int index)
	{
		if (_buttonGroup is null)
		{
			CallDeferred("SetView", index);
			return;
		}

		var i = 0;
		foreach (var btn in _buttonGroup.GetButtons())
		{
			if (index == i)
			{
				btn.ButtonPressed = true;
				btn.SelfModulate = Colors.Green;
			}
			else
			{
				btn.ButtonPressed = false;
				btn.SelfModulate = Colors.White;
			}
		}
	}
	#endregion
	
}