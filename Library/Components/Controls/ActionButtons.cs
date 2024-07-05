using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;

namespace GodotManager.Library.Components.Controls;

[Tool]
public partial class ActionButtons : PanelContainer
{
	#region Signals
	[Signal] public delegate void ButtonClickedEventHandler(int index);
	
	#endregion

	#region Exports
	
	[Export(PropertyHint.File)] private Array<Texture2D> Icons = null;
	[Export] private Array<string> HelpText = null;
	
	#endregion

	#region Node Paths
	
	[NodePath] private HBoxContainer Buttons = null;
	
	#endregion
	
	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		Icons ??= new Array<Texture2D>();
		HelpText ??= new Array<string>();
		
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
			btn.TooltipText = HelpText[i];
			btn.AddThemeStyleboxOverride("hover", highlight);
			btn.AddThemeStyleboxOverride("focus", empty);
			Buttons.AddChild(btn);
			var index = i;
			btn.Pressed += () => { EmitSignal(SignalName.ButtonClicked, index); };
		}
	}
	#endregion

	#region Public Methods
	public void SetHidden(params int[] indexes)
	{
		foreach (var index in indexes) {
			if (index < 0 || index > Buttons.GetChildCount())
				return;
			Buttons.GetChild<Button>(index).Visible = false;
			Visible = IsAnyVisible();
		}
	}

	public void SetVisible(params int[] indexes)
	{
		foreach (var index in indexes)
		{
			if (index < 0 || index > Buttons.GetChildCount())
				return;
			Buttons.GetChild<Button>(index).Visible = true;
			Visible = IsAnyVisible();
		}
	}

	public bool IsAnyVisible()
	{
		foreach(var node in Buttons.GetChildren())
			if (node is Button { Visible: true })
				return true;
		return false;
	}
	#endregion
}