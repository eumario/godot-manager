using Godot;
using Godot.Sharp.Extras;
using System;

public class ToggleButton : ColorRect
{
	[NodePath("../../../../../AnimationPlayer")]
	private AnimationPlayer anim_player = null;
	public override void _Ready()
	{
		this.OnReady();
	}

	[SignalHandler("gui_input")]
	void OnToggleButton_GuiInput(InputEvent inputEvent) {
		if (!(inputEvent is InputEventMouseButton))
            return;
        
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed)
            return;
		
		if ((ButtonList)iemb.ButtonIndex != ButtonList.Left)
			return;

		if (GetNode<ColorRect>("../..").RectMinSize.x == 70)	{
			anim_player.Play("sidebar_anim");
		} else {
			anim_player.PlayBackwards("sidebar_anim");
		}
	}
}
