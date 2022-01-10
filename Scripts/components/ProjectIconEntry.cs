using Godot;
using GodotSharpExtras;
using System;

public class ProjectIconEntry : CenterContainer
{
#region Private Node Variables
    [NodePath("vc/ProjectIcon")]
    private TextureRect _icon = null;
    [NodePath("vc/ProjectName")]
    private Label _projectName = null;
    [NodePath("vc/ProjectLocation")]
    private Label _projectLocation = null;
    [NodePath("vc/GodotVersion")]
    private Label _godotVersion = null;
#endregion

#region Private Variables
    private string sIcon;
    private string sProjectName;
    private string sProjectLocation;
    private int iGodotVersion;
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

    public string ProjectLocation {
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
            ProjectLocation = value.Location;
            var gv = CentralStore.Instance.FindVersion(value.GodotVersion);
            GodotVersion = CentralStore.Versions.IndexOf(gv);
        }
    }

    public int GodotVersion {
        get {
            if (_godotVersion != null)
                return (int)_godotVersion.Get("GodotVersion");
            else
                return iGodotVersion;
        }

        set {
            iGodotVersion = value;
            if (_godotVersion != null) {
                _godotVersion.Set("GodotVersion", value);
                if (iGodotVersion >= 0)
                    _godotVersion.Text = CentralStore.Versions[value].GetDisplayName();
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
        ProjectLocation = sProjectLocation;
        GodotVersion = iGodotVersion;
    }
}
