using Godot;
using GodotSharpExtras;
using Godot.Collections;

public class EditProject : ReferenceRect
{
#region Signals
    [Signal]
    public delegate void project_updated();
#endregion

#region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/VC/HC/ProjectIcon")]
    TextureRect _Icon = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/HC/MC/VC/ProjectName")]
    LineEdit _ProjectName = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/GodotVersion")]
    OptionButton _GodotVersion = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/ProjectDescription")]
    TextEdit _ProjectDescription = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/SaveBtn")]
    Button _SaveBtn = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")]
    Button _CancelBtn = null;
#endregion

#region Private Variables
    ProjectFile _pf = null;
    bool _isDirty = false;
    EPData _data;
#endregion

#region Internal Structure
    struct EPData {
        public string IconPath;
        public string ProjectName;
        public string GodotVersion;
        public string Description;
    }
#endregion

#region Public Variables
    public string IconPath {
        get => _data.IconPath;
        set => _data.IconPath = value;
    }

    public string ProjectName {
        get => _data.ProjectName;
        set => _data.ProjectName = value;
    }

    public string GodotVersion { 
        get => _data.GodotVersion;
        set => _data.GodotVersion = value;
    }

    public string Description {
        get => _data.Description;
        set => _data.Description = value;
    }

    public ProjectFile ProjectFile {
        get => _pf;
        set {
            _pf = value;
            IconPath = _pf.Icon;
            ProjectName = _pf.Name;
            GodotVersion = _pf.GodotVersion;
            Description = _pf.Description;
        }
    }
#endregion

    // TODO: Need to implement UI Glue to Data Backend, once Data has been edited, mark as dirty, and enable Save Button.
    // Warn when Dirty when closing / canceling the dialog.
    // Once Confirmed to make changes, save the CentralStore database.  (Need to set a revert point, so maybe not directly
    // accessing _pf is a good idea.)
    public override void _Ready()
    {
        this.OnReady();
        _data = new EPData();

        _SaveBtn.Connect("pressed", this, "OnSaveBtnPressed");
        _CancelBtn.Connect("pressed", this, "OnCancelBtnPressed");
        _Icon.Connect("gui_input", this, "OnIconGuiInput");
        _ProjectName.Connect("text_changed", this, "OnProjectNameTextChanged");
        _GodotVersion.Connect("item_selected", this, "OnGodotVersionItemSelected");
        _ProjectDescription.Connect("text_changed", this, "OnProjectDescriptionTextChanged");
    }

#region Public Functions
    public void ShowDialog(ProjectFile pf) {
        ProjectFile = pf;
        PopulateData();
        Visible = true;
    }
#endregion

#region Private Functions
    void PopulateData() {
        _Icon.Texture = ProjectFile.Location.GetResourceBase(IconPath).LoadImage();
        _ProjectName.Text = ProjectName;
        _ProjectDescription.Text = Description;
        _GodotVersion.Clear();
        foreach(GodotVersion gdver in CentralStore.Versions) {
            string gdName = gdver.GetDisplayName();
            if (gdver.Id == CentralStore.Settings.DefaultEngine)
                gdName += " (Default)";
            _GodotVersion.AddItem(gdName);
            _GodotVersion.SetItemMetadata(_GodotVersion.GetItemCount()-1, gdver.Id);
            if (ProjectFile.GodotVersion == gdver.Id)
                _GodotVersion.Selected = _GodotVersion.GetItemCount()-1;
        }
        _isDirty = false;
        _SaveBtn.Disabled = true;
    }
#endregion

#region Event Handlers
    void OnSaveBtnPressed() {
        ProjectFile.Name = ProjectName;
        ProjectFile.Description = Description;
        ProjectFile.Icon = IconPath;
        ProjectFile.GodotVersion = GodotVersion;
        ProjectFile.WriteUpdatedData();
        CentralStore.Instance.SaveDatabase();
        Visible = false;
        EmitSignal("project_updated");
    }

    async void OnCancelBtnPressed() {
        if (_isDirty) {
            var res = AppDialogs.YesNoDialog.ShowDialog("Edit Project", "There is unsaved changes, do you wish to continue?");
            await res;
            if (res.Result) {
                Visible = false;
            }
        } else {
            Visible = false;
        }
    }

    void OnIconGuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb) {
            if (iemb.Pressed && iemb.ButtonIndex == (int)ButtonList.Left)
                AppDialogs.ImageFileDialog.Connect("file_selected", this, "OnFileSelected");
                AppDialogs.ImageFileDialog.Connect("popup_hide", this, "OnFilePopupHide");
                AppDialogs.ImageFileDialog.CurrentDir = ProjectFile.Location.GetBaseDir();
                AppDialogs.ImageFileDialog.PopupCentered();
        }
    }

    async void OnFileSelected(string path) {
        if (path == "")
            return;
        var pfpath = ProjectFile.Location.GetBaseDir().Replace(@"\", "/");
        if (path.StartsWith(pfpath))
            IconPath = pfpath.GetProjectRoot(path);
        else {
            var ret = AppDialogs.YesNoDialog.ShowDialog("Icon Selection","This file is outside your project structure, do you want to copy it to the root of your project?");
            await ret;
            if (ret.Result) {
                var dir = new Directory();
                GD.Print($"src: {path}\ndest: {pfpath.PlusFile(path.GetFile())}");
                dir.Copy(path, pfpath.PlusFile(path.GetFile()));
                path = pfpath.PlusFile(path.GetFile());
                IconPath = pfpath.GetProjectRoot(path);
            } else {
                AppDialogs.MessageDialog.ShowMessage("Icon Selection", "Icon not copied, unable to use icon for Project.");
                AppDialogs.ImageFileDialog.Visible = false;
                return;
            }
        }
        _Icon.Texture = path.LoadImage();
        GD.Print($"pfpath: {pfpath}\nIconPath: {IconPath}\npath: {path}");
        AppDialogs.ImageFileDialog.Visible = false;
        _isDirty = true;
        _SaveBtn.Disabled = false;
    }

    void OnFilePopupHide() {
        AppDialogs.ImageFileDialog.Disconnect("file_selected", this, "OnFileSelected");
        AppDialogs.ImageFileDialog.Disconnect("popup_hide", this, "OnFilePopupHide");
    }

    void OnProjectNameTextChanged(string text) {
        ProjectName = text;
        _isDirty = true;
        _SaveBtn.Disabled = false;
    }

    void OnGodotVersionItemSelected(int index) {
        GodotVersion = _GodotVersion.GetItemMetadata(index) as string;
        _isDirty = true;
        _SaveBtn.Disabled = false;
    }

    void OnProjectDescriptionTextChanged() {
        Description = _ProjectDescription.Text;
        _isDirty = true;
        _SaveBtn.Disabled = false;
    }
#endregion

}
