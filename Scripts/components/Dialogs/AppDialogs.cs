using Godot;
using Godot.Collections;

public class AppDialogs : Control
{
#region Node Paths
    public FirstTimeInstall FirstTimeInstall_ = null;
    public AddCustomGodot AddCustomGodot_ = null;
    public BusyDialog BusyDialog_ = null;
    public NewVersion NewVersion_ = null;
    public YesNoDialog YesNoDialog_ = null;
    public ImportProject ImportProject_ = null;
    public MessageDialog MessageDialog_ = null;
    public FileDialog ImportFileDialog_ = null;
    public CreateProject CreateProject_ = null;
    public AssetLibPreview AssetLibPreview_ = null;
    public DownloadAddon DownloadAddon_ = null;
#endregion

#region Singleton Variables to access in program
    public static FirstTimeInstall FirstTimeInstall { get => Instance.FirstTimeInstall_; }
    public static AddCustomGodot AddCustomGodot { get => Instance.AddCustomGodot_; }
    public static BusyDialog BusyDialog { get => Instance.BusyDialog_; }
    public static NewVersion NewVersion { get => Instance.NewVersion_; }
    public static YesNoDialog YesNoDialog { get => Instance.YesNoDialog_; }
    public static ImportProject ImportProject { get => Instance.ImportProject_; }
    public static MessageDialog MessageDialog { get => Instance.MessageDialog_; }
    public static FileDialog ImportFileDialog { get => Instance.ImportFileDialog_; }
    public static CreateProject CreateProject { get => Instance.CreateProject_; }
    public static AssetLibPreview AssetLibPreview { get => Instance.AssetLibPreview_; }
    public static DownloadAddon DownloadAddon { get => Instance.DownloadAddon_; }
#endregion

    private static AppDialogs _instance = null;

    public static AppDialogs Instance {
        get {
            if (_instance == null)
                _instance = new AppDialogs();
            
            return _instance;
        }
    }

    private Array<ReferenceRect> dialogs;

    protected AppDialogs() {

        // Initialize Dialogs
        FirstTimeInstall_ = GD.Load<PackedScene>("res://components/Dialogs/FirstTimeInstall.tscn").Instance<FirstTimeInstall>();
        AddCustomGodot_ = GD.Load<PackedScene>("res://components/Dialogs/AddCustomGodot.tscn").Instance<AddCustomGodot>();
        BusyDialog_ = GD.Load<PackedScene>("res://components/Dialogs/BusyDialog.tscn").Instance<BusyDialog>();
        NewVersion_ = GD.Load<PackedScene>("res://components/Dialogs/NewVersion.tscn").Instance<NewVersion>();
        YesNoDialog_ = GD.Load<PackedScene>("res://components/Dialogs/YesNoDialog.tscn").Instance<YesNoDialog>();
        ImportProject_ = GD.Load<PackedScene>("res://components/Dialogs/ImportProject.tscn").Instance<ImportProject>();
        MessageDialog_ = GD.Load<PackedScene>("res://components/Dialogs/MessageDialog.tscn").Instance<MessageDialog>();
        CreateProject_ = GD.Load<PackedScene>("res://components/Dialogs/CreateProject.tscn").Instance<CreateProject>();
        AssetLibPreview_ = GD.Load<PackedScene>("res://components/Dialogs/AssetLibPreview.tscn").Instance<AssetLibPreview>();
        DownloadAddon_ = GD.Load<PackedScene>("res://components/Dialogs/DownloadAddon.tscn").Instance<DownloadAddon>();
        ImportFileDialog_ = new FileDialog();
        ImportFileDialog_.Name = "ImportFileDialog";
        ImportFileDialog_.Mode = FileDialog.ModeEnum.OpenFile;
        ImportFileDialog_.Access = FileDialog.AccessEnum.Filesystem;
        ImportFileDialog_.WindowTitle = "Open Godot Project...";
        ImportFileDialog_.Filters = new string[] {"*.godot"};
        ImportFileDialog_.RectMinSize = new Vector2(510, 390);

        dialogs = new Array<ReferenceRect> {    // Hierarchy of Dialogs in window, for proper displaying
            FirstTimeInstall_,                  // First Time Installation Helper
            AddCustomGodot_, NewVersion_,       // Add Custom Godot / New Godot Version Prompt
            CreateProject_, ImportProject_,     // Create Project / Import Project
            AssetLibPreview_, DownloadAddon_,   // Asset Library Preview / Download Addon/Project
            YesNoDialog_,                       // Yes No Prompt
            BusyDialog_,                        // Busy Dialog
            MessageDialog_,                     // Message Dialog
        };

        MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public override void _EnterTree() {
        // Setup Full Rect for dialogs:
        foreach(ReferenceRect dlg in dialogs ) {
            dlg.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
            dlg.Visible = false;
            AddChild(dlg);
        }
    }
}
