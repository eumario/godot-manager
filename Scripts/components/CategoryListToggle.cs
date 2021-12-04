using Godot;
using GodotSharpExtras;
using System;

public class CategoryListToggle : VBoxContainer
{
    [NodePath("CategoryName")]
    private Label _categoryName = null;
    [NodePath("HBoxContainer/TextureRect")]
    private TextureRect _toggle = null;
    [NodePath("CategoryList")]
    private VBoxContainer _categoryList = null;

    private PackedScene _template = GD.Load<PackedScene>("res://components/ProjectLineEntry.tscn");

    private string sName;

    public string CategoryName {
        get {
            if (_categoryName == null)
                return sName;
            else
                return _categoryName.Text;
        }

        set {
            sName = value;
            if (_categoryName != null)
                _categoryName.Text = value;
        }
    }

    public bool IsToggled {
        get {
            return _toggle.FlipV;
        }
    }

    public void SetToggle(bool value) {
        _toggle.FlipV = value;
        if (_toggle.FlipV)
            _categoryList.Hide();
        else
            _categoryList.Show();
    }

    public VBoxContainer CategoryList {
        get {
            return _categoryList;
        }
    }

    public override void _Ready()
    {
        this.OnReady();

        CategoryName = sName;

        _toggle.Connect("gui_input", this, "OnToggle_GuiInput");
    }

    public void OnToggle_GuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed)
            return;
        
        if ((ButtonList)iemb.ButtonIndex != ButtonList.Left)
            return;
        
        _toggle.FlipV = !_toggle.FlipV;
        if (_toggle.FlipV) {
            _categoryList.Hide();
            return;
        }
        else
        {
            _categoryList.Show();
            return;
        }   
    }

    public void AddProject(ProjectFile projectFile) {
        ProjectLineEntry ple = _template.Instance<ProjectLineEntry>();
        ple.Name = projectFile.Name;
        ple.Description = projectFile.Description;
        ple.Icon = projectFile.Location.GetResourceBase(projectFile.Icon);
        ple.Location = projectFile.Location;
        _categoryList.AddChild(ple);
    }
}
