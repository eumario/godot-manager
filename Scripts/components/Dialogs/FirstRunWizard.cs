using System.Diagnostics.CodeAnalysis;
using System.Text;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;

[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class FirstRunWizard : ReferenceRect
{
    #region XDestkop String
    const string DESKTOP_ENTRY = @"[Desktop Entry]
Version=1.0
Type=Application
Name=Godot Manager
Icon={0}
Exec={1}
Comment=Godot Project and Engine Version Manager
Categories=Development;IDE;
Terminal=false
StartupWMClass=Godot-Manager
StartupNotify=true
";
    #endregion

    #region Signals

    [Signal]
    public delegate void wizard_completed();
    #endregion
    
    #region Node Paths

    // Main Wizard Controls
    [NodePath] private Button PrevStep = null;
    [NodePath] private Button Cancel = null;
    [NodePath] private Button NextStep = null;
    
    // Wizard Pages
    [NodePath] private Panel Page1 = null;
    [NodePath] private Panel Page2 = null;
    [NodePath] private Panel Page3 = null;
    [NodePath] private Panel Page4 = null;
    [NodePath] private Panel Page5 = null;
    
    // Wizard Control
    [NodePath] private TabContainer Wizard = null;
    
    // STEPS
    // Step 2 (All Platforms) - Setup Directories
    [NodePath] private LineEdit EngineLoc = null;
    [NodePath] private Button EngineBrowse = null;
    [NodePath] private Button EngineDefault = null;

    [NodePath] private LineEdit CacheLoc = null;
    [NodePath] private Button CacheBrowse = null;
    [NodePath] private Button CacheDefault = null;

    [NodePath] private LineEdit ProjectLoc = null;
    [NodePath] private Button ProjectBrowse = null;
    [NodePath] private Button ProjectDefault = null;
    
    // Step 3 (Linux Only) - Create Desktop Entry
    [NodePath] private CheckBox CreateShortcut = null;
    [NodePath] private CheckBox GlobalShortcut = null;
    
    // Step 4 (All Platforms) - Install Godot Engines
    [NodePath] private GodotPanel GodotPanel = null;
    #endregion

    private bool loaded_engines = false;

    private Array<string> OriginalSettings = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        if (Platform.OperatingSystem != "Linux (or BSD)")
        {
            Wizard.RemoveChild(Page3);
        }

        OriginalSettings = new Array<string>()
        {
            CentralStore.Settings.EnginePath.GetOSDir().NormalizePath(),
            CentralStore.Settings.CachePath.GetOSDir().NormalizePath(),
            CentralStore.Settings.ProjectPath.GetOSDir().NormalizePath()
        };
        EngineLoc.Text = CentralStore.Settings.EnginePath.GetOSDir().NormalizePath();
        CacheLoc.Text = CentralStore.Settings.CachePath.GetOSDir().NormalizePath();
        ProjectLoc.Text = CentralStore.Settings.ProjectPath.GetOSDir().NormalizePath();

        Wizard.CurrentTab = 0;
        PrevStep.Disabled = true;
    }

    public void ShowDialog() => Visible = true;
    public void HideDialog() => Visible = false;

    [SignalHandler("toggled", nameof(CreateShortcut))]
    void OnToggled_CreateShortcut(bool toggled)
    {
        GlobalShortcut.Disabled = !toggled;
    }

    // Default Buttons Handlers
    [SignalHandler("pressed", nameof(EngineDefault))]
    void OnPressed_EngineDefault() => EngineLoc.Text = "user://versions/".GetOSDir().NormalizePath();

    [SignalHandler("pressed", nameof(CacheDefault))]
    void OnPressed_CacheDefault() => CacheLoc.Text = "user://cache/".GetOSDir().NormalizePath();

    [SignalHandler("pressed", nameof(ProjectDefault))]
    void OnPressed_ProjectDefault() => ProjectLoc.Text = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects","").NormalizePath();
    
    // Browse Buttons Handlers
    [SignalHandler("pressed", nameof(EngineBrowse))]
    void OnPressed_EngineBrowse()
    {
        AppDialogs.BrowseFolderDialog.WindowTitle = "Location for Godot Engines";
        AppDialogs.BrowseFolderDialog.CurrentDir = EngineLoc.Text;
        if (!AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_EngineBrowse)))
            AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, nameof(OnDirSelected_EngineBrowse), new Array(), (int)ConnectFlags.Oneshot);
        AppDialogs.BrowseFolderDialog.PopupExclusive = true;
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }

    [SignalHandler("pressed", nameof(CacheBrowse))]
    void OnPressed_CacheBrowse()
    {
        AppDialogs.BrowseFolderDialog.WindowTitle = "Location for Cache Store";
        AppDialogs.BrowseFolderDialog.CurrentDir = CacheLoc.Text;
        if (!AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_CacheBrowse)))
            AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, nameof(OnDirSelected_CacheBrowse), new Array(), (int)ConnectFlags.Oneshot);
        AppDialogs.BrowseFolderDialog.PopupExclusive = true;
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }
    
    [SignalHandler("pressed", nameof(ProjectBrowse))]
    void OnPressed_ProjectBrowse()
    {
        AppDialogs.BrowseFolderDialog.WindowTitle = "Location for Projects";
        AppDialogs.BrowseFolderDialog.CurrentDir = ProjectLoc.Text;
        if (!AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_ProjectBrowse)))
            AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, nameof(OnDirSelected_ProjectBrowse), new Array(), (int)ConnectFlags.Oneshot);
        AppDialogs.BrowseFolderDialog.PopupExclusive = true;
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }

    // File Dialog Handlers
    void OnDirSelected_EngineBrowse(string dir) => EngineLoc.Text = dir.GetOSDir().Join("").NormalizePath();

    void OnDirSelected_CacheBrowse(string dir) => CacheLoc.Text = dir.GetOSDir().Join("").NormalizePath();

    void OnDirSelected_ProjectBrowse(string dir) => ProjectLoc.Text = dir.GetOSDir().Join("").NormalizePath();

    // Navigation buttons Handlers
    [SignalHandler("pressed", nameof(PrevStep))]
    void OnPressed_PrevStep()
    {
        if (Wizard.CurrentTab == 1)
            PrevStep.Disabled = true;
        Wizard.CurrentTab--;
        if (NextStep.Text == "Finished")
            NextStep.Text = "Next";
    }

    [SignalHandler("pressed", nameof(NextStep))]
    async void OnPressed_NextStep()
    {
        if (NextStep.Text == "Finished")
        {
            UpdateSettings(true);
            EmitSignal("wizard_completed");
            Visible = false;
        }
        if (Wizard.CurrentTab <= Wizard.GetTabCount())
            Wizard.CurrentTab++;
        if (Wizard.CurrentTab == Wizard.GetTabCount() - 1)
            NextStep.Text = "Finished";

        if (Wizard.GetCurrentTabControl() == Page4)
        {
            if (!loaded_engines)
            {
                CentralStore.GHVersions.Clear();
                foreach (int id in CentralStore.MRVersions.Keys)
                    CentralStore.MRVersions[id].Clear();
                UpdateSettings();
                await GodotPanel.GatherReleases();
                await GodotPanel.PopulateList();
                loaded_engines = true;
            }
        }
        PrevStep.Disabled = false;
    }

    [SignalHandler("pressed", nameof(Cancel))]
    async void OnPressed_Cancel()
    {
        var res = await AppDialogs.YesNoDialog.ShowDialog("First Run Wizard", 
            "Are you sure you want to cancel this first run wizard? " + 
            " Any settings you have changed, will be lost.");
        if (res)
        { // System.IO.Compression.FileSystem
            CentralStore.Settings.EnginePath = OriginalSettings[0];
            CentralStore.Settings.CachePath = OriginalSettings[1];
            CentralStore.Settings.ProjectPath = OriginalSettings[2];
            CentralStore.Settings.ScanDirs = new Array<string>() { OriginalSettings[2] };
            CentralStore.Instance.SaveDatabase();
            HideDialog();
        }
    }
    
    // Save our Settings:
    private void UpdateSettings(bool finished = false)
    {
        CentralStore.Settings.CachePath = CacheLoc.Text;
        CentralStore.Settings.EnginePath = EngineLoc.Text;
        CentralStore.Settings.ProjectPath = ProjectLoc.Text;
        CentralStore.Settings.ScanDirs = new Array<string>() { ProjectLoc.Text };
        #if GODOT_X11 || GODOT_LINUXBSD
        if (finished)
        {
            if (CreateShortcut.Pressed)
            {
                Array<string> dirsToMake = new Array<string>();
                string shortcutPath = OS.GetEnvironment("HOME").Join(".local","share","applications","godot-manager.desktop").NormalizePath();
                string iconPath = OS.GetEnvironment("HOME").Join(".local", "share", "icons", "hicolor", "64x64", "apps", "godot-manager.png");
                string executablePath = OS.GetExecutablePath();
                
                bool needRoot = false;
                if (GlobalShortcut.Pressed)
                {
                    shortcutPath = "/usr/local/share/applications/godot-manager.desktop".NormalizePath();
                    iconPath = "/usr/local/share/icons/hicolor/64x64/apps/godot-manager.png".NormalizePath();
                    needRoot = true;
                }
                
                if (!System.IO.Directory.Exists(shortcutPath.GetBaseDir())) dirsToMake.Add(shortcutPath.GetBaseDir());
                if (!System.IO.Directory.Exists(iconPath.GetBaseDir())) dirsToMake.Add(iconPath.GetBaseDir());
                
                // Save Icon
                var res = GD.Load<StreamTexture>("res://icon.png");
                var data = res.GetData();
                data.SavePng("/tmp/godot-manager.png");
                var body = string.Format(DESKTOP_ENTRY, iconPath, executablePath);
                System.IO.File.WriteAllText("/tmp/godot-manager.desktop", body, Encoding.UTF8);

                if (needRoot)
                {
                    using (var fh = new File())
                    {
                        fh.Open("/tmp/installer.sh", File.ModeFlags.Write);
                        fh.StoreString("#!/bin/bash\n\n");
                        foreach(var dir in dirsToMake) fh.StoreString($"mkdir -p {dir}\n");
                        fh.StoreString($"cp /tmp/godot-manager.png {iconPath}\n");
                        fh.StoreString($"cp /tmp/godot-manager.desktop {shortcutPath}\n");
                        fh.Close();
                        Util.Chmod("/tmp/installer.sh", 0755);
                        var execre = Util.PkExec("/tmp/installer.sh");
                        System.IO.File.Delete("/tmp/installer.sh");
                    }
                }
                else
                {
                    foreach (var dir in dirsToMake) System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.Copy("/tmp/godot-manager.png", iconPath);
                    System.IO.File.Copy("/tmp/godot-manager.desktop", shortcutPath);
                }
                System.IO.File.Delete("/tmp/godot-manager.png");
                System.IO.File.Delete("/tmp/godot-manager.desktop");
            }
        }
        #endif
        CentralStore.Instance.SaveDatabase();
    }
}
