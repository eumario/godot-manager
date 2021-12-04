using Godot;
using GodotSharpExtras;
using System;

[Tool]
public class CategoryList : VBoxContainer
{

#region Node Variables
    [NodePath("CategoryName")]
    private Label _categoryName = null;
    [NodePath("CategoryList")]
    private VBoxContainer _categoryList = null;
    [NodePath("hc/ToggleIcon")]
    private TextureRect _toggleIcon = null;
#endregion

#region Private Variables
    private string sText;
    private bool bToggable;
    private bool bToggled;
#endregion

#region Exports / Class Fields
    [Export]
    public string CategoryName {
        get {
            if (_categoryName != null)
                return _categoryName.Text;
            else
                return sText;
        }

        set {
            sText = value;
            if (_categoryName != null)
                _categoryName.Text = value;
        }
    }

    [Export]
    public bool Toggable {
        get {
            if (_toggleIcon != null)
                return _toggleIcon.Visible;
            else
                return bToggable;
        }

        set {
            bToggable = value;
            if (_toggleIcon != null)
                _toggleIcon.Visible = value;
        }
    }

    public bool Toggled {
        get {
            if (_toggleIcon != null)
                return _toggleIcon.FlipV;
            else
                return bToggled;
        }

        set {
            bToggled = value;
            if (_toggleIcon != null)
                _toggleIcon.FlipV = value;
        }
    }

    public VBoxContainer List {
        get {
            return _categoryList;
        }
    }
#endregion

#region Templates
    private PackedScene pstProject = GD.Load<PackedScene>("res://components/ProjectLineEntry.tscn");
    private PackedScene pstGodot = GD.Load<PackedScene>("res://components/GodotLineEntry.tscn");
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        
        CategoryName = sText;
        Toggable = bToggable;
        Toggled = bToggled;

        _toggleIcon.Connect("gui_input", this, "OnToggle_GuiInput");
    }

    public void OnToggle_GuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed)
            return;
        if ((ButtonList)iemb.ButtonIndex != ButtonList.Left)
            return;
        
        _toggleIcon.FlipV = !_toggleIcon.FlipV;
        if (_toggleIcon.FlipV)
            _categoryList.Hide();
        else
            _categoryList.Show();
    }

    public void AddProject(ProjectFile projectFile) {
        ProjectLineEntry ple = pstProject.Instance<ProjectLineEntry>();
        ple.ProjectFile = projectFile;
        _categoryList.AddChild(ple);
    }

    public void AddGodotVersion(GodotVersion godotVersion) {
        GodotLineEntry gle = pstGodot.Instance<GodotLineEntry>();
        gle.GodotVersion = godotVersion;
        _categoryList.AddChild(gle);
    }
}
