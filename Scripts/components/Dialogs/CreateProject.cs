using System;
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

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/GLES3/GLES3")]
    CheckBox _gles3 = null;
    
    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/GLES3/GLES3Desc")]
    Label _gles3Desc = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/GLES2/GLES2")]
    CheckBox _gles2 = null;
    
    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Settings/HBoxContainer3/GLES2/GLES2Desc")]
    Label _gles2Desc = null;

    [NodePath("PC/CC/P/VB/MCContent/TabContainer/Project Plugins/ScrollContainer/List")]
    VBoxContainer _pluginList = null;

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

#region Assets
    [Resource("res://components/AddonLineEntry.tscn")] private PackedScene ALineEntry = null;
    [Resource("res://Assets/Icons/default_project_icon.png")] private Texture DefaultIcon = null;
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
        
        foreach(AddonLineEntry ale in _pluginList.GetChildren()) {
            if (ale.Installed) {
                prj.Plugins.Add(ale.GetMeta("asset") as AssetPlugin);
            }
        }

        prj.CreateProject();
        ProjectFile pf = ProjectFile.ReadFromFile(prj.ProjectLocation.PlusFile("project.godot").NormalizePath());
        pf.GodotVersion = prj.GodotVersion;
        pf.Assets = new Array<string>();
        
        foreach(AssetPlugin plugin in prj.Plugins)
            pf.Assets.Add(plugin.Asset.AssetId);
        
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
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnDirSelected", null, (int)ConnectFlags.Oneshot);
        AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnDirSelected_PopupHidden", null, (int)ConnectFlags.Oneshot);
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

    void OnDirSelected_PopupHidden()
    {
        if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnDirSelected"))
            AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnDirSelected");
    }

    [SignalHandler("toggled", nameof(_useGodot3))]
    void OnToggled_UseGodot3(bool toggle)
    {
        _gles3.Text = Tr("OpenGL ES 3.0");
        _gles2.Text = Tr("OpenGL ES 2.0");
        _gles3Desc.Text = Tr(@"Higher Visual Quality
All Features available
Incompatible with older hardware
Not recommended for web games");
        _gles2Desc.Text = Tr(@"Lower Visual Quality
Some Features not available
Works on most hardware
Recommended for web games");
        PopulateEngines();
    }

    [SignalHandler("toggled", nameof(_useGodot4))]
    void OnToggled_UseGodot4(bool toggle)
    {
        _gles3.Text = Tr("Forward+");
        _gles2.Text = Tr("Mobile");
        _gles3Desc.Text = Tr(@"Supports desktop platforms only.
Advanced 3D graphics available.
Can scale to large complex scenes.
Slower rendering of simple scenes.");
        _gles2Desc.Text = Tr(@"Supports desktop + mobile platforms.
Less advanced 3D graphics.
Less scalable for complex scenes.
Faster rendering of simple scenes.");
        PopulateEngines();
    }

    [SignalHandler("pressed", nameof(_cancelBtn))]
    void OnCancelPressed() {
        Visible = false;
    }

    public void ShowDialog() {
        _projectName.Text = "Untitled Project";
        _projectLocation.Text = CentralStore.Settings.ProjectPath;
        TestPath(CentralStore.Settings.ProjectPath);
        
        PopulateEngines(true);

        _gles3.Pressed = true;
        _gles2.Pressed = false;

        _projectTemplates.Clear();
        _projectTemplates.AddItem("None");
        foreach(AssetProject tmpl in CentralStore.Templates) {
            string gdName = tmpl.Asset.Title;
            _projectTemplates.AddItem(gdName);
            _projectTemplates.SetItemMetadata(CentralStore.Templates.IndexOf(tmpl)+1, tmpl);
        }

        foreach (AddonLineEntry node in _pluginList.GetChildren()) node.QueueFree();

        foreach (AssetPlugin plgn in CentralStore.Plugins)
        {
            string imgLoc =
                $"{CentralStore.Settings.CachePath}/images/{plgn.Asset.AssetId}{plgn.Asset.IconUrl.GetExtension()}"
                    .NormalizePath();
            AddonLineEntry ale = ALineEntry.Instance<AddonLineEntry>();

            ale.Icon = Util.LoadImage(imgLoc);
            if (ale.Icon == null) ale.Icon = DefaultIcon;

            ale.Title = plgn.Asset.Title;
            ale.Version = plgn.Asset.VersionString;
            ale.SetMeta("asset", plgn);
            _pluginList.AddChild(ale);
        }
        
        
        Visible = true;
    }

    private void PopulateEngines(bool updateUse = false)
    {
        int defaultGodot = -1;
        if (CentralStore.Settings.DefaultEngine != Guid.Empty.ToString())
        {
            var defaultEngine = CentralStore.Instance.GetVersion(CentralStore.Settings.DefaultEngine);
            if (defaultEngine != null)
            {
                if (updateUse)
                {
                    if (defaultEngine.IsGodot4())
                    {
                        _useGodot4.Pressed = true;
                    }
                }
            }
        }

        _godotVersion.Clear();
        var indx = 0;
        foreach (GodotVersion version in CentralStore.Versions)
        {
            if (_useGodot4.Pressed == version.IsGodot4())
            {
                string gdName = version.GetDisplayName();
                if (version.Id == (string)CentralStore.Settings.DefaultEngine)
                {
                    defaultGodot = indx;
                    gdName += " (Default)";
                }

                _godotVersion.AddItem(gdName, indx);
                _godotVersion.SetItemMetadata(indx, version.Id);
                indx++;
            }
        }

        if (defaultGodot != -1)
            _godotVersion.Select(defaultGodot);
    }

    [SignalHandler("text_changed", nameof(_projectLocation))]
    void OnProjectLocation_TextChanged(string new_text) {
        TestPath(new_text);
    }

    private void TestPath(string path) {
        if (!Directory.Exists(path)) {
            ShowMessage(Tr("The path specified doesn't exist."), DirError.ERROR);
            return;
        }
        
        if (!path.IsDirEmpty()) {
            ShowMessage(Tr("Please choose an empty folder."), DirError.ERROR);
        } else {
            ShowMessage("",DirError.OK);
        }
    }
}
