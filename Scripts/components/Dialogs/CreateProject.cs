using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using Directory = System.IO.Directory;

public class CreateProject : ReferenceRect
{

#region Signals
    [Signal]
    public delegate void project_created(ProjectFile projFile);
#endregion

#region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer/ProjectName")]
    LineEdit _projectName = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer/CreateFolder")]
    Button _createFolder = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer2/ProjectLocation")]
    LineEdit _projectLocation = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer2/Browse")]
    Button _browseLocation = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer2/ErrorIcon")]
    TextureRect _errorIcon = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/ErrorText")]
    Label _errorText = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/CenterContainer/HBoxContainer4/Godot3")]
    CheckBox _useGodot3 = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/CenterContainer/HBoxContainer4/Godot4")]
    CheckBox _useGodot4 = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/ProjectTemplates")]
    OptionButton _projectTemplates = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/GodotVersion")]
    OptionButton _godotVersion = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/VBoxContainer/GLES3")]
    CheckBox _gles3 = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/VBoxContainer2/GLES2")]
    CheckBox _gles2 = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Plugins/ScrollContainer/VB/List")]
    GridContainer _pluginList = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CreateBtn")]
    Button _createBtn = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")]
    Button _cancelBtn = null;
#endregion

#region Resources
    Texture StatusError = GD.Load<Texture>("res://Assets/Icons/icon_status_error.svg");
    Texture StatusSuccess = GD.Load<Texture>("res://Assets/Icons/icon_status_success.svg");
    Texture StatusWarning = GD.Load<Texture>("res://Assets/Icons/icon_status_warning.svg");
#endregion

#region Variables
#endregion

#region Helper Functions
    public enum DirError {
        OK,
        ERROR,
        WARNING
    }

    public void ShowMessage(string msg, DirError err) {
        _errorText.Text = msg;
        switch(err) {
            case DirError.OK:
                _errorIcon.Texture = StatusSuccess;
                _createBtn.Disabled = false;
                break;
            case DirError.WARNING:
                _errorIcon.Texture = StatusWarning;
                _createBtn.Disabled = false;
                break;
            case DirError.ERROR:
                _errorIcon.Texture = StatusError;
                _createBtn.Disabled = true;
                break;
        }
    }
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        ShowMessage("",DirError.OK);
    }

    [SignalHandler("pressed", nameof(_createBtn))]
    void OnCreatePressed() {
        NewProject prj = new NewProject {
            ProjectName = _projectName.Text,
            ProjectLocation = _projectLocation.Text,
            GodotVersion = _godotVersion.GetSelectedMetadata() as string,
            Gles3 = _gles3.Pressed,
            Godot4 = _useGodot4.Pressed,
            Plugins = new Array<AssetPlugin>()
        };
        if (_projectTemplates.Selected > 0)
            prj.Template = _projectTemplates.GetSelectedMetadata() as AssetProject;
        
        foreach(CheckBox cp in _pluginList.GetChildren()) {
            if (cp.Pressed) {
                prj.Plugins.Add(cp.GetMeta("asset") as AssetPlugin);
            }
        }
        prj.CreateProject();
        ProjectFile pf = ProjectFile.ReadFromFile(prj.ProjectLocation.PlusFile("project.godot").NormalizePath());
        pf.GodotVersion = prj.GodotVersion;
        CentralStore.Projects.Add(pf);
        CentralStore.Instance.SaveDatabase();
        EmitSignal("project_created", pf);
        Visible = false;
    }

    [SignalHandler("pressed", nameof(_createFolder))]
    void OnCreateFolderPressed() {
        string path = _projectLocation.Text;
        string newDir = path.Join(_projectName.Text).NormalizePath();
        Directory.CreateDirectory(newDir);
        _projectLocation.Text = newDir;
        TestPath(newDir);
    }

    [SignalHandler("pressed", nameof(_browseLocation))]
    void OnBrowseLocationPressed() {
        AppDialogs.BrowseFolderDialog.CurrentFile = "";
        AppDialogs.BrowseFolderDialog.CurrentPath = (CentralStore.Settings.ProjectPath + "/").NormalizePath();
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnDirSelected");
    }

    void OnDirSelected(string bfdir) {
        bfdir = bfdir.NormalizePath();
        _projectLocation.Text = bfdir;
        AppDialogs.BrowseFolderDialog.Visible = false;
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnDirSelected");
        TestPath(bfdir);
        if (bfdir.IsDirEmpty() && _projectName.Text == "Untitled Project")
            _projectName.Text = bfdir.GetFile().Capitalize();
    }

    [SignalHandler("pressed", nameof(_cancelBtn))]
    void OnCancelPressed() {
        Visible = false;
    }

    public void ShowDialog() {
        int defaultGodot = -1;
        _projectName.Text = "Untitled Project";
        _projectLocation.Text = CentralStore.Settings.ProjectPath;
        TestPath(CentralStore.Settings.ProjectPath);

        _godotVersion.Clear();
        foreach(GodotVersion version in CentralStore.Versions) {
            string gdName = version.GetDisplayName();
            int indx = CentralStore.Versions.IndexOf(version);
            if (version.Id == (string)CentralStore.Settings.DefaultEngine) {
                defaultGodot = indx;
                gdName += " (Default)";
            }
            _godotVersion.AddItem(gdName, indx);
            _godotVersion.SetItemMetadata(indx, version.Id);
        }

        if (defaultGodot != -1)
            _godotVersion.Select(defaultGodot);

        _gles3.Pressed = true;
        _gles2.Pressed = false;

        _projectTemplates.Clear();
        _projectTemplates.AddItem("None");
        foreach(AssetProject tmpl in CentralStore.Templates) {
            string gdName = tmpl.Asset.Title;
            _projectTemplates.AddItem(gdName);
            _projectTemplates.SetItemMetadata(CentralStore.Templates.IndexOf(tmpl)+1, tmpl);
        }

        foreach(CheckBox plugin in _pluginList.GetChildren())
            plugin.QueueFree();
        
        foreach(AssetPlugin plgn in CentralStore.Plugins) {
            CheckBox plugin = new CheckBox();
            plugin.Text = plgn.Asset.Title;
            plugin.SetMeta("asset",plgn);
            _pluginList.AddChild(plugin);
        }
        Visible = true;
    }

    [SignalHandler("text_changed", nameof(_projectLocation))]
    void OnProjectLocation_TextChanged(string new_text) {
        TestPath(new_text);
    }

    private void TestPath(string path) {
        if (!Directory.Exists(path)) {
            ShowMessage("The path specified doesn't exist.", DirError.ERROR);
            return;
        }
        
        if (!path.IsDirEmpty()) {
            ShowMessage("Please choose an empty folder.", DirError.ERROR);
        } else {
            ShowMessage("",DirError.OK);
        }
    }
}
