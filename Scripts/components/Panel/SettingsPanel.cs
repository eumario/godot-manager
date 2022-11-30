using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using ActionStack = System.Collections.Generic.Stack<System.Action>;
using Uri = System.Uri;
using TimeSpan = System.TimeSpan;
using Dir = System.IO.Directory;
using SFile = System.IO.File;
using System.IO.Compression;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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

	[NodePath("VB/Header/HC/PC/HC/Contributions")]
	Button _contributionBtn = null;

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

	[NodePath("VB/MC/TC/General/GC/SIContainer/TitleBar")]
	private CheckBox _useSystemTitlebar = null;

	[NodePath("VB/MC/TC/General/GC/SIContainer/CDELabel")]
	private Label _cdeLabel = null;
	
	[NodePath("VB/MC/TC/General/GC/SIContainer/CreateDesktopEntry")]
	private Button _createDesktopEntry = null;

	[NodePath("VB/MC/TC/General/GC/SIContainer/RemoveDesktopEntry")]
	private Button _removeDesktopEntry = null;

	[NodePath("VB/MC/TC/General/GC/UseLastMirror")]
	CheckBox _useLastMirror = null;

	[NodePath("VB/MC/TC/General/GC/HBCI/CheckBox")]
	CheckBox _useProxy = null;

	[NodePath("VB/MC/TC/General/GC/HBCI/ProxyContainer")]
	HBoxContainer _proxyContainer = null;

	[NodePath("VB/MC/TC/General/GC/HBCI/ProxyContainer/ProxyHost")]
	LineEdit _proxyHost = null;

	[NodePath("VB/MC/TC/General/GC/HBCI/ProxyContainer/ProxyPort")]
	LineEdit _proxyPort = null;

	[NodePath("VB/MC/TC/General/GC/VBLO/NoConsole")]
	CheckBox _noConsole = null;

	[NodePath("VB/MC/TC/General/GC/VBLO/EditorProfiles")]
	CheckBox _editorProfiles = null;

	[NodePath("VB/MC/TC/General/GC/MirrorTabs/Asset Library")]
	ItemListWithButtons _assetMirror = null;

	// [NodePath("VB/MC/TC/General/GC/MirrorTabs/Godot Engine")]
	// ItemListWithButtons _godotMirror = null;
	#endregion

	#region Projects Page
	[NodePath("VB/MC/TC/Projects/GC/HBPL/DefaultProjectLocation")]
	LineEdit _defaultProjectLocation = null;

	[NodePath("VB/MC/TC/Projects/GC/HBPL/BrowseProjectLocation")]
	Button _browseProjectLocation = null;

	[NodePath("VB/MC/TC/Projects/GC/ExitManager")]
	CheckBox _exitGodotManager = null;

	[NodePath("VB/MC/TC/Projects/GC/AutoScanProjects")]
	CheckBox _autoScanProjects = null;

	[NodePath("VB/MC/TC/Projects/GC/DirectoryScan")]
	ItemListWithButtons _directoryScan = null;
	#endregion

	#region About Page
	[NodePath("VB/MC/TC/About/MC/VB/HBHeader/VB/VersionInfo")]
	Label _versionInfo = null;
	[NodePath("VB/MC/TC/About/MC/VB/HBHeader/VB/EmailWebsite")]
	RichTextLabel _emailWebsite = null;

	[NodePath("VB/MC/TC/About/MC/VB/HBHeader/VB2/CheckUpdatesGM")]
	Button _checkForUpdatesGM = null;

	[NodePath("VB/MC/TC/About/MC/VB/MC/BuiltWith")]
	RichTextLabel _builtWith = null;

	[NodePath("VB/MC/TC/About/MC/VB/CenterContainer/HB/VB/BuyMe")]
	TextureRect _buyMe = null;

	[NodePath("VB/MC/TC/About/MC/VB/CenterContainer/HB/VB/ItchIO")]
	TextureRect _itchIo = null;

	[NodePath("VB/MC/TC/About/MC/VB/CenterContainer/HB/VB2/Github")]
	TextureRect _github = null;

	[NodePath("VB/MC/TC/About/MC/VB/CenterContainer/HB/VB2/Discord")]
	TextureRect _discord = null;
	#endregion

	#region Contributions Page
	[NodePath("VB/MC/TC/Contributions/Contributors")]
	RichTextLabel _contributors = null;
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

	#region Version String for About
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

	Regex IsNumeric = new Regex(@"\d+");
	#endregion

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();
		UpdateShortcutButtons();

		_builtWith.BbcodeText = BuildVersionInfo(); //VERSION_INFORMATION;
		_undoActions = new ActionStack();
		_views = new Array<string>();

		for (int i = 0; i < _defaultProjectView.GetItemCount(); i++) {
			_views.Add(_defaultProjectView.GetItemText(i));
		};

		// Load up our Settings to be displayed to end user
		LoadSettings();
		updateActionButtons();

		GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
		_versionInfo.Text = $"Version {VERSION.GodotManager}-{VERSION.Channel}";

		_updateCheckInterval.Disabled = !_checkForUpdates.Pressed;

		// Translations for Options
		_defaultProjectView.UpdateTr(0, Tr("List View"));
		_defaultProjectView.UpdateTr(1, Tr("Icon View"));
		_defaultProjectView.UpdateTr(2, Tr("Category View"));
		_defaultProjectView.UpdateTr(3, Tr("Last View Used"));

		_updateCheckInterval.UpdateTr(0, Tr("1 Hour"));
		_updateCheckInterval.UpdateTr(1, Tr("12 Hours"));
		_updateCheckInterval.UpdateTr(2, Tr("1 Day"));
		_updateCheckInterval.UpdateTr(3, Tr("1 Week"));
		_updateCheckInterval.UpdateTr(4, Tr("Bi-Weekly"));
		_updateCheckInterval.UpdateTr(5, Tr("Monthly (Every 30 Days)"));
	}
	
	void UpdateShortcutButtons() {
		_cdeLabel.Visible = Platform.OperatingSystem == "Linux (or BSD)";
		_createDesktopEntry.Visible = Platform.OperatingSystem == "Linux (or BSD)";
		_removeDesktopEntry.Visible = Platform.OperatingSystem == "Linux (or BSD)";
#if GODOT_X11 || GODOT_LINUXBSD
		_createDesktopEntry.Disabled = CentralStore.Settings.ShortcutMade;
		_removeDesktopEntry.Disabled = !CentralStore.Settings.ShortcutMade;
#endif
	}

	void updateActionButtons() {
		_actionButtons.Visible = _undoActions.Count > 0;
	}

	string BuildVersionInfo() {
		return "[table=3][cell][color=green]" + Tr("Project Name") +
		"[/color][/cell][cell][color=green]" + Tr("Version") +
		"[/color]     [/cell][cell][color=green]Website[/color][/cell]" +
		"[cell][color=aqua]Godot Engine (Mono Edition)      [/color][/cell][cell][color=white]v" +
		Engine.GetVersionInfo()["string"] +
		"     [/color][/cell][cell][color=yellow][url]https://godotengine.org[/url][/color][/cell]" +
		"[cell][color=aqua]GodotSharpExtras[/color][/cell][cell][color=white]v" +
		VERSION.GodotSharpExtras +
		"[/color][/cell][cell][color=yellow][url]https://github.com/eumario/GodotSharpExtras[/url][/color][/cell]" +
		"[cell][color=aqua]NewtonSoft JSON[/color][/cell][cell][color=white]v" + 
		VERSION.NewtonsoftJSON +
		"[/color][/cell][cell][color=yellow][url]https://www.newtonsoft.com/json[/url][/color][/cell]" +
		"[cell][color=aqua]SixLabors ImageSharp[/color][/cell][cell][color=white]v" +
		VERSION.ImageSharp +
		"[/color][/cell][cell][color=yellow][url]https://sixlabors.com/products/imagesharp/[/url][/color][/cell]" +
		"[cell][color=aqua]System.IO.Compression[/color][/cell][cell][color=white]v" +
		VERSION.SystemIOCompression +
		"[/color][/cell][cell][color=yellow][url]https://www.nuget.org/packages/System.IO.Compression/[/url][/color][/cell][/table]";
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
		_useSystemTitlebar.Pressed = CentralStore.Settings.UseSystemTitlebar;
		_useLastMirror.Pressed = CentralStore.Settings.UseLastMirror;
		_useProxy.Pressed = CentralStore.Settings.UseProxy;
		_proxyContainer.Visible = CentralStore.Settings.UseProxy;
		_proxyHost.Text = CentralStore.Settings.ProxyHost;
		_proxyPort.Text = $"{CentralStore.Settings.ProxyPort}";
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
		
		// _godotMirror.Clear();
		// foreach (string meta in _godotMirror.GetMetaList())
		// 	_godotMirror.RemoveMeta(meta);
		
		// foreach (Dictionary<string, string> mirror in CentralStore.Settings.EngineMirrors) {
		// 	_godotMirror.AddItem(mirror["name"]);
		// 	_godotMirror.SetMeta(mirror["name"], mirror["url"]);
		// }

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
		CentralStore.Settings.UseSystemTitlebar = _useSystemTitlebar.Pressed;
		CentralStore.Settings.UseLastMirror = _useLastMirror.Pressed;
		CentralStore.Settings.UseProxy = _useProxy.Pressed;
		CentralStore.Settings.ProxyHost = _proxyHost.Text;
		CentralStore.Settings.ProxyPort = _proxyPort.Text.ToInt();
		CentralStore.Settings.SelfContainedEditors = _editorProfiles.Pressed;

		if (CentralStore.Settings.UseSystemTitlebar) {
			OS.WindowBorderless = false;
			GetTree().Root.GetNode<Titlebar>("SceneManager/MainWindow/bg/Shell/VC/TitleBar").Visible = false;
			GetTree().Root.GetNode<Control>("SceneManager/MainWindow/bg/Shell/VC/VisibleSpacer").Visible = true;
		} else {
			OS.WindowBorderless = true;
			GetTree().Root.GetNode<Titlebar>("SceneManager/MainWindow/bg/Shell/VC/TitleBar").Visible = true;
			GetTree().Root.GetNode<Control>("SceneManager/MainWindow/bg/Shell/VC/VisibleSpacer").Visible = false;
		}

		foreach(GodotVersion version in CentralStore.Versions) {
			if (_editorProfiles.Pressed) {
				File fh = new File();
				fh.Open($"{version.Location}/._sc_".GetOSDir().NormalizePath(), File.ModeFlags.Write);
				fh.StoreString(" ");
				fh.Close();
			} else {
				Directory dh = new Directory();
				dh.Open($"{version.Location}".GetOSDir().NormalizePath());
				dh.Remove($"{version.Location}/._sc_".GetOSDir().NormalizePath());
			}
		}

		CentralStore.Settings.NoConsole = _noConsole.Pressed;
		CentralStore.Settings.AssetMirrors.Clear();
		for (int i = 0; i < _assetMirror.GetItemCount(); i++) {
			Dictionary<string, string> data = new Dictionary<string, string>();
			data["name"] = _assetMirror.GetItemText(i);
			data["url"] = (string)_assetMirror.GetMeta(data["name"]);
			CentralStore.Settings.AssetMirrors.Add(data);
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
		if (GetParent<TabContainer>().GetCurrentTabControl() == this) {
			LoadSettings();
		} else {
			if (_undoActions.Count > 0) {
				var res = AppDialogs.YesNoDialog.ShowDialog(Tr("Unsaved Settings"), 
					Tr("You have unsaved settings, do you wish to save your settings?"));
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
		_contributionBtn.Pressed = false;
		_licensesBtn.Pressed = false;
		_pages.CurrentTab = 0;
	}

	[SignalHandler("pressed", nameof(_projectsBtn))]
	void OnProjectsPressed() {
		_generalBtn.Pressed = false;
		_projectsBtn.Pressed = true;
		_aboutBtn.Pressed = false;
		_contributionBtn.Pressed = false;
		_licensesBtn.Pressed = false;
		_pages.CurrentTab = 1;
	}

	[SignalHandler("pressed", nameof(_aboutBtn))]
	void OnAboutPressed() {
		_generalBtn.Pressed = false;
		_projectsBtn.Pressed = false;
		_aboutBtn.Pressed = true;
		_contributionBtn.Pressed = false;
		_licensesBtn.Pressed = false;
		_pages.CurrentTab = 2;
	}

	[SignalHandler("pressed", nameof(_contributionBtn))]
	void OnContributionPressed() {
		_generalBtn.Pressed = false;
		_projectsBtn.Pressed = false;
		_aboutBtn.Pressed = false;
		_contributionBtn.Pressed = true;
		_licensesBtn.Pressed = false;
		_pages.CurrentTab = 3;
	}

	[SignalHandler("pressed", nameof(_licensesBtn))]
	void OnLicensesPressed() {
		_generalBtn.Pressed = false;
		_projectsBtn.Pressed = false;
		_aboutBtn.Pressed = false;
		_contributionBtn.Pressed = false;
		_licensesBtn.Pressed = true;
		_pages.CurrentTab = 4;
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
		AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnBrowseGodot_DirSelected", null, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnBrowseGodot_HidePopup", null, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.WindowTitle = Tr("Browse for Godot Install Folder...");
		AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
	}

	void OnBrowseGodot_DirSelected(string dir_name) {
		_godotInstallLocation.Text = dir_name.GetOSDir().NormalizePath();
		OnGodotInstallLocation();
	}

	void OnBrowseGodot_HidePopup()
	{
		if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnBrowseGodot_DirSelected"))
			AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnBrowseGodot_DirSelected");
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
		AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnBrowseCache_DirSelected", null, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnBrowseCache_PopupHide", null, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.WindowTitle = Tr("Browse for Cache Folder...");
		AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
	}

	void OnBrowseCache_DirSelected(string dir_name) {
		_cacheInstallLocation.Text = dir_name.GetOSDir().NormalizePath();
		OnCacheInstallLocation();
	}

	void OnBrowseCache_PopupHide()
	{
		if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnBrowseCache_DirSelected"))
			AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnBrowseCache_DirSelected");
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
				await GetNode<GodotPanel>("/root/SceneManager/MainWindow/bg/Shell/VC/TabContainer/Godot").PopulateList();
			});
			updateActionButtons();
		}
		CentralStore.Settings.DefaultEngine = engine;
		await GetNode<GodotPanel>("/root/SceneManager/MainWindow/bg/Shell/VC/TabContainer/Godot").PopulateList();
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

	[SignalHandler("pressed", nameof(_useProxy))]
	void OnPressed_UseProxy() {
		bool oldVal = CentralStore.Settings.UseProxy;
		if (!bPInternal) {
			_undoActions.Push(() => {
				CentralStore.Settings.UseProxy = oldVal;
				_proxyContainer.Visible = oldVal;
				_useProxy.Pressed = oldVal;
			});
			updateActionButtons();
		}
		_proxyContainer.Visible = _useProxy.Pressed;
		CentralStore.Settings.UseProxy = _useProxy.Pressed;
	}

	[SignalHandler("text_changed", nameof(_proxyHost))]
	void OnTextChanged_ProxyHost(string newText) {
		string oldVal = CentralStore.Settings.ProxyHost;
		if (!bPInternal) {
			_undoActions.Push(() => {
				CentralStore.Settings.ProxyHost = oldVal;
				_proxyHost.Text = oldVal;
			});
			updateActionButtons();
		}
		CentralStore.Settings.ProxyHost = _proxyHost.Text;
	}
	
	[SignalHandler("text_changed", nameof(_proxyPort))]
	void OnTextChanged_ProxyPort(string newText) {
		if (!bPInternal) {
			if (newText != string.Empty && IsNumeric.IsMatch(newText)) {
				int oldVal = CentralStore.Settings.ProxyPort;
				_undoActions.Push(() => {
					CentralStore.Settings.ProxyPort = oldVal;
					_proxyPort.Text = $"{oldVal}";
				});
				CentralStore.Settings.ProxyPort = _proxyPort.Text.ToInt();
			} else {
				if (newText != string.Empty)
					_proxyPort.Text = newText.Substr(0,newText.Length-1);
			}
			updateActionButtons();
		}
	}

	#if GODOT_X11 || GODOT_LINUXBSD
	[SignalHandler("pressed", nameof(_createDesktopEntry))]
	async void OnPressed_CreateDesktopEntry()
	{
		string iconPath = OS.GetExecutablePath().GetBaseDir().Join("godot-manager.svg");
		string executablePath = OS.GetExecutablePath();

		var allUsers = await AppDialogs.YesNoCancelDialog.ShowDialog(Tr("Create Desktop Entry"),
			Tr("Do you wish to integrate for all users, or just your user?"), Tr("All Users"),
			Tr("Just You"), Tr("Cancel"));
		bool needRoot = false;
		if (allUsers == YesNoCancelDialog.ActionResult.FirstAction)
			needRoot = true;
		else if (allUsers == YesNoCancelDialog.ActionResult.CancelAction)
			return;

		
		using (var fh = new File())
		{
			var err = fh.Open("res://godot-manager.dat", File.ModeFlags.Read);
			var size = fh.GetLen();
			var svg = fh.GetBuffer((long)size);
			fh.Close();
			System.IO.File.WriteAllBytes(iconPath,svg);
		}
		
		if (needRoot)
		{
			iconPath = "/opt/GodotManager/godot-manager.svg";
		}
		
		System.IO.File.WriteAllText("/tmp/godot-manager.desktop", 
			string.Format(FirstRunWizard.DESKTOP_ENTRY, iconPath, executablePath), Encoding.ASCII);
		if (needRoot)
		{
			bool needToCopy = false;
			if (!executablePath.StartsWith("/opt/GodotManager"))
			{
				executablePath = "/opt/GodotManager/GodotManager.x86_64";
				System.IO.File.WriteAllText("/tmp/godot-manager.desktop",
					string.Format(FirstRunWizard.DESKTOP_ENTRY, iconPath, executablePath), Encoding.ASCII);
				needToCopy = true;
			}

			using (var fh = new File())
			{
				fh.Open("/tmp/godot-installer.sh", File.ModeFlags.Write);
				fh.StoreString("#!/bin/bash\n\n");
				if (needToCopy)
				{
					if (System.IO.Directory.Exists("/opt/GodotManager")) fh.StoreString("rm -rf /opt/GodotManager\n");
					fh.StoreString("mkdir -p /opt/GodotManager\n");
					fh.StoreString($"cp -r {OS.GetExecutablePath().GetBaseDir()}/* /opt/GodotManager/\n");
				}
				fh.StoreString("xdg-desktop-menu install --mode system /tmp/godot-manager.desktop\n");
				fh.StoreString("xdg-desktop-menu forceupdate --mode system\n");

				fh.Close();
				Util.Chmod("/tmp/godot-installer.sh", 0755);
				var execre = Util.PkExec("/tmp/godot-installer.sh", Tr("Install Shortcut"),
					Tr("Godot Manager needs Administrative privileges to complete the requested actions."));
				GD.Print($"Execre: {execre}");
				if (execre > 0)
				{
					AppDialogs.MessageDialog.ShowMessage(Tr("Install Shortcut"), Tr("Failed to install Shortcut."));
					return;
				}
				System.IO.File.Delete("/tmp/godot-installer.sh");
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
		CentralStore.Instance.SaveDatabase();
		UpdateShortcutButtons();
	}

	[SignalHandler("pressed", nameof(_removeDesktopEntry))]
	async void OnPressed_RemoveDesktopEntry()
	{
		if (CentralStore.Settings.ShortcutMade)
		{
			if (CentralStore.Settings.ShortcutRoot)
			{
				using (var fh = new File())
				{
					var res = fh.Open("/tmp/godot-uninstaller.sh", File.ModeFlags.Write);
					fh.StoreString("#!/bin/bash\n\n");
					fh.StoreString("xdg-desktop-menu uninstall --mode system godot-manager.desktop\n");
					fh.StoreString("xdg-desktop-menu forceupdate --mode system\n");

					fh.Close();
					Util.Chmod("/tmp/godot-uninstaller.sh", 0755);
					var execre = Util.PkExec("/tmp/godot-uninstaller.sh", Tr("Remove Shortcut"),
						Tr("Godot Manager needs Administrative privileges to complete the requested actions."));
					GD.Print($"execre: {execre}");
					if (execre > 0)
					{
						AppDialogs.MessageDialog.ShowMessage(Tr("Remove Shortcut"), Tr("Failed to remove Shortcut.") );
						return;
					}
					System.IO.File.Delete("/tmp/godot-uninstaller.sh");
				}
			}
			else
			{
				Util.XdgDesktopUninstall("godot-manager.desktop");
				Util.XdgDesktopUpdate();
			}

			CentralStore.Settings.ShortcutMade = false;
			CentralStore.Settings.ShortcutRoot = false;
			CentralStore.Instance.SaveDatabase();
			UpdateShortcutButtons();
		}
	}
	#endif

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

	// #region Godot Mirror Actions
	// [SignalHandler("add_requested", nameof(_godotMirror))]
	// void OnGodotMirror_Add() {

	// }

	// [SignalHandler("edit_requested", nameof(_godotMirror))]
	// void OnGodotMirror_Edit() {

	// }

	// [SignalHandler("remove_requested", nameof(_godotMirror))]
	// void OnGodotMirror_Remove() {

	// }
	// #endregion
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
		AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnBrowseProjectLocation_DirSelected", null, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnBrowseProjectLocation_PopupHide", null, (int)ConnectFlags.Oneshot);
	}

	void OnBrowseProjectLocation_DirSelected(string path) {
		_defaultProjectLocation.Text = path.NormalizePath();
		AppDialogs.BrowseFolderDialog.Visible = false;
		OnDefaultProjectLocation();
	}

	void OnBrowseProjectLocation_PopupHide()
	{
		if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnBrowseProjectLocation_DirSelected"))
			AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnBrowseProjectLocation_DirSelected");
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

	[SignalHandler("toggled", nameof(_autoScanProjects))]
	void OnAutoScanProjects(bool toggle)
	{
		bool oldVal = CentralStore.Settings.EnableAutoScan;
		if (!bPInternal) {
			_undoActions.Push(() => {
				CentralStore.Settings.EnableAutoScan = oldVal;
				_autoScanProjects.Pressed = oldVal;
			});
			updateActionButtons();
		}
		CentralStore.Settings.EnableAutoScan = toggle;
	}

	[SignalHandler("toggled", nameof(_useSystemTitlebar))]
	void OnUseSystemTitlebar(bool toggle) {
		bool oldVal = CentralStore.Settings.UseSystemTitlebar;
		if (!bPInternal) {
			_undoActions.Push(() => {
				CentralStore.Settings.UseSystemTitlebar = oldVal;
				_useSystemTitlebar.Pressed = oldVal;
			});
			updateActionButtons();
		}
		CentralStore.Settings.UseSystemTitlebar = toggle;
	}

	[SignalHandler("toggled", nameof(_useLastMirror))]
	void OnUseLastMirror(bool toggle)
	{
		bool oldVal = CentralStore.Settings.UseLastMirror;
		if (!bPInternal)
		{
			_undoActions.Push(() =>
			{
				CentralStore.Settings.UseLastMirror = oldVal;
				_useLastMirror.Pressed = oldVal;
			});
			updateActionButtons();
		}

		CentralStore.Settings.UseLastMirror = toggle;
	}

	#region Directory Scan List Actions
	[SignalHandler("add_requested", nameof(_directoryScan))]
	void OnDirScan_AddRequest() {
		AppDialogs.BrowseFolderDialog.CurrentFile = "";
		AppDialogs.BrowseFolderDialog.CurrentPath = _defaultProjectLocation.Text.NormalizePath();
		AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
		AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnDirScan_DirSelected", null, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnDirScan_PopupHide", null, (int)ConnectFlags.Oneshot);
	}

	void OnDirScan_DirSelected(string path) {
		int index = _directoryScan.GetItemCount();
		_directoryScan.AddItem(path.NormalizePath());
		if (!bPInternal) {
			_undoActions.Push(() => _directoryScan.RemoveItem(index));
			updateActionButtons();
		}
		AppDialogs.BrowseFolderDialog.Visible = false;
	}

	void OnDirScan_PopupHide()
	{
		if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnDirScan_DirSelected"))
			AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnDirScan_DirSelected");
	}

	[SignalHandler("edit_requested", nameof(_directoryScan))]
	void OnDirScan_EditRequest() {
		int index = _directoryScan.GetSelected();
		if (index == -1)
			return;
		AppDialogs.BrowseFolderDialog.WindowTitle = Tr("Folder to Add to Scan");
		AppDialogs.BrowseFolderDialog.CurrentFile = "";
		AppDialogs.BrowseFolderDialog.CurrentPath = _directoryScan.GetItemText(index).NormalizePath();
		AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnEditDirScan_DirSelected", new Array() { index }, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnEditDirScan_PopupHide", null, (int)ConnectFlags.Oneshot);
		AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
	}

	void OnEditDirScan_DirSelected(string path, int index) {
		string oldVal = _directoryScan.GetItemText(index);
		_undoActions.Push(() => _directoryScan.SetItemText(index, oldVal));
		updateActionButtons();
		_directoryScan.SetItemText(index, path);
		AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnEditDirScan_DirSelected");
	}

	void OnEditDirScan_PopupHide()
	{
		if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnEditDirScan_DirSelected"))
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
	[SignalHandler("meta_clicked", nameof(_mitLicense))]
	[SignalHandler("meta_clicked", nameof(_apacheLicense))]
	[SignalHandler("meta_clicked", nameof(_contributors))]
	void OnMetaClicked(object meta) {
		OS.ShellOpen((string)meta);
	}

	[SignalHandler("pressed", nameof(_checkForUpdatesGM))]
	async void OnCheckForUpdatesGM_Pressed() {
		AppDialogs.BusyDialog.UpdateHeader(Tr("Checking for updates for Godot Manager..."));
		AppDialogs.BusyDialog.UpdateByline(Tr("Connecting to GitHub..."));
		AppDialogs.BusyDialog.ShowDialog();
		var res = Github.Github.Instance.GetLatestManagerRelease();
		while (!res.IsCompleted) {
			await this.IdleFrame();
		}

		if (res.Result == null) {
			AppDialogs.BusyDialog.HideDialog();
			AppDialogs.MessageDialog.ShowMessage(Tr("Godot Manager - Check for Updates"), 
				Tr("Failed to get release information from Github"));
			return;
		}

		Github.Release rel = res.Result;
		if (rel.TagName != $"v{VERSION.GodotManager}") {
			AppDialogs.BusyDialog.HideDialog();
			AppDialogs.NewVersion.ShowDialog(rel,true);
			AppDialogs.NewVersion.Connect("download_manager_update", this, "OnDownloadManagerUpdate");
		} else {
			AppDialogs.BusyDialog.HideDialog();
			AppDialogs.MessageDialog.ShowMessage(Tr("Check for Godot Manager Updates"),
				Tr("Currently on latest version of Godot Manager."));
		}
	}

	void OnDownloadManagerUpdate(Github.Release release) {
		AppDialogs.NewVersion.Disconnect("download_manager_update", this, "OnDownloadManagerUpdate");
		AppDialogs.DownloadGodotManager.ShowDialog(release);
		AppDialogs.DownloadGodotManager.Connect("download_complete", this, "OnDownloadGodotManagerCompleted");
	}

	void OnDownloadGodotManagerCompleted(Github.Release release,Github.Asset asset) {
		AppDialogs.DownloadGodotManager.Disconnect("download_complete", this,"OnDownloadGodotManagerCompleted");
		string updatePath = Util.GetUpdateFolder().Join("update.zip").GetOSDir().NormalizePath();
		#if GODOT_WINDOWS || GODOT_UWP || GODOT_LINUXBSD || GODOT_X11
		string updater = Util.GetUpdateFolder().Join(OS.GetExecutablePath().GetFile()).NormalizePath();
		#else
		string updater = Util.GetUpdateFolder().Join("Godot Manager.app","Contents","MacOS",OS.GetExecutablePath().GetFile()).NormalizePath();
		#endif
		ZipFile.ExtractToDirectory(updatePath, updatePath.GetBaseDir());

		#if GODOT_LINUXBSD || GODOT_X11 || GODOT_MACOS || GODOT_OSX
		Util.Chmod(updater,0755);
		#endif

		#if GODOT_MACOS || GODOT_OSX
		Util.XAttr(Util.GetUpdateFolder().Join("Godot Manager.app").NormalizePath(), "-cr");
		#endif

		ProcessStartInfo psi = new ProcessStartInfo();
		psi.FileName = updater;
		#if GODOT_WINDOWS || GODOT_UWP || GODOT_LINUXBSD || GODOT_X11
		psi.WorkingDirectory = updater.GetBaseDir().NormalizePath();
		#else
		psi.WorkingDirectory = updater.GetParentFolder().GetBaseDir().NormalizePath();
		#endif
		psi.Arguments = $"--update {Process.GetCurrentProcess().Id}";
		psi.UseShellExecute = false;
		psi.CreateNoWindow = false;

		Process proc = Process.Start(psi);
		GetTree().Quit(0);
	}

	[SignalHandler("gui_input", nameof(_buyMe))]
	void OnBuyMe_GuiInput(InputEvent inputEvent) {
		if (inputEvent is InputEventMouseButton iembEvent) {
			if (iembEvent.Pressed && iembEvent.ButtonIndex == (int)ButtonList.Left) {
				OS.ShellOpen("https://www.buymeacoffee.com/eumario");
			}
		}
	}

	[SignalHandler("gui_input", nameof(_itchIo))]
	void OnItchIo_GuiInput(InputEvent inputEvent) {
		if (inputEvent is InputEventMouseButton iembEvent) {
			if (iembEvent.Pressed && iembEvent.ButtonIndex == (int)ButtonList.Left) {
				OS.ShellOpen("https://eumario.itch.io/godot-manager");
			}
		}
	}

	[SignalHandler("gui_input", nameof(_github))]
	void OnGithub_GuiInput(InputEvent inputEvent) {
		if (inputEvent is InputEventMouseButton iembEvent) {
			if (iembEvent.Pressed && iembEvent.ButtonIndex == (int)ButtonList.Left) {
				OS.ShellOpen("https://github.com/eumario/godot-manager");
			}
		}
	}

	[SignalHandler("gui_input", nameof(_discord))]
	void OnDiscord_GuiInput(InputEvent inputEvent) {
		if (inputEvent is InputEventMouseButton iembEvent) {
			if (iembEvent.Pressed && iembEvent.ButtonIndex == (int)ButtonList.Left) {
				OS.ShellOpen("https://discord.gg/ESkwAMN2Tt");
			}
		}
	}
#endregion

}
