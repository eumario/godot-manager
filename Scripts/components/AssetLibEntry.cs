using Godot;
using GodotSharpExtras;
using Godot.Collections;
using System.Threading.Tasks;

public class AssetLibEntry : ColorRect
{
#region Node Paths
    [NodePath("hc/Icon")]
    TextureRect _icon = null;

    [NodePath("hc/vc/Title")]
    Label _title = null;

    [NodePath("hc/vc/hc/Category")]
    Label _category = null;

    [NodePath("hc/vc/hc/License")]
    Label _license = null;

    [NodePath("hc/vc/Author")]
    Label _author = null;
#endregion

#region Private Variables
    Texture tIcon;
    string sTitle;
    string sCategory;
    string sLicense;
    string sAuthor;
#endregion

#region Public Accessors
    public Texture Icon {
        get { return (_icon != null ? _icon.Texture : tIcon); }
        set {
            tIcon = value;
            if (_icon != null)
                _icon.Texture = value;
        }
    }

    public string Title {
        get { return (_title != null ? _title.Text : sTitle); }
        set {
            sTitle = value;
            if (_title != null)
                _title.Text = value;
        }
    }

    public string Category {
        get { return (_category != null ? _category.Text : sCategory); }
        set {
            sCategory = value;
            if (_category != null)
                _category.Text = "Category: " + value;
        }
    }

    public string License {
        get { return (_license != null ? _license.Text : sLicense);}
        set {
            sLicense = value;
            if (_license != null)
                _license.Text = "License: " + value;
        }
    }

    public string Author {
        get { return (_author != null ? _author.Text : sAuthor); }
        set {
            sAuthor = value;
            if (_author != null)
                _author.Text = "Author: " + value;
        }
    }

    public string AssetId { get; set; }
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        Icon = tIcon;
        Title = sTitle;
        Category = sCategory;
        License = sLicense;
        Author = sAuthor;
        Connect("mouse_entered", this, "OnMouseEntered");
        Connect("mouse_exited", this, "OnMouseExited");
        Connect("gui_input", this, "OnGuiInput");
    }

    void OnMouseEntered() {
        Color = new Color("2a2e37");
    }

    void OnMouseExited() {
        Color = new Color("002a2e37");
    }

    async void OnGuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iembEvent) {
            if (iembEvent.Pressed && (ButtonList)iembEvent.ButtonIndex == ButtonList.Left)
            {
                AppDialogs.Instance.BusyDialog.UpdateHeader("Getting asset information...");
                AppDialogs.Instance.BusyDialog.UpdateByline("Connecting...");
                AppDialogs.Instance.BusyDialog.ShowDialog();
                Task<AssetLib.Asset> asset = AssetLib.AssetLib.Instance.GetAsset(AssetId);
                while (!asset.IsCompleted)
                    await this.IdleFrame();
                AppDialogs.Instance.BusyDialog.Visible = false;
                AppDialogs.Instance.AssetLibPreview.ShowDialog(asset.Result);
            }
        }
    }
}
