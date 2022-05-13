using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System;
using System.Linq;

[Tool]
public class CategoryList : VBoxContainer
{
#region Signals
    [Signal]
    public delegate void list_toggled();
#endregion

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
            if (_toggleIcon != null) {
                _toggleIcon.FlipV = value;
                if (_toggleIcon.FlipV)
                    _categoryList.Hide();
                else
                    _categoryList.Show();
            }
        }
    }

    public VBoxContainer List {
        get {
            return _categoryList;
        }
    }

    public ProjectLineEntry ProjectSelected {
        get {
            foreach(ProjectLineEntry ple in GetChildren()) {
                if (ple.SelfModulate == new Color("ffffffff"))
                    return ple;
            }
            return null;
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
    }

    [SignalHandler("gui_input", nameof(_toggleIcon))]
    void OnToggle_GuiInput(InputEvent inputEvent) {
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
        EmitSignal("list_toggled");
    }

    public async void SortListing() {
        Array<ProjectLineEntry> pleCache = new Array<ProjectLineEntry>();
        Array<ProjectFile> pfCache = new Array<ProjectFile>();

        foreach(ProjectLineEntry ple in _categoryList.GetChildren()) {
            pleCache.Add(ple);
            pfCache.Add(ple.ProjectFile);
            _categoryList.RemoveChild(ple);
        }

        await this.IdleFrame();

        var fav = pfCache.Where(pf => pf.Favorite)
                    .OrderByDescending(pf => pf.LastAccessed);
        
        var non_fav = pfCache.Where(pf => !pf.Favorite)
                    .OrderByDescending(pf => pf.LastAccessed);

        foreach(ProjectFile pf in fav) {
            int indx = pfCache.IndexOf(pf);
            _categoryList.AddChild(pleCache[indx]);
        }

        foreach(ProjectFile pf in non_fav) {
            int indx = pfCache.IndexOf(pf);
            _categoryList.AddChild(pleCache[indx]);
        }
    }

    public ProjectLineEntry AddProject(ProjectFile projectFile) {
        ProjectLineEntry ple = pstProject.Instance<ProjectLineEntry>();
        if (!ProjectFile.ProjectExists(projectFile.Location))
            ple.MissingProject = true;
        ple.ProjectFile = projectFile;
        _categoryList.AddChild(ple);
        return ple;
    }

    public void AddGodotVersion(GodotVersion godotVersion) {
        GodotLineEntry gle = pstGodot.Instance<GodotLineEntry>();
        gle.GodotVersion = godotVersion;
        _categoryList.AddChild(gle);
    }

	public override bool CanDropData(Vector2 position, object data)
	{
        if ((int)GetMeta("ID") == -1)
            return false;
        Dictionary dictData = data as Dictionary;
		CategoryList parent = dictData["parent"] as CategoryList;
        if (parent == this)
            return false;
        if (dictData["source"] is ProjectLineEntry)
            return true;
        return false;      
	}

	public override void DropData(Vector2 position, object data)
	{
		Dictionary dictData = data as Dictionary;
        CategoryList parent = dictData["parent"] as CategoryList;
        ProjectLineEntry ple = dictData["source"] as ProjectLineEntry;
        parent.List.RemoveChild(ple);
        List.AddChild(ple);
        ple.ProjectFile.CategoryId = (int)GetMeta("ID");
        CentralStore.Instance.SaveDatabase();
        parent.SortListing();
        SortListing();
	}
}
