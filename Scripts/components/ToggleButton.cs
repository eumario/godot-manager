using Godot;
using GodotSharpExtras;
using System;

public class ToggleButton : ColorRect
{
	[NodePath("../../../../../AnimationPlayer")]
	private AnimationPlayer anim_player = null;
	public override void _Ready()
	{
		this.OnReady();

		Connect("gui_input", this, "OnToggleButton_GuiInput");
	}

	// func _on_ColorRect_gui_input(event:InputEvent) -> void:
	// 	pass # Replace with function body.
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
