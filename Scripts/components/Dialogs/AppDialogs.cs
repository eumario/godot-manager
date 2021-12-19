using Godot;
using Godot.Collections;

public class AppDialogs : Control
{
#region Node Paths
    public FirstTimeInstall FirstTimeInstall = null;
    public AddCustomGodot AddCustomGodot = null;
    public BusyDialog BusyDialog = null;
    public NewVersion NewVersion = null;
    public YesNoDialog YesNoDialog = null;
    public ImportProject ImportProject = null;
    public MessageDialog MessageDialog = null;
    public FileDialog ImportFileDialog = null;
    public CreateProject CreateProject = null;
#endregion

    private static AppDialogs _instance = null;

    public static AppDialogs Instance {
        get {
            if (_instance == null)
                _instance = new AppDialogs();
            
            return _instance;
        }
    }

    protected AppDialogs() {

        // Initialize Dialogs
        FirstTimeInstall = GD.Load<PackedScene>("res://components/Dialogs/FirstTimeInstall.tscn").Instance<FirstTimeInstall>();
        AddCustomGodot = GD.Load<PackedScene>("res://components/Dialogs/AddCustomGodot.tscn").Instance<AddCustomGodot>();
        BusyDialog = GD.Load<PackedScene>("res://components/Dialogs/BusyDialog.tscn").Instance<BusyDialog>();
        NewVersion = GD.Load<PackedScene>("res://components/Dialogs/NewVersion.tscn").Instance<NewVersion>();
        YesNoDialog = GD.Load<PackedScene>("res://components/Dialogs/YesNoDialog.tscn").Instance<YesNoDialog>();
        ImportProject = GD.Load<PackedScene>("res://components/Dialogs/ImportProject.tscn").Instance<ImportProject>();
        MessageDialog = GD.Load<PackedScene>("res://components/Dialogs/MessageDialog.tscn").Instance<MessageDialog>();
        CreateProject = GD.Load<PackedScene>("res://components/Dialogs/CreateProject.tscn").Instance<CreateProject>();
        ImportFileDialog = new FileDialog();
        ImportFileDialog.Mode = FileDialog.ModeEnum.OpenFile;
        ImportFileDialog.Access = FileDialog.AccessEnum.Filesystem;
        ImportFileDialog.WindowTitle = "Open Godot Project...";
        ImportFileDialog.Filters = new string[] {"*.godot"};
        ImportFileDialog.RectMinSize = new Vector2(510, 390);

        // Setup Full Rect for dialogs:
        foreach(ReferenceRect dlg in new Array<ReferenceRect> {
                FirstTimeInstall, AddCustomGodot, BusyDialog,
                NewVersion, YesNoDialog, ImportProject,
                MessageDialog, CreateProject 
            }) {
            dlg.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
            dlg.Visible = false;
        }

        MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public override void _EnterTree() {
        AddChild(FirstTimeInstall);
        AddChild(AddCustomGodot);
        AddChild(BusyDialog);
        AddChild(NewVersion);
        AddChild(YesNoDialog);
        AddChild(ImportProject);
        AddChild(MessageDialog);
        AddChild(ImportFileDialog);
        AddChild(CreateProject);
    }
}
