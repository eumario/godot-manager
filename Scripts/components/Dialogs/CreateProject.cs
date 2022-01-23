using Godot;
using GodotSharpExtras;

public class CreateProject : ReferenceRect
{
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
    public string ProjectPath = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects");
#endregion

#region Helper Functions
    public void ShowError() {
        _errorText.Text = "Please choose an empty folder.";
        _errorIcon.Texture = StatusError;
        _createBtn.Disabled = true;
    }

    public void ShowWarning(string warn_msg) {
        _errorText.Text = warn_msg;
        _errorIcon.Texture = StatusWarning;
        _createBtn.Disabled = true;
    }

    public void ShowSuccess() {
        _errorText.Text = "";
        _errorIcon.Texture = StatusSuccess;
        _createBtn.Disabled = false;
    }
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        ShowError();
        _projectLocation.Connect("text_changed", this, "OnProjectLocation_TextChanged");
        _createFolder.Connect("pressed", this, "OnCreateFolderPressed");
        _browseLocation.Connect("pressed", this, "OnBrowseLocationPressed");
        _cancelBtn.Connect("pressed", this, "OnCancelPressed");
        _createBtn.Connect("pressed", this, "OnCreatePressed");
        ProjectPath = CentralStore.Settings.ProjectPath;
    }

    void OnCreateFolderPressed() {
        if (_errorIcon.Texture == StatusWarning)
            AppDialogs.MessageDialog.ShowMessage("Create Folder", "You need to select a valid base directory to store this project in.");
        string path = _projectLocation.Text;
        string newDir = path.Join(_projectName.Text);
        System.IO.Directory.CreateDirectory(newDir);
        OnProjectLocation_TextChanged(_projectLocation.Text);
    }

    void OnBrowseLocationPressed() {
        AppDialogs.BrowseFolderDialog.CurrentFile = "";
        AppDialogs.BrowseFolderDialog.CurrentPath = "";
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnDirSelected");
    }

    void OnDirSelected(string bfdir) {
        _projectLocation.Text = bfdir;
        AppDialogs.BrowseFolderDialog.Visible = false;
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnDirSelected");
    }

    void OnCancelPressed() {
        Visible = false;
    }

    public void ShowDialog() {
        int defaultGodot = -1;
        _projectName.Text = "Untitled Project";
        _projectLocation.Text = ProjectPath;

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
        }

        foreach(CheckBox plugin in _pluginList.GetChildren())
            plugin.QueueFree();
        
        foreach(AssetPlugin plgn in CentralStore.Plugins) {
            CheckBox plugin = new CheckBox();
            plugin.Text = plgn.Asset.Title;
            _pluginList.AddChild(plugin);
        }
        Visible = true;
    }

    void OnProjectLocation_TextChanged(string new_text) {
        if (System.IO.Directory.Exists(new_text)) {
            if (System.IO.Directory.Exists(new_text.Join(_projectName.Text)))
                ShowSuccess();
            else if (System.IO.Directory.GetDirectories(new_text).Length == 0 &&
                System.IO.Directory.GetFiles(new_text).Length == 0) {
                ShowWarning("Project Directory does not exist!");
            } else {
                ShowError();
            }
        } else {
            ShowWarning("Base Directory does not exist!");
        }
    }
}
