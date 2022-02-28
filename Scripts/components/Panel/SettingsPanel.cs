using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using ActionStack = System.Collections.Generic.Stack<System.Action>;
using Uri = System.Uri;
using TimeSpan = System.TimeSpan;

public class SettingsPanel : Panel
{
    #region Node Paths
    
    #region Page Buttons
    [NodePath("VB/Header/HC/PC/HC/General")]
    Button _generalBtn = null;

    [NodePath("VB/Header/HC/PC/HC/Projects")]
    Button _projectsBtn = null;

    [NodePath("VB/Header/HC/PC/HC/About")]
    Button _aboutBtn = null;

    [NodePath("VB/Header/HC/PC/HC/Licenses")]
    Button _licensesBtn = null;
    #endregion

    #region Main Controls
    [NodePath("VB/MC/TC")]
    TabContainer _pages = null;

    [NodePath("VB/Header/HC/ActionButtons")]
    ActionButtons _actionButtons = null;
    #endregion

    #region General Page
    [NodePath("VB/MC/TC/General/GC/HBIL/GodotInstallLocation")]
    LineEdit _godotInstallLocation = null;

    [NodePath("VB/MC/TC/General/GC/HBIL/Browse")]
    Button _godotBrowseButton = null;

    [NodePath("VB/MC/TC/General/GC/HBCL/CacheInstallLocation")]
    LineEdit _cacheInstallLocation = null;

    [NodePath("VB/MC/TC/General/GC/HBCL/Browse")]
    Button _cacheBrowseButton = null;

    [NodePath("VB/MC/TC/General/GC/ProjectView")]
    OptionButton _defaultProjectView = null;

    [NodePath("VB/MC/TC/General/GC/GodotDefault")]
    OptionButton _defaultEngine = null;

    [NodePath("VB/MC/TC/General/GC/CheckForUpdates")]
    CheckBox _checkForUpdates = null;

    [NodePath("VB/MC/TC/General/GC/HBCI/UpdateCheckInterval")]
    OptionButton _updateCheckInterval = null;

    [NodePath("VB/MC/TC/General/GC/VBLO/NoConsole")]
    CheckBox _noConsole = null;

    [NodePath("VB/MC/TC/General/GC/VBLO/EditorProfiles")]
    CheckBox _editorProfiles = null;

    [NodePath("VB/MC/TC/General/GC/MirrorTabs/Asset Library")]
    ItemListWithButtons _assetMirror = null;

    [NodePath("VB/MC/TC/General/GC/MirrorTabs/Godot Engine")]
    ItemListWithButtons _godotMirror = null;
    #endregion

    #region Projects Page
    [NodePath("VB/MC/TC/Projects/GC/HBPL/DefaultProjectLocation")]
    LineEdit _defaultProjectLocation = null;

    [NodePath("VB/MC/TC/Projects/GC/HBPL/BrowseProjectLocation")]
    Button _browseProjectLocation = null;

    [NodePath("VB/MC/TC/Projects/GC/ExitManager")]
    CheckBox _exitGodotManager = null;

    [NodePath("VB/MC/TC/Projects/GC/DirectoryScan")]
    ItemListWithButtons _directoryScan = null;
    #endregion

    #region About Page
    [NodePath("VB/MC/TC/About/MC/VB/HBHeader/VB/EmailWebsite")]
    RichTextLabel _emailWebsite = null;

    [NodePath("VB/MC/TC/About/MC/VB/MC/BuiltWith")]
    RichTextLabel _builtWith = null;

    [NodePath("VB/MC/TC/About/MC/VB/MC2/SpecialThanks")]
    RichTextLabel _specialThanks = null;

    [NodePath("VB/MC/TC/About/MC/VB/CenterContainer/HB/BuyMe")]
    TextureRect _buyMe = null;
    #endregion
    
    #region Licenses Page
    [NodePath("VB/MC/TC/Licenses")]
    TabContainer _licenses = null;

    [NodePath("VB/MC/TC/Licenses/MIT License")]
    RichTextLabel _mitLicense = null;

    [NodePath("VB/MC/TC/Licenses/Apache License")]
    RichTextLabel _apacheLicense = null;
    #endregion

    #endregion

    #region Private Variables
    Array<string> _views;
    // In Hours
    double[] _dCheckInterval = new double[] {
        1,      // 1 Hour
        12,     // 12 hours
        24,     // 1 day
        168,    // 1 week
        336,    // Bi-Weekly
        720     // Monthly (30 Days)
    };
    bool bPInternal = false;
    ActionStack _undoActions;
    #endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        _undoActions = new ActionStack();
        _views = new Array<string>();

        for (int i = 0; i < _defaultProjectView.GetItemCount(); i++) {
            _views.Add(_defaultProjectView.GetItemText(i));
        };

        // Load up our Settings to be displayed to end user
        LoadSettings();
        updateActionButtons();

        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");

        _updateCheckInterval.Disabled = !_checkForUpdates.Pressed;
    }

    void updateActionButtons() {
        _actionButtons.Visible = _undoActions.Count > 0;
    }

#region Internal Functions for use in the Settings Page
    int GetIntervalIndex() {
        // _dCheckInterval.(CentralStore.Settings.CheckInterval.TotalHours)
        for (int i = 0; i < _dCheckInterval.Length; i++) {
            if (_dCheckInterval[i] == CentralStore.Settings.CheckInterval.TotalHours)
                return i;
        }
        return -1;
    }

    void LoadSettings() {
        // General Page
        bPInternal = true;
        _godotInstallLocation.Text = CentralStore.Settings.EnginePath.GetOSDir().NormalizePath();
        _cacheInstallLocation.Text = CentralStore.Settings.CachePath.GetOSDir().NormalizePath();
        _defaultProjectView.Select(_views.IndexOf(CentralStore.Settings.DefaultView));
        PopulateGodotEngine();
        _checkForUpdates.Pressed = CentralStore.Settings.CheckForUpdates;
        _updateCheckInterval.Select(GetIntervalIndex());
        _editorProfiles.Pressed = CentralStore.Settings.SelfContainedEditors;
        _noConsole.Pressed = CentralStore.Settings.NoConsole;

        _assetMirror.Clear();
        foreach (string meta in _assetMirror.GetMetaList())
            _assetMirror.RemoveMeta(meta);
        
        foreach (Dictionary<string, string> mirror in CentralStore.Settings.AssetMirrors) {
            _assetMirror.AddItem(mirror["name"]);
            _assetMirror.SetMeta(mirror["name"], mirror["url"]);
        }
        
        _godotMirror.Clear();
        foreach (string meta in _godotMirror.GetMetaList())
            _godotMirror.RemoveMeta(meta);
        
        foreach (Dictionary<string, string> mirror in CentralStore.Settings.EngineMirrors) {
            _godotMirror.AddItem(mirror["name"]);
            _godotMirror.SetMeta(mirror["name"], mirror["url"]);
        }

        // Project Page
        _defaultProjectLocation.Text = CentralStore.Settings.ProjectPath.NormalizePath();
        _exitGodotManager.Pressed = CentralStore.Settings.CloseManagerOnEdit;
        _directoryScan.Clear();
        foreach (string dir in CentralStore.Settings.ScanDirs) {
            _directoryScan.AddItem(dir.NormalizePath());
        }
        bPInternal = false;
    }

    void UpdateSettings() {
        CentralStore.Settings.EnginePath = _godotInstallLocation.Text.GetOSDir().NormalizePath();
        Error result;
        if (CentralStore.Settings.CachePath != _cacheInstallLocation.Text.GetOSDir().NormalizePath()) {
            Directory dir = new Directory();
            dir.Open(_cacheInstallLocation.Text.GetOSDir().NormalizePath());
            if (!dir.DirExists("AssetLib"))
                result = dir.MakeDir("AssetLib");
            if (!dir.DirExists("Godot"))
                result = dir.MakeDir("Godot");
            if (!dir.DirExists("images"))
                result = dir.MakeDir("images");
        }
        CentralStore.Settings.CachePath = _cacheInstallLocation.Text.GetOSDir().NormalizePath();
        CentralStore.Settings.DefaultView = _defaultProjectView.GetItemText(_defaultProjectView.Selected);
        CentralStore.Settings.DefaultEngine = (string)_defaultEngine.GetItemMetadata(_defaultEngine.Selected);
        CentralStore.Settings.CheckForUpdates = _checkForUpdates.Pressed;
        CentralStore.Settings.CheckInterval = System.TimeSpan.FromHours(_dCheckInterval[_updateCheckInterval.Selected]);
        CentralStore.Settings.SelfContainedEditors = _editorProfiles.Pressed;
        CentralStore.Settings.NoConsole = _noConsole.Pressed;
        CentralStore.Settings.AssetMirrors.Clear();
        for (int i = 0; i < _assetMirror.GetItemCount(); i++) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["name"] = _assetMirror.GetItemText(i);
            data["url"] = (string)_assetMirror.GetMeta(data["name"]);
            CentralStore.Settings.AssetMirrors.Add(data);
        }
        CentralStore.Settings.EngineMirrors.Clear();
        for (int i = 0; i < _godotMirror.GetItemCount(); i++) {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["name"] = _godotMirror.GetItemText(i);
            data["url"] = (string)_godotMirror.GetMeta(data["name"]);
            CentralStore.Settings.EngineMirrors.Add(data);
        }
        CentralStore.Settings.ProjectPath = _defaultProjectLocation.Text.GetOSDir().NormalizePath();
        CentralStore.Settings.CloseManagerOnEdit = _exitGodotManager.Pressed;
        CentralStore.Settings.ScanDirs.Clear();
        for (int i = 0; i < _directoryScan.GetItemCount(); i++) {
            CentralStore.Settings.ScanDirs.Add(_directoryScan.GetItemText(i));
        }
        CentralStore.Instance.SaveDatabase();
        _undoActions.Clear();
        updateActionButtons(); 
    }

    void PopulateGodotEngine() {
        int defaultGodot = -1;
        _defaultEngine.Clear();
        foreach(GodotVersion version in CentralStore.Versions) {
            string gdName = version.GetDisplayName();
            int indx = CentralStore.Versions.IndexOf(version);
            if (version.Id == (string)CentralStore.Settings.DefaultEngine) {
                defaultGodot = indx;
                //gdName += " (Default)";
            }
            _defaultEngine.AddItem(gdName, indx);
            _defaultEngine.SetItemMetadata(indx, version.Id);
        }
        if (defaultGodot != -1)
            _defaultEngine.Select(defaultGodot);
        
    }
#endregion

#region Event Handlers for Notebook
    async void OnPageChanged(int page) {
        if (page == 3) {
            LoadSettings();
        } else {
            if (_undoActions.Count > 0) {
                var res = AppDialogs.YesNoDialog.ShowDialog("Unsaved Settings", "You have unsaved settings, do you wish to save your settings?");
                await res;
                if (res.Result)
                    UpdateSettings();
                else {
                    bPInternal = true;
                    while (_undoActions.Count > 0) {
                        _undoActions.Pop().Invoke();
                    }
                    updateActionButtons();
                    bPInternal = false;
                }
            }
        }
    }

    [SignalHandler("pressed", nameof(_generalBtn))]
    void OnGeneralPressed() {
        _generalBtn.Pressed = true;
        _projectsBtn.Pressed = false;
        _aboutBtn.Pressed = false;
        _licensesBtn.Pressed = false;
        _pages.CurrentTab = 0;
    }

    [SignalHandler("pressed", nameof(_projectsBtn))]
    void OnProjectsPressed() {
        _generalBtn.Pressed = false;
        _projectsBtn.Pressed = true;
        _aboutBtn.Pressed = false;
        _licensesBtn.Pressed = false;
        _pages.CurrentTab = 1;
    }

    [SignalHandler("pressed", nameof(_aboutBtn))]
    void OnAboutPressed() {
        _generalBtn.Pressed = false;
        _projectsBtn.Pressed = false;
        _aboutBtn.Pressed = true;
        _licensesBtn.Pressed = false;
        _pages.CurrentTab = 2;
    }

    [SignalHandler("pressed", nameof(_licensesBtn))]
    void OnLicensesPressed() {
        _generalBtn.Pressed = false;
        _projectsBtn.Pressed = false;
        _aboutBtn.Pressed = false;
        _licensesBtn.Pressed = true;
        _pages.CurrentTab = 3;
    }
#endregion

#region Event Handlers for Action Buttons
    [SignalHandler("clicked", nameof(_actionButtons))]
    void OnActionButtonsClicked(int index) {
        switch(index) {
            case 0:
                UpdateSettings();
                updateActionButtons();
                break;
            case 1:
                bPInternal = true;
                while (_undoActions.Count > 0) {
                    _undoActions.Pop().Invoke();
                }
                updateActionButtons();
                bPInternal = false;
                break;
        }
    }
#endregion

#region Event Handlers for General Page
    [SignalHandler("text_changed", nameof(_godotInstallLocation))]
    void OnGodotInstallLocation() {
        string oldVal = CentralStore.Settings.EnginePath;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.EnginePath = oldVal;
                _godotInstallLocation.Text = oldVal.GetOSDir().NormalizePath();
            });
            updateActionButtons();
        }
        CentralStore.Settings.EnginePath = _godotInstallLocation.Text;
    }

    [SignalHandler("pressed", nameof(_godotBrowseButton))]
    void OnGodotBrowse() {
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnBrowseGodot_DirSelected");
        AppDialogs.BrowseFolderDialog.WindowTitle = "Browse for Godot Install Folder...";
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }

    void OnBrowseGodot_DirSelected(string dir_name) {
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnBrowseGodot_DirSelected");
        _godotInstallLocation.Text = dir_name.GetOSDir().NormalizePath();
        OnGodotInstallLocation();
    }

    [SignalHandler("text_changed", nameof(_cacheInstallLocation))]
    void OnCacheInstallLocation() {
        string oldVal = CentralStore.Settings.CachePath;
        if (!bPInternal) {
            _undoActions.Push(() => {
                _cacheInstallLocation.Text = oldVal.GetOSDir().NormalizePath();
            });
            updateActionButtons();
        }
        _cacheInstallLocation.Text = _cacheInstallLocation.Text.GetOSDir().NormalizePath();
    }

    [SignalHandler("pressed", nameof(_cacheBrowseButton))]
    void OnBrowseCacheLocation() {
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnBrowseCache_DirSelected");
        AppDialogs.BrowseFolderDialog.WindowTitle = "Browse for Cache Folder...";
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }

    void OnBrowseCache_DirSelected(string dir_name) {
        _cacheInstallLocation.Text = dir_name.GetOSDir().NormalizePath();
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnBrowseCache_DirSelected");
        OnCacheInstallLocation();
    }

    [SignalHandler("item_selected", nameof(_defaultProjectView))]
    void OnDefaultProjectView(int index) {
        string oldVal = CentralStore.Settings.DefaultView;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.DefaultView = oldVal;
                _defaultProjectView.Select(_views.IndexOf(oldVal));
            });
            updateActionButtons();
        }
        CentralStore.Settings.DefaultView = _defaultProjectView.GetItemText(index);
    }

    [SignalHandler("item_selected", nameof(_defaultEngine))]
    async void OnDefaultEngine(int index) {
        string oldVal = CentralStore.Settings.DefaultEngine;
        string engine = (string)_defaultEngine.GetItemMetadata(index);
        int oldIndex = -1;
        for (int i = 0; i < _defaultEngine.GetItemCount(); i++) {
            if (oldVal == (string)_defaultEngine.GetItemMetadata(index)) {
                oldIndex = i;
                break;
            }
        }

        if (!bPInternal) {
            _undoActions.Push(async () => {
                CentralStore.Settings.DefaultEngine = oldVal;
                _defaultEngine.Select(oldIndex);
                await GetNode<GodotPanel>("/root/MainWindow/bg/Shell/VC/TabContainer/Godot").PopulateList();
            });
            updateActionButtons();
        }
        CentralStore.Settings.DefaultEngine = engine;
        await GetNode<GodotPanel>("/root/MainWindow/bg/Shell/VC/TabContainer/Godot").PopulateList();
    }

    [SignalHandler("toggled", nameof(_checkForUpdates))]
    void OnToggleCheckForUpdates(bool toggle) {
        _updateCheckInterval.Disabled = !toggle;
        bool oldVal = CentralStore.Settings.CheckForUpdates;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.CheckForUpdates = oldVal; 
                _checkForUpdates.Pressed = oldVal;
            });
            updateActionButtons();
        }
        CentralStore.Settings.CheckForUpdates = toggle;
    }

    [SignalHandler("item_selected", nameof(_updateCheckInterval))]
    void OnUpdateCheckInterval(int index) {
        TimeSpan oldVal = CentralStore.Settings.CheckInterval;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.CheckInterval = oldVal;
                _updateCheckInterval.Select(GetIntervalIndex());
            });
            updateActionButtons();
        }
        CentralStore.Settings.CheckInterval = System.TimeSpan.FromHours(_dCheckInterval[index]);
    }

    [SignalHandler("toggled", nameof(_noConsole))]
    void OnNoConsole(bool toggle) {
        bool oldVal = CentralStore.Settings.NoConsole;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.NoConsole = oldVal;
                _noConsole.Pressed = oldVal;
            });
            updateActionButtons();
        }
        CentralStore.Settings.NoConsole = toggle;
    }

    [SignalHandler("toggled", nameof(_editorProfiles))]
    void OnEditorProfiles(bool toggle) {
        bool oldVal = CentralStore.Settings.SelfContainedEditors;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.SelfContainedEditors = oldVal;
                _editorProfiles.Pressed = oldVal;
            });
            updateActionButtons();
        }
        CentralStore.Settings.SelfContainedEditors = toggle;
    }

    #region Asset Mirror Actions
    [SignalHandler("add_requested", nameof(_assetMirror))]
    void OnAssetMirror_Add() {
        AppDialogs.AddonMirror.Connect("asset_add_mirror", this, "OnAssetAddMirror");
        AppDialogs.AddonMirror.ShowDialog();
    }

    void OnAssetAddMirror(string protocol, string domainName, string pathTo) {
        string url = $"{protocol}://{domainName}{pathTo}";

        int indx = _assetMirror.GetItemCount();
        _undoActions.Push(() => {
            _assetMirror.RemoveItem(indx);
            _assetMirror.RemoveMeta(domainName);
        });
        updateActionButtons();

        _assetMirror.AddItem(domainName);
        _assetMirror.SetMeta(domainName, url);
        
        AppDialogs.AddonMirror.Disconnect("asset_add_mirror", this, "OnAssetAddMirror");
    }

    [SignalHandler("edit_requested", nameof(_assetMirror))]
    void OnAssetMirror_Edit() {
        int indx = _assetMirror.GetSelected();
        if (indx == -1)
            return;

        var url = (string)_assetMirror.GetMeta(_assetMirror.GetItemText(indx));
        var uri = new Uri(url);

        AppDialogs.AddonMirror.Connect("asset_add_mirror", this, "OnAssetEditMirror");
        AppDialogs.AddonMirror.ShowDialog(uri.Scheme, uri.Host, uri.AbsolutePath, true);
    }

    void OnAssetEditMirror(string protocol, string domainName, string pathTo) {
        string url = $"{protocol}://{domainName}{pathTo}";
        int indx = _assetMirror.GetSelected();

        var oldName = _assetMirror.GetItemText(indx);
        var oldUrl = (string)_assetMirror.GetMeta(oldName);
        
        if (oldName != domainName)
            _assetMirror.RemoveMeta(oldName);
        
        _assetMirror.SetItemText(indx, domainName);
        _assetMirror.SetMeta(domainName, url);

        _undoActions.Push(() => {
            if (oldName != domainName)
                _assetMirror.RemoveMeta(domainName);
            
            _assetMirror.SetMeta(oldName, oldUrl);
            _assetMirror.SetItemText(indx, oldName);
        });
        updateActionButtons();

        AppDialogs.AddonMirror.Disconnect("asset_add_mirror", this, "OnAssetEditMirror");
    }

    [SignalHandler("remove_requested", nameof(_assetMirror))]
    void OnAssetMirror_Remove() {
        int indx = _assetMirror.GetSelected();
        if (indx == -1)
            return;
        
        var oldName = _assetMirror.GetItemText(indx);
        var oldUrl = _assetMirror.GetMeta(oldName);

        _undoActions.Push(() => {
            var nindx = _assetMirror.GetItemCount();
            _assetMirror.AddItem(oldName);
            _assetMirror.SetMeta(oldName, oldUrl);
            _assetMirror.MoveItem(nindx, indx);
        });
        updateActionButtons();

        _assetMirror.RemoveItem(indx);
        _assetMirror.RemoveMeta(oldName);
    }
    #endregion

    #region Godot Mirror Actions
    [SignalHandler("add_requested", nameof(_godotMirror))]
    void OnGodotMirror_Add() {

    }

    [SignalHandler("edit_requested", nameof(_godotMirror))]
    void OnGodotMirror_Edit() {

    }

    [SignalHandler("remove_requested", nameof(_godotMirror))]
    void OnGodotMirror_Remove() {

    }
    #endregion
#endregion

#region Event Handlers for Projects Page
    [SignalHandler("text_changed", nameof(_defaultProjectLocation))]
    void OnDefaultProjectLocation() {
        string oldVal = CentralStore.Settings.ProjectPath;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.ProjectPath = oldVal;
                _defaultProjectLocation.Text = oldVal.GetOSDir().NormalizePath();
            });
            updateActionButtons();
        }
        CentralStore.Settings.ProjectPath = _defaultProjectLocation.Text;
    }

    [SignalHandler("pressed", nameof(_browseProjectLocation))]
    void OnBrowseProjectLocation_Pressed() {
        AppDialogs.BrowseFolderDialog.CurrentFile = "";
        AppDialogs.BrowseFolderDialog.CurrentPath = _defaultProjectLocation.Text.NormalizePath();
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnBrowseProjectLocation_DirSelected");
    }

    void OnBrowseProjectLocation_DirSelected(string path) {
        _defaultProjectLocation.Text = path.NormalizePath();
        AppDialogs.BrowseFolderDialog.Visible = false;
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnBrowseProjectLocation_DirSelected");
        OnDefaultProjectLocation();
    }

    [SignalHandler("toggled", nameof(_exitGodotManager))]
    void OnExitGodotManager(bool toggle) {
        bool oldVal = CentralStore.Settings.CloseManagerOnEdit;
        if (!bPInternal) {
            _undoActions.Push(() => {
                CentralStore.Settings.CloseManagerOnEdit = oldVal;
                _exitGodotManager.Pressed = oldVal;
            });
            updateActionButtons();
        }
        CentralStore.Settings.CloseManagerOnEdit = toggle;
    }

    #region Directory Scan List Actions
    [SignalHandler("add_requested", nameof(_directoryScan))]
    void OnDirScan_AddRequest() {
        AppDialogs.BrowseFolderDialog.CurrentFile = "";
        AppDialogs.BrowseFolderDialog.CurrentPath = _defaultProjectLocation.Text.NormalizePath();
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnDirScan_DirSelected");
    }

    void OnDirScan_DirSelected(string path) {
        int index = _directoryScan.GetItemCount();
        _directoryScan.AddItem(path.NormalizePath());
        if (!bPInternal) {
            _undoActions.Push(() => _directoryScan.RemoveItem(index));
            updateActionButtons();
        }
        AppDialogs.BrowseFolderDialog.Visible = false;
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnDirScan_DirSelected");
    }

    [SignalHandler("edit_requested", nameof(_directoryScan))]
    void OnDirScan_EditRequest() {
        int index = _directoryScan.GetSelected();
        if (index == -1)
            return;
        AppDialogs.BrowseFolderDialog.CurrentFile = "";
        AppDialogs.BrowseFolderDialog.CurrentPath = _directoryScan.GetItemText(index).NormalizePath();
        AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnEditDirScan_DirSelected", new Array() { index });
        AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
    }

    void OnEditDirScan_DirSelected(string path, int index) {
        string oldVal = _directoryScan.GetItemText(index);
        _undoActions.Push(() => _directoryScan.SetItemText(index, oldVal));
        updateActionButtons();
        _directoryScan.SetItemText(index, path);
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnEditDirScan_DirSelected");
    }

    [SignalHandler("remove_requested", nameof(_directoryScan))]
    void OnDirScan_RemoveRequest() {
        int indx = _directoryScan.GetSelected();
        if (indx == -1)
            return;
        string oldVal = _directoryScan.GetItemText(indx);
        if (!bPInternal) {
            _undoActions.Push(() => {
                int nidx = _directoryScan.GetItemCount();
                _directoryScan.AddItem(oldVal);
                _directoryScan.MoveItem(nidx, indx);
            });
            updateActionButtons();
        }
        _directoryScan.RemoveItem(indx);
    }
    #endregion

#endregion

#region Event Handler for About Page
    [SignalHandler("meta_clicked", nameof(_emailWebsite))]
    [SignalHandler("meta_clicked", nameof(_builtWith))]
    [SignalHandler("meta_clicked", nameof(_specialThanks))]
    [SignalHandler("meta_clicked", nameof(_mitLicense))]
    [SignalHandler("meta_clicked", nameof(_apacheLicense))]
    void OnMetaClicked(object meta) {
        OS.ShellOpen((string)meta);
    }

    [SignalHandler("gui_input", nameof(_buyMe))]
    void OnBuyMe_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iembEvent) {
            if (iembEvent.Pressed && iembEvent.ButtonIndex == (int)ButtonList.Left) {
                OS.ShellOpen("https://www.buymeacoffee.com/eumario");
            }
        }
    }
#endregion

}
