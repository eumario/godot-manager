using Godot;
using Godot.Collections;
using System;

[Tool]
public class ViewToggleButtons : PanelContainer
{
    [Signal]
    delegate void Clicked(int index);

    [Export(PropertyHint.File)]
    Array<StreamTexture> Icons = null;

    Array<ColorRect> _icons;

    int toggleIndx = 0;

    public override void _Ready()
    {
        _icons = new Array<ColorRect>();

        for (var i = 0; i < Icons.Count; i++) {
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
            
            if (i == toggleIndx) {
                icon.SelfModulate = new Color("7defa7");
            }
            
            if (i+1 < Icons.Count) {
                VSeparator sep = new VSeparator();
                GetNode<HBoxContainer>("Buttons").AddChild(sep);
            }
        }
    }

    public void Icon_MouseEntered(ColorRect rect) {
        rect.SelfModulate = new Color("B9FFFFFF");
    }

    public void Icon_MouseExited(ColorRect rect) {
        rect.SelfModulate = new Color("00FFFFFF");
    }

    public void Icon_GuiInput(InputEvent inputEvent, ColorRect bg, int index) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed && (ButtonList)iemb.ButtonIndex != ButtonList.Left)
            return;
        
        _icons[toggleIndx].GetChild<TextureRect>(0).SelfModulate = new Color("FFFFFF");
        toggleIndx = index;
        bg.GetChild<TextureRect>(0).SelfModulate = new Color("7defa7");
        EmitSignal("Clicked", index);
    }
}
