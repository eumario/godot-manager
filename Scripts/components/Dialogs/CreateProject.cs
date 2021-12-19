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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        ShowError();
        _cancelBtn.Connect("pressed", this, "ShowWarning");
        _createBtn.Connect("pressed", this, "ShowSuccess");
    }

    public void ShowError() {
        _errorText.Text = "Please choose an empty folder.";
        _errorIcon.Texture = StatusError;
    }

    public void ShowWarning() {
        _errorText.Text = "";
        _errorIcon.Texture = StatusWarning;
    }

    public void ShowSuccess() {
        _errorText.Text = "";
        _errorIcon.Texture = StatusSuccess;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
