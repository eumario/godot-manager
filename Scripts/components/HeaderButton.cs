using Godot;
using Godot.Sharp.Extras;
using System;

[Tool]
public class HeaderButton : PanelContainer
{
    public enum SortDirection {
        Indeterminate,
        Up,
        Down
    }

    [Signal]
    public delegate void direction_changed(SortDirection button);

    [Export]
    public string Title {
        get {
            if (_headerTitle != null)
                return _headerTitle.Text;
            else
                return _title;
        }

        set {
            _title = value;
            if (_headerTitle != null)
                _headerTitle.Text = value;
        }
    }

    [Export(PropertyHint.Enum,"Direction to Sort")]
    public SortDirection Direction {
        get {
            if (_dirIcon != null) {
                if (_dirIcon.Texture == arrow)
                    return (_dirIcon.FlipV ? SortDirection.Up : SortDirection.Down);
                else
                    return SortDirection.Indeterminate;
            } else {
                return _direction;
            }
        }

        set {
            _direction = value;
            if (_dirIcon != null) {
                if (value == SortDirection.Indeterminate)
                    _dirIcon.Texture = minus;
                else {
                    _dirIcon.Texture = arrow;
                    _dirIcon.FlipV = (value == SortDirection.Up);
                }
            }
        }
    }

    [NodePath("HC/Label")]
    Label _headerTitle = null;

    [NodePath("HC/DirIcon")]
    TextureRect _dirIcon = null;

    private string _title;
    private SortDirection _direction;

    Texture arrow = GD.Load<Texture>("res://Assets/Icons/drop_down1.svg");
    Texture minus = GD.Load<Texture>("res://Assets/Icons/minus.svg");

    public override void _Ready()
    {
        this.OnReady();
        Title = _title;
        Direction = _direction;
    }

    public void Indeterminate() {
        Direction = SortDirection.Indeterminate;
    }

    [SignalHandler("gui_input")]
    void OnGuiInput_Header(InputEvent @event) {
        if (@event is InputEventMouseButton @iemb) {
            if (@iemb.Doubleclick && @iemb.ButtonIndex == (int)ButtonList.Left) {
                Direction = SortDirection.Indeterminate;
                EmitSignal("direction_changed", Direction);
            } else if (@iemb.Pressed && @iemb.ButtonIndex == (int)ButtonList.Left) {
                Direction = (Direction == SortDirection.Down) ? SortDirection.Up : SortDirection.Down;
                EmitSignal("direction_changed", Direction);
            }
        }
    }
}
