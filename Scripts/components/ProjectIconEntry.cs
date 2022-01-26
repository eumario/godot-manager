using Godot;
using GodotSharpExtras;
using System;

public class ProjectIconEntry : ColorRect
{
#region Signals
    [Signal]
    public delegate void Clicked(ProjectLineEntry self);
    [Signal]
    public delegate void DoubleClicked(ProjectLineEntry self);
#endregion

#region Private Node Variables
    [NodePath("cc/vc/ProjectIcon")]
    private TextureRect _icon = null;
    [NodePath("cc/vc/ProjectName")]
    private Label _projectName = null;
    [NodePath("cc/vc/ProjectLocation")]
    private Label _projectLocation = null;
    [NodePath("cc/vc/GodotVersion")]
    private Label _godotVersion = null;
#endregion

#region Private Variables
    private string sIcon;
    private string sProjectName;
    private string sProjectLocation;
    private string sGodotVersion;
    //private int iGodotVersion;
    private ProjectFile pfProjectFile;
#endregion

#region Public Accessors
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

    public string ProjectName {
        get {
            if (_projectName != null)
                return _projectName.Text;
            else
                return sProjectName;
        }

        set {
            sProjectName = value;
            if (_projectName != null)
                _projectName.Text = value;
        }
    }

    public string Location {
        get {
            if (_projectLocation != null)
                return _projectLocation.Text;
            else
                return sProjectLocation;
        }

        set {
            sProjectLocation = value;
            if (_projectLocation != null)
                _projectLocation.Text = value;
        }
    }

    public ProjectFile ProjectFile {
        get {
            return pfProjectFile;
        }

        set {
            pfProjectFile = value;
            ProjectName = value.Name;
            Icon = value.Location.GetResourceBase(value.Icon);
            Location = value.Location;
            GodotVersion = value.GodotVersion;
        }
    }

    public string GodotVersion {
        get {
            return sGodotVersion;
        }

        set {
            sGodotVersion = value;
            GodotVersion gv = CentralStore.Instance.FindVersion(value);
            if (_godotVersion != null) {
                if (gv != null)
                    _godotVersion.Text = gv.GetDisplayName();
                else
                    _godotVersion.Text = "Unknown";
            }
        }
    }
#endregion

    public override void _Ready() {
        this.OnReady();

        Icon = sIcon;
        ProjectName = sProjectName;
        Location = sProjectLocation;
        GodotVersion = sGodotVersion;
        this.Connect("gui_input", this, "OnGuiInput");
    }

    void OnGuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed)
            return;
        
        if (iemb.ButtonIndex == (int)ButtonList.Left) {
            if (iemb.Doubleclick)
                EmitSignal("DoubleClicked", this);
            else {
                SelfModulate = new Color("ffffffff");
                EmitSignal("Clicked", this);
            }
        } else if (iemb.ButtonIndex == (int)ButtonList.Right) {
            // Handle Popup Menu, similar to the 3-Dot Menu Icon in ListView/CategoryView
        }

    }
}
