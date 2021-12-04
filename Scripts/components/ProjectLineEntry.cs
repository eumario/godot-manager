using Godot;
using GodotSharpExtras;
using System;

public class ProjectLineEntry : ColorRect
{
#region Signals
    [Signal]
    public delegate void Clicked(ProjectLineEntry self);
    [Signal]
    public delegate void DoubleClicked(ProjectLineEntry self);
#endregion

#region Private Node Variables
    [NodePath("hc/ProjectIcon")]
    private TextureRect _icon = null;
    [NodePath("hc/vc/ProjectName")]
    private Label _name = null;
    [NodePath("hc/vc/ProjectDesc")]
    private Label _desc = null;
    [NodePath("hc/vc/ProjectLocation")]
    private Label _location = null;
    [NodePath("hc/GodotVersion")]
    private Label _version = null;
    [NodePath("hc/SubMenu")]
    private TextureRect _subMenu = null;
#endregion

#region Private Variables
    private string sIcon = "res://Assets/Icons/missing_icon.svg";
    private string sName = "Project Name";
    private string sDesc = "Project Description";
    private string sLocation = "/home/eumario/Projects/Godot/ProjectName";
    private int iGodotVersion = -1;
    private ProjectFile pfProjectFile = null;
#endregion

#region Public Accessors
    public ProjectFile ProjectFile {
        get {
            return pfProjectFile;
        }

        set {
            pfProjectFile = value;
            Icon = value.Location.GetResourceBase(value.Icon);
            Name = value.Name;
            Description = value.Description;
            Location = value.Location;
            GodotVersion = value.GodotVersion;
        }
    }

    public string Icon {
        get {
            if (_icon != null)
                return _icon.Texture.ResourcePath;
            else
                return sIcon;
        }

        set {
            sIcon = value;
            if (_icon != null)
                _icon.Texture = value.LoadImage();
        }
    }

    new public string Name {
        get {
            if (_name != null)
                return _name.Text;
            else
                return sName;
        }
        set {
            sName = value;
            if (_name != null)
                _name.Text = value;
        }
    }

    public string Description {
        get {
            return sDesc;
        }
        set {
            sDesc = value;
            if (_desc != null) {
                if (sDesc == null || sDesc.StripEdges() == "")
                    _desc.Text = "No Description";
                else
                    _desc.Text = value;
            }
        }
    }

    public string Location {
        get {
            return sLocation;
        }
        set {
            sLocation = value;
            if (_location != null)
                _location.Text = value.GetBaseDir();
        }
    }

    public int GodotVersion {
        get {
            if (_version != null) {
                return (int)_version.Get("godotVersion");
            } else {
                return iGodotVersion;
            }
        }

        set {
            iGodotVersion = value;
            if (_version != null) {
                _version.Set("godotVersion", value);
                if (value >= 0) {
                    _version.Text = CentralStore.Instance.Versions[value].Tag;
                } else {
                    _version.Text = "Unknown";
                }
            }
        }
    }
#endregion

    public override void _Ready()
    {
        this.OnReady();

        Icon = sIcon;
        Name = sName;
        Description = sDesc;
        Location = sLocation;
        GodotVersion = iGodotVersion;
        this.Connect("gui_input", this, "OnGuiInput");
    }

    public void OnGuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed)
            return;
        
        if (iemb.Doubleclick)
            EmitSignal("DoubleClicked", this);
        else {
            SelfModulate = new Color("ffffffff");
            EmitSignal("Clicked", this);
        }
    }
}