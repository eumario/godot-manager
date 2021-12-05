using Godot;
using Godot.Collections;

[Tool]
public class ActionButtons : PanelContainer
{
    [Signal]
    delegate void Clicked(int index);

    [Export(PropertyHint.File)]
    Array<StreamTexture> Icons = null;

    Array<ColorRect> _icons = new Array<ColorRect>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Icons == null) {
            Icons = new Array<StreamTexture>();
            return;
        }
        for (int i = 0; i < Icons.Count; i++) {
            ColorRect icon_bg = new ColorRect();
            icon_bg.RectMinSize = new Vector2(20,20);
            icon_bg.Color = new Color("ACACAC");
            icon_bg.SelfModulate = new Color("00FFFFFF");
            TextureRect icon = new TextureRect();
            icon.Texture = Icons[i];
            icon.RectMinSize = new Vector2(20,20);
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.Expand = true;
            icon_bg.AddChild(icon);
            _icons.Add(icon_bg);
            
            Godot.Collections.Array bg = new Godot.Collections.Array();
            bg.Add(icon_bg);

            GetNode<HBoxContainer>("Buttons").AddChild(icon_bg);
            icon_bg.Connect("mouse_entered", this, "Icon_MouseEntered", bg);
            icon_bg.Connect("mouse_exited", this, "Icon_MouseExited", bg);

            bg.Add(i);
            icon_bg.Connect("gui_input", this, "Icon_GuiInput", bg);
        }
    }

    public void Icon_MouseEntered(ColorRect rect) {
        rect.SelfModulate = new Color("B9FFFFFF");
    }

    public void Icon_MouseExited(ColorRect rect) {
        rect.SelfModulate = new Color("00FFFFFF");
    }

    public void Icon_GuiInput(InputEvent inputEvent, ColorRect bg, int index) {
        if (inputEvent is InputEventMouseButton iemb && iemb.Pressed && (ButtonList)iemb.ButtonIndex == ButtonList.Left)
        {
            EmitSignal("Clicked", index);
        }
    }
}
