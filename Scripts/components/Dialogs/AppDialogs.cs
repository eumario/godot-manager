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
        FirstTimeInstall_ = GD.Load<PackedScene>("res://components/Dialogs/FirstTimeInstall.tscn").Instance<FirstTimeInstall>();
        AddCustomGodot_ = GD.Load<PackedScene>("res://components/Dialogs/AddCustomGodot.tscn").Instance<AddCustomGodot>();
        BusyDialog_ = GD.Load<PackedScene>("res://components/Dialogs/BusyDialog.tscn").Instance<BusyDialog>();
        NewVersion_ = GD.Load<PackedScene>("res://components/Dialogs/NewVersion.tscn").Instance<NewVersion>();
        YesNoDialog_ = GD.Load<PackedScene>("res://components/Dialogs/YesNoDialog.tscn").Instance<YesNoDialog>();
        ImportProject_ = GD.Load<PackedScene>("res://components/Dialogs/ImportProject.tscn").Instance<ImportProject>();
        MessageDialog_ = GD.Load<PackedScene>("res://components/Dialogs/MessageDialog.tscn").Instance<MessageDialog>();
        CreateProject_ = GD.Load<PackedScene>("res://components/Dialogs/CreateProject.tscn").Instance<CreateProject>();
        AssetLibPreview_ = GD.Load<PackedScene>("res://components/Dialogs/AssetLibPreview.tscn").Instance<AssetLibPreview>();
        ImportFileDialog_ = new FileDialog();
        ImportFileDialog_.Mode = FileDialog.ModeEnum.OpenFile;
        ImportFileDialog_.Access = FileDialog.AccessEnum.Filesystem;
        ImportFileDialog_.WindowTitle = "Open Godot Project...";
        ImportFileDialog_.Filters = new string[] {"*.godot"};
        ImportFileDialog_.RectMinSize = new Vector2(510, 390);

        // Setup Full Rect for dialogs:
        foreach(ReferenceRect dlg in new Array<ReferenceRect> {
                FirstTimeInstall_, AddCustomGodot_, BusyDialog_,
                NewVersion_, YesNoDialog_, ImportProject_,
                MessageDialog_, CreateProject_, AssetLibPreview_ 
            }) {
            dlg.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
            dlg.Visible = false;
        }

        MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public override void _EnterTree() {
        AddChild(FirstTimeInstall_);
        AddChild(AddCustomGodot_);
        AddChild(BusyDialog_);
        AddChild(NewVersion_);
        AddChild(YesNoDialog_);
        AddChild(ImportProject_);
        AddChild(MessageDialog_);
        AddChild(ImportFileDialog_);
        AddChild(CreateProject_);
        AddChild(AssetLibPreview_);
    }
}
