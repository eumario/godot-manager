using Godot;
using Godot.Sharp.Extras;

[Tool]
public class SysButton : ColorRect
{
    enum TYPES { close, maximize, minimize }

    [Export(PropertyHint.Enum)]
    TYPES ButtonType = TYPES.close;

    [Export]
    public NodePath WindowMain = null;

    [ResolveNode(nameof(WindowMain))]
    Control WindowHandle = null;

    private StreamTexture _icon = ResourceLoader.Load<StreamTexture>("res://Assets/Icons/x.svg");

    [Export]
    StreamTexture Icon {
        get {
            return _icon;
        }
        set {
            _icon = value;
            if (GetNode<TextureRect>("cc/icon") != null) {
                GetNode<TextureRect>("cc/icon").Texture = _icon;
            }
        }
    }

    private Color BaseColor;

    public override void _Ready()
    {
        this.OnReady();

        BaseColor = Color;
        GetNode<TextureRect>("cc/icon").Texture = Icon;
        this.OnReady();
    }

    [SignalHandler("gui_input")]
    void OnSysButton_GuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed && (ButtonList)iemb.ButtonIndex != ButtonList.Left)
            return;
        
        switch(ButtonType) {
            case TYPES.close:
                if (WindowHandle is MainWindow) {
                    GetTree().Quit();
                } else {
                    WindowHandle.Visible = false;
                }
                break;
            case TYPES.minimize:
                OS.WindowMinimized = true;
                break;
            default:
                OS.WindowMaximized = !OS.WindowMaximized;
                break;
        }
    }

    [SignalHandler("mouse_entered")]
    void OnSysButton_MouseEntered() {
        if (ButtonType == TYPES.close) {
            Color = new Godot.Color("e11f1f");
        } else {
            Color = new Godot.Color("383d4a");
        }
    }

    [SignalHandler("mouse_exited")]
    void OnSysButton_MouseExited() {
        Color = BaseColor;
    }
}