using System.Diagnostics.CodeAnalysis;
using System.Text;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using Directory = System.IO.Directory;
using File = Godot.File;

[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class FirstRunWizard : ReferenceRect
{
    #region XDestkop String

    public const string DESKTOP_ENTRY = @"[Desktop Entry]
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

    string GetEngineDefaultPath() => "user://versions/".GetOSDir().NormalizePath();
    string GetCacheDefaultPath() => "user://cache/".GetOSDir().NormalizePath();

    string GetProjectDefaultPath() => OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects", "").NormalizePath();

    // Default Buttons Handlers
    [SignalHandler("pressed", nameof(EngineDefault))]
    void OnPressed_EngineDefault()
    {
        OriginalSettings[0] = EngineLoc.Text;
        EngineLoc.Text = GetEngineDefaultPath();
        CentralStore.Settings.EnginePath = GetEngineDefaultPath();
    }

    [SignalHandler("pressed", nameof(CacheDefault))]
    void OnPressed_CacheDefault()
    {
        OriginalSettings[1] = CacheLoc.Text;
        CacheLoc.Text = GetCacheDefaultPath();
        CentralStore.Settings.CachePath = CacheLoc.Text;
    }

    [SignalHandler("pressed", nameof(ProjectDefault))]
    void OnPressed_ProjectDefault()
    {
        OriginalSettings[2] = ProjectLoc.Text;
        ProjectLoc.Text = GetProjectDefaultPath();
        CentralStore.Settings.ProjectPath = ProjectLoc.Text;
    }

    // Browse Buttons Handlers
    [SignalHandler("pressed", nameof(EngineBrowse))]
    void OnPressed_EngineBrowse()
    {
        AppDialogs.BrowseFolderDialog.WindowTitle = Tr("Location for Godot Engines");
        AppDialogs.BrowseFolderDialog.CurrentDir = EngineLoc.Text;
        if (!AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_EngineBrowse)))
        {
            AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, nameof(OnDirSelected_EngineBrowse), null,
                (int)ConnectFlags.Oneshot);
            AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, nameof(OnPopupHide_EngineBrowse), null,
                (int)ConnectFlags.Oneshot);
        }

        AppDialogs.BrowseFolderDialog.PopupExclusive = true;
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }

    [SignalHandler("pressed", nameof(CacheBrowse))]
    void OnPressed_CacheBrowse()
    {
        AppDialogs.BrowseFolderDialog.WindowTitle = Tr("Location for Cache Store");
        AppDialogs.BrowseFolderDialog.CurrentDir = CacheLoc.Text;
        if (!AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_CacheBrowse)))
        {
            AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, nameof(OnDirSelected_CacheBrowse), null,
                    (int)ConnectFlags.Oneshot);
            AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, nameof(OnPopupHide_CacheBrowse), null,
                    (int)ConnectFlags.Oneshot);
        }

        AppDialogs.BrowseFolderDialog.PopupExclusive = true;
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }

    [SignalHandler("pressed", nameof(ProjectBrowse))]
    void OnPressed_ProjectBrowse()
    {
        AppDialogs.BrowseFolderDialog.WindowTitle = Tr("Location for Projects");
        AppDialogs.BrowseFolderDialog.CurrentDir = ProjectLoc.Text;
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, nameof(OnDirSelected_ProjectBrowse), null,
            (int)ConnectFlags.Oneshot);
        AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, nameof(OnPopupHide_ProjectBrowse), null,
            (int)ConnectFlags.Oneshot);

        AppDialogs.BrowseFolderDialog.PopupExclusive = true;
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }
    

    // File Dialog Handlers
    void OnDirSelected_EngineBrowse(string dir)
    {
        OriginalSettings[0] = EngineLoc.Text;
        EngineLoc.Text = dir.GetOSDir().Join("").NormalizePath();
        CentralStore.Settings.EnginePath = EngineLoc.Text;
        EnsureDirectoryExists(EngineLoc.Text);
    }


    void OnDirSelected_CacheBrowse(string dir)
    {
        OriginalSettings[1] = CacheLoc.Text;
        CacheLoc.Text = dir.GetOSDir().Join("").NormalizePath();
        CentralStore.Settings.CachePath = CacheLoc.Text;
        EnsureDirectoryExists(EngineLoc.Text);
    }

    void OnDirSelected_ProjectBrowse(string dir)
    {
        OriginalSettings[2] = ProjectLoc.Text;
        ProjectLoc.Text = dir.GetOSDir().Join("").NormalizePath();
        CentralStore.Settings.ProjectPath = ProjectLoc.Text;
        EnsureDirectoryExists(EngineLoc.Text);
    }

    void OnPopupHide_EngineBrowse()
    {
        if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_EngineBrowse)))
            AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, nameof(OnDirSelected_EngineBrowse));
    }
    
    void OnPopupHide_CacheBrowse()
    {
        if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_CacheBrowse)))
            AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, nameof(OnDirSelected_CacheBrowse));
    }
    
    void OnPopupHide_ProjectBrowse()
    {
        if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, nameof(OnDirSelected_ProjectBrowse)))
            AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, nameof(OnDirSelected_ProjectBrowse));
    }

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
        var res = await AppDialogs.YesNoDialog.ShowDialog(Tr("First Run Wizard"),
            Tr("Are you sure you want to cancel this first run wizard? " +
               " Any settings you have changed, will be lost."));
        if (res)
        {
            // System.IO.Compression.FileSystem
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
        CentralStore.Settings.ScanDirs = new Array<string>() { ProjectLoc.Text };
#if GODOT_X11 || GODOT_LINUXBSD
        if (finished && CreateShortcut.Pressed) CreateShortcuts();
#endif
        CentralStore.Instance.SaveDatabase();
    }

    void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

#if GODOT_X11 || GODOT_LINUXBSD
    void CreateShortcuts()
    {
        string iconPath = OS.GetExecutablePath().GetBaseDir().Join("godot-manager.svg");
        string executablePath = OS.GetExecutablePath();

        using (var fh = new File())
        {
            var err = fh.Open("res://godot-manager.dat", File.ModeFlags.Read);
            var size = fh.GetLen();
            var svg = fh.GetBuffer((long)size);
            fh.Close();
            System.IO.File.WriteAllBytes(iconPath, svg);
        }
        
        bool needRoot = false;
        if (GlobalShortcut.Pressed)
        {
            iconPath = "/opt/GodotManager/godot-manager.svg";
            needRoot = true;
        }
        
        var body = string.Format(DESKTOP_ENTRY, iconPath, executablePath);
        System.IO.File.WriteAllText("/tmp/godot-manager.desktop", body, Encoding.ASCII);
        
        if (needRoot)
        {
            bool needToCopy = false;
            if (!executablePath.StartsWith($"/opt/GodotManager"))
            {
                executablePath = "/opt/GodotManager/GodotManager.x86_64";
                body = string.Format(DESKTOP_ENTRY, iconPath, executablePath);
                System.IO.File.WriteAllText("/tmp/godot-manager.desktop", body, Encoding.ASCII);
                needToCopy = true;
            }

            using (var fh = new File())
            {
                fh.Open("/tmp/installer.sh", File.ModeFlags.Write);
                fh.StoreString("#!/bin/bash\n\n");
                if (needToCopy)
                {
                    if (System.IO.Directory.Exists("/opt/GodotManager")) fh.StoreString("rm -rf /opt/GodotManager\n");
                    fh.StoreString("mkdir -p /opt/GodotManager\n");
                    fh.StoreString($"cp -r {OS.GetExecutablePath().GetBaseDir()}/* /opt/GodotManager/\n");
                }
                fh.StoreString("xdg-desktop-menu install --mode system /tmp/godot-manager.desktop\n");
                fh.StoreString("xdg-desktop-menu install forceupdate --mode system\n");

                fh.Close();
                Util.Chmod("/tmp/installer.sh", 0755);
                var execre = Util.PkExec("/tmp/installer.sh", Tr("Install Shortcut"),
                    Tr("Godot Manager needs Administrative privileges to complete the requested actions."));
                System.IO.File.Delete("/tmp/installer.sh");
            }
        }
        else
        {
            Util.XdgDesktopInstall("/tmp/godot-manager.desktop");
            Util.XdgDesktopUpdate();
        }
        System.IO.File.Delete("/tmp/godot-manager.desktop");
        CentralStore.Settings.ShortcutMade = true;
        CentralStore.Settings.ShortcutRoot = needRoot;
    }
    #endif
}
