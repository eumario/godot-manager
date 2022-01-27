using Godot;
using GodotSharpExtras;

public class CategoryButton : Button
{

#region Nodes
    [NodePath("HC/Icon")]
    TextureRect _Icon = null;

    [NodePath("HC/Text")]
    Label _Text = null;
#endregion

#region Variables
    private Texture tIcon;
    private string sText;
#endregion

#region Public Accessors / Exports
    [Export]
    public new string Text {
        get {
            if (_Text != null)
                return _Text.Text;
            else
                return sText;
        }

        set {
            sText = value;
            if (_Text != null)
                _Text.Text = value;
        }
    }

    [Export(PropertyHint.File)]
    public new Texture Icon {
        get {
            if (_Icon != null)
                return _Icon.Texture;
            else
                return tIcon;
        }
        set {
            tIcon = value;
            if (_Icon != null)
                _Icon.Texture = value;
        }
    }
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        Icon = tIcon;
        Text = sText;
    }
}
