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

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/GodotVersion")]
    OptionButton _godotVersion = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/VBoxContainer/GLES3")]
    CheckBox _gles3 = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/VBoxContainer2/GLES2")]
    CheckBox _gles2 = null;

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
    }

    public void ShowWarning(string warn_msg) {
        _errorText.Text = warn_msg;
        _errorIcon.Texture = StatusWarning;
    }

    public void ShowSuccess() {
        _errorText.Text = "";
        _errorIcon.Texture = StatusSuccess;
    }
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        ShowError();
        _projectLocation.Connect("text_changed", this, "OnProjectLocation_TextChanged");
        _cancelBtn.Connect("pressed", this, "OnCancelPressed");
        _createBtn.Connect("pressed", this, "OnCreatePressed");
        ProjectPath = CentralStore.Settings.ProjectPath;
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

        Visible = true;
    }

    void OnProjectLocation_TextChanged(string new_text) {
        if (System.IO.Directory.Exists(new_text)) {
            if (System.IO.Directory.GetDirectories(new_text).Length == 0 &&
                System.IO.Directory.GetFiles(new_text).Length == 0) {
                ShowSuccess();
            } else {
                ShowError();
            }
        } else {
            ShowWarning("Base Directory does not exist!");
        }
    }
}
