using Godot;
using System;

public class Titlebar : Control
{
    private bool following = false;
    private Vector2 start_pos = Vector2.Zero;
    public override void _Ready()
    {
        Connect("gui_input", this, "OnTitlebar_GuiInput");
    }

    void OnTitlebar_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton) {
            var iemb = inputEvent as InputEventMouseButton;
            if (iemb.ButtonIndex == 1) {
                following = !following;
                start_pos = GetLocalMousePosition();
                return;
            }
        }

        if (following) {
            OS.WindowPosition = OS.WindowPosition + GetLocalMousePosition() - start_pos;
        }
    }
}
