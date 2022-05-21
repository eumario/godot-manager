using Godot;
using Godot.Sharp.Extras;
using Guid = System.Guid;

public class AddCustomGodot : ReferenceRect
{

#region Signals
    [Signal]
    public delegate void added_custom_godot();
#endregion

#region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/VB/Name")]
    LineEdit _Name = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/HBoxContainer/Location")]
    LineEdit _Location = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/HBoxContainer/Browse")]
    Button _Browse = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/MonoEnabled")]
    CheckBox _MonoEnabled = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/AddBtn")]
    Button _AddBtn = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")]
    Button _CancelBtn = null;
#endregion

    public override void _Ready()
    {
        this.OnReady();
    }

#region Public Functions
    public void ShowDialog() {
        _Name.Text = "";
        _Location.Text = "";
        _MonoEnabled.Pressed = false;
        Visible = true;
    }
#endregion

#region Events
    [SignalHandler("pressed", nameof(_Browse))]
    void OnBrowsePressed() {
        AppDialogs.BrowseGodotDialog.Connect("file_selected", this, "OnFileSelected");
        AppDialogs.BrowseGodotDialog.Connect("popup_hide", this, "OnBrowseDialogHidden");
        AppDialogs.BrowseGodotDialog.PopupCentered();
    }

    void OnFileSelected(string file) {
        _Location.Text = file;
    }

    void OnBrowseDialogHidden() {
        AppDialogs.BrowseGodotDialog.Disconnect("file_selected", this, "OnFileSelected");
        AppDialogs.BrowseGodotDialog.Disconnect("popup_hide", this, "OnBrowseDialogHidden");
    }

    [SignalHandler("pressed", nameof(_AddBtn))]
    void OnAddPressed() {
        if (_Name.Text == "" || _Location.Text == "") {
            AppDialogs.MessageDialog.ShowMessage(Tr("Add Custom Godot"), Tr("Need to provide a Name and a location for the custom version of the Godot engine."));
            return;
        }

        GodotVersion gv = new GodotVersion();
        gv.Id = Guid.NewGuid().ToString();
        gv.Tag = _Name.Text;
        gv.Url = "Local";
#if GODOT_MACOS || GODOT_OSX
        gv.Location = _Location.Text.GetBaseDir();
        gv.ExecutableName = "Godot";
#else
        gv.Location = _Location.Text.GetBaseDir();
        gv.ExecutableName = _Location.Text.GetFile();
#endif
        gv.IsMono = _MonoEnabled.Pressed;
        CentralStore.Versions.Add(gv);
        CentralStore.Instance.SaveDatabase();
        Visible = false;
        EmitSignal("added_custom_godot");
    }

    [SignalHandler("pressed", nameof(_CancelBtn))]
    void OnCancelPressed() {
        Visible = false;
    }
#endregion
}
