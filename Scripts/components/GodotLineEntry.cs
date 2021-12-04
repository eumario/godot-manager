using Godot;
using GodotSharpExtras;
using System;

public class GodotLineEntry : HBoxContainer
{
#region Private Node Variables
    [NodePath("vc/VersionTag")]
    private Label _label = null;
    [NodePath("vc/hc/Source")]
    private Label _source = null;
    [NodePath("vc/hc/Filesize")]
    private Label _filesize = null;
    [NodePath("Download")]
    private TextureRect _download = null;
    [NodePath("Default")]
    private TextureRect _default = null;

    private StreamTexture downloadIcon;
    private StreamTexture uninstallIcon;
#endregion

#region Private String Variables
    private string sLabel = "Godot Version x.x.x (Stable)";
    private string sSource = "Source: TuxFamily.org";
    private string sFilesize = "Size: 32MB";
    private bool bDownloaded = false;
    private bool bDefault = false;
    private GodotVersion gvGodotVersion = null;
#endregion

#region Public Accessors
    public GodotVersion GodotVersion {
        get {
            return gvGodotVersion;
        }

        set {
            gvGodotVersion = value;
            Label = value.Tag;
            
        }
    }

    public string Label {
        get {
            return sLabel;
        }
        set {
            sLabel = value;
            if (_label != null)
                _label.Text = $"Godot {value}";
        }
    }

    public string Source {
        get {
            return sSource;
        }

        set {
            sSource = value;
            if (_source != null)
                _source.Text = $"Source: {value}";
        }
    }

    public string Filesize {
        get {
            return sFilesize;
        }
        set {
            sFilesize = value;
            if (_filesize != null)
                _filesize.Text = $"Size: {value}";
        }
    }

    [Export]
    public bool Downloaded {
        get {
            return bDownloaded;
        }

        set {
            bDownloaded = value;
            if (_download != null) {
                ToggleDownloadUninstall(bDownloaded);
            }
        }
    }

    public bool IsDownloaded {
        get {
            return bDownloaded;
        }
    }

    public bool IsDefault {
        get {
            return bDefault;
        }
    }
#endregion


    public override void _Ready()
    {
        this.OnReady();

        Label = sLabel;
        Source = sSource;
        Filesize = sFilesize;

        _download.Connect("gui_input", this, "OnDownload_GuiInput");
        _default.Connect("gui_input", this, "OnDefault_GuiInput");
        downloadIcon = GD.Load<StreamTexture>("res://Assets/Icons/download.svg");
        uninstallIcon = GD.Load<StreamTexture>("res://Assets/Icons/uninstall.svg");
        Downloaded = bDownloaded;
    }

    public void ToggleDownloadUninstall(bool value) {
        if (value) {
            _download.Texture = uninstallIcon;
            _download.SelfModulate = new Color("ff0000");
        }
        else {
            _download.Texture = downloadIcon;
            _download.SelfModulate = new Color("00ff00");
        }
    }

    public void ToggleDefault(bool value) {
        if (value) {
            _default.SelfModulate = new Color("ffffff");
        } else {
            _default.SelfModulate = new Color("ffff00");
        }
    }

    public void OnDownload_GuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed && (ButtonList)iemb.ButtonIndex != ButtonList.Left)
            return;
        
        ToggleDownloadUninstall((_download.Texture == downloadIcon));
    }

    public void OnDefault_GuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed && (ButtonList)iemb.ButtonIndex != ButtonList.Left)
            return;
        
        ToggleDefault((_default.SelfModulate == new Color("ffff00")));
    }
}
