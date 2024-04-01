using Godot;
using Godot.Sharp.Extras;

public class Titlebar : Control
{
    private bool moving = false;
    private bool following = false;
    private Vector2 start_pos = Vector2.Zero;
    public override void _Ready()
    {
        this.OnReady();
    }

    [SignalHandler("gui_input")]
    void OnTitlebar_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb) {
            if (iemb.ButtonIndex == 1) {
                following = !following;
                start_pos = GetLocalMousePosition();
                return;
            }
        }

        if (following && !moving) {
            var movement = GetLocalMousePosition() - start_pos;
            if (movement == Vector2.Zero) return;
            moving = true;
            OS.WindowPosition += movement;
            moving = false;
        }
    }
}
