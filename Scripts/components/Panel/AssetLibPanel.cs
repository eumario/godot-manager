using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using TimeSpan = System.TimeSpan;
using DateTime = System.DateTime;
using FileStream = System.IO.FileStream;
using FileMode = System.IO.FileMode;
using Directory = System.IO.Directory;
using SFile = System.IO.File;
using System.IO.Compression;

public class AssetLibPanel : Panel
{
#region Nodes Path
    #region Search Switcher
    [NodePath("VC/MC/HC/PC/HC/Addons")]
    Button _addonsBtn = null;

    [NodePath("VC/MC/HC/PC/HC/Templates")]
    Button _templatesBtn = null;

    [NodePath("VC/MC/HC/PC/HC/Manage")]
    Button _manageBtn = null;
    #endregion

    #region Search Container
    [NodePath("VC/SearchContainer")]
    VBoxContainer _searchContainer = null;

    #region Search Fields
    [NodePath("VC/SearchContainer/HC/SearchField")]
    LineEdit _searchField = null;

    [NodePath("VC/SearchContainer/HC/Import")]
    Button _import = null;

    [NodePath("VC/SearchContainer/HC2/SortBy")]
    OptionButton _sortBy = null;

    [NodePath("VC/SearchContainer/HC2/Category")]
    OptionButton _category = null;

    [NodePath("VC/SearchContainer/HC2/GodotVersion")]
    private OptionButton _godotVersion = null;

    [NodePath("VC/SearchContainer/HC2/MirrorSite")]
    OptionButton _mirrorSite = null;

    [NodePath("VC/SearchContainer/HC2/Support")]
    Button _support = null;

    [NodePath("VC/SearchContainer/HC2/Support/SupportPopup")]
    PopupMenu _supportPopup = null;
    #endregion

    #region Paginated Listings for Addons and Templates
    [NodePath("VC/SearchContainer/plAddons")]
    PaginatedListing _plAddons = null;

    [NodePath("VC/SearchContainer/plTemplates")]
    PaginatedListing _plTemplates = null;
    #endregion
    #endregion

    #region Manage Container
    [NodePath("VC/ManageContainer")]
    VBoxContainer _manageContainer = null;

    [NodePath("VC/ManageContainer/HC/PC/HC/Addons")]
    Button _mAddonsBtn = null;

    [NodePath("VC/ManageContainer/HC/PC/HC/Templates")]
    Button _mTemplateBtn = null;

    [NodePath("VC/ManageContainer/plmAddons")]
    PaginatedListing _plmAddons = null;

    [NodePath("VC/ManageContainer/plmTemplates")]
    PaginatedListing _plmTemplates = null;
    #endregion

    #region Timers
    [NodePath("ExecuteDelay")]
    Timer _executeDelay = null;
    #endregion
#endregion

#region Private Variables
    int _plaCurrentPage = 0;
    int _pltCurrentPage = 0;
    int _plmCurrentPage = 0;
    string lastSearch = "";
    DateTime lastConfigureRequest;
    DateTime lastSearchRequest;
    TimeSpan defaultWaitSearch = TimeSpan.FromMinutes(5);
    TimeSpan defaultWaitConfigure = TimeSpan.FromHours(2);
#endregion

    public override void _Ready()
    {
        this.OnReady();
        lastConfigureRequest = DateTime.Now - TimeSpan.FromHours(3);
        lastSearchRequest = DateTime.Now - TimeSpan.FromMinutes(6);
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
        _mirrorSite.Clear();
        foreach (Dictionary<string, string> mirror in CentralStore.Settings.AssetMirrors) {
            var indx = _mirrorSite.GetItemCount();
            _mirrorSite.AddItem(mirror["name"]);
            _mirrorSite.SetItemMetadata(indx,mirror["url"]);
        }

        // Translations for Options/Menu Items
        _sortBy.UpdateTr(0, Tr("Recently Updated"));
        _sortBy.UpdateTr(1, Tr("Least Recently Updated"));
        _sortBy.UpdateTr(2, Tr("Name (A-Z)"));
        _sortBy.UpdateTr(3, Tr("Name (Z-A)"));
        _sortBy.UpdateTr(4, Tr("License (A-Z)"));
        _sortBy.UpdateTr(5, Tr("License (Z-A)"));

        _supportPopup.UpdateTr(0, Tr("Official"));
        _supportPopup.UpdateTr(1, Tr("Community"));
        _supportPopup.UpdateTr(2, Tr("Testing"));
    }

    [SignalHandler("pressed", nameof(_import))]
    async void OnImportPressed() {
        var result = await AppDialogs.YesNoCancelDialog.ShowDialog(Tr("Import Asset..."),Tr("Do you wish to import a Template or an Addon?"),Tr("Template"),Tr("Addon"),Tr("Cancel"));
        if (result == YesNoCancelDialog.ActionResult.FirstAction) {
            AppDialogs.ImportFileDialog.WindowTitle = Tr("Import Template...");
            AppDialogs.ImportFileDialog.Filters = new string[] { "project.godot", "*.zip" };
            AppDialogs.ImportFileDialog.Connect("file_selected", this, "OnTemplateImport");
        } else if (result == YesNoCancelDialog.ActionResult.SecondAction) {
            AppDialogs.ImportFileDialog.WindowTitle = Tr("Import Plugin...");
            AppDialogs.ImportFileDialog.Filters = new string[] { "plugin.cfg", "*.zip" };
            AppDialogs.ImportFileDialog.Connect("file_selected", this, "OnPluginImport");
        } else {
            return;
        }
        AppDialogs.ImportFileDialog.Connect("hide", this, "OnImportClosed");
        AppDialogs.ImportFileDialog.CurrentFile = "";
        AppDialogs.ImportFileDialog.CurrentPath = "";
        AppDialogs.ImportFileDialog.PopupCentered(new Vector2(510, 390));
    }

    void OnImportClosed() {
        if (AppDialogs.ImportFileDialog.IsConnected("file_selected", this, "OnTemplateImport"))
            AppDialogs.ImportFileDialog.Disconnect("file_selected", this, "OnTemplateImport");

        if (AppDialogs.ImportFileDialog.IsConnected("file_selected", this, "OnPluginImport"))
            AppDialogs.ImportFileDialog.Disconnect("file_selected", this, "OnPluginImport");
        
        AppDialogs.ImportFileDialog.Disconnect("hide", this, "OnImportClosed");
    }

    void OnPluginImport(string filepath) {
        if (filepath.EndsWith(".cfg")) {  // Plugin Directory Selected
            PluginDirectoryImport(filepath);
        } else if (filepath.EndsWith(".zip")) { // Zip File Selected
            AssetZipImport(filepath, true);
        } else {
            AppDialogs.MessageDialog.ShowMessage(Tr("Import Plugin"), string.Format(Tr("Unable to use {0} to import the plugin."),filepath));
        }
    }

    void OnTemplateImport(string filepath) {
        if (filepath.EndsWith("godot.project")) {
            TemplateDirectoryImport(filepath);
        } else if (filepath.EndsWith(".zip")) {
            AssetZipImport(filepath, false);
        } else {
            AppDialogs.MessageDialog.ShowMessage(Tr("Import Template"), string.Format(Tr("Unable to use {0} to import the template."),filepath));
        }
    }

    [SignalHandler("id_pressed", nameof(_supportPopup))]
    async void OnSupportPopup_IdPressed(int id)
    {
        _supportPopup.SetItemChecked(id, !_supportPopup.IsItemChecked(id));
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    [SignalHandler("pressed", nameof(_support))]
    void OnSupportPressed() {
        _supportPopup.Popup_(new Rect2(_support.RectGlobalPosition + new Vector2(0,_support.RectSize.y), _supportPopup.RectSize));
    }

    [SignalHandler("timeout", nameof(_executeDelay))]
    async void OnExecuteDelay_Timeout() {
        if (lastSearch == _searchField.Text)
            return;
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
        lastSearch = _searchField.Text;
    }

    [SignalHandler("text_changed", nameof(_searchField))]
    void OnSearchField_TextChanged(string text) {
        _executeDelay.Start();
    }

    [SignalHandler("text_entered", nameof(_searchField))]
    async void OnSearchField_TextEntered(string text) {
        if (!_executeDelay.IsStopped())
            _executeDelay.Stop();
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    [SignalHandler("item_selected", nameof(_category))]
    async void OnCategorySelected(int index) {
        _plaCurrentPage = 0;
        _pltCurrentPage = 0;
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    [SignalHandler("item_selected", nameof(_sortBy))]
    async void OnSortBySelected(int index) {
        _plaCurrentPage = 0;
        _pltCurrentPage = 0;
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    [SignalHandler("item_selected", nameof(_godotVersion))]
    async void OnGodotVersionSelected(int index)
    {
        _plaCurrentPage = 0;
        _pltCurrentPage = 0;
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    [SignalHandler("page_changed", nameof(_plAddons))]
    async void OnPLAPageChanged(int page) {
        _plaCurrentPage = page;
        await UpdatePaginatedListing(_plAddons);
    }

    [SignalHandler("page_changed", nameof(_plTemplates))]
    async void OnPLTPageChanged(int page) {
        _pltCurrentPage = page;
        await UpdatePaginatedListing(_plTemplates);
    }

    [SignalHandler("pressed", nameof(_addonsBtn))]
    async void OnAddonsPressed() {
        _templatesBtn.Pressed = false;
        _manageBtn.Pressed = false;
        _plTemplates.Visible = false;
        _plAddons.Visible = true;
        _searchContainer.Visible = true;
        _manageContainer.Visible = false;
        await Configure(false);
        await UpdatePaginatedListing(_plAddons);
    }

    [SignalHandler("pressed", nameof(_templatesBtn))]
    async void OnTemplatesPressed() {
        _addonsBtn.Pressed = false;
        _manageBtn.Pressed = false;
        _plTemplates.Visible = true;
        _plAddons.Visible = false;
        _searchContainer.Visible = true;
        _manageContainer.Visible = false;
        await Configure(true);
        await UpdatePaginatedListing(_plTemplates);
    }

    [SignalHandler("pressed", nameof(_manageBtn))]
    async void OnManagePressed() {
        _addonsBtn.Pressed = false;
        _templatesBtn.Pressed = false;
        _plTemplates.Visible = false;
        _plAddons.Visible = false;
        _searchContainer.Visible = false;
        _manageContainer.Visible = true;
        if (_mTemplateBtn.Pressed)
            await UpdatePaginatedListing(_plmTemplates);
        else
            await UpdatePaginatedListing(_plmAddons);
    }

    [SignalHandler("pressed", nameof(_mAddonsBtn))]
    async void OnManageAddonPressed() {
        _plmAddons.Visible = true;
        _mAddonsBtn.Pressed = true;
        _plmTemplates.Visible = false;
        _mTemplateBtn.Pressed = false;
        await UpdatePaginatedListing(_plmAddons);
    }

    [SignalHandler("pressed", nameof(_mTemplateBtn))]
    async void OnManageTemplatePressed() {
        _plmAddons.Visible = false;
        _mAddonsBtn.Pressed = false;
        _plmTemplates.Visible = true;
        _mTemplateBtn.Pressed = true;
        await UpdatePaginatedListing(_plmTemplates);
    }

    async void OnPageChanged(int page) {
        if (GetParent<TabContainer>().GetCurrentTabControl() == this)
		{
            if ((DateTime.Now - lastConfigureRequest) >= defaultWaitConfigure) {
			    await Configure(_templatesBtn.Pressed);
                if (_category.GetItemCount() == 1)
                    return;
            }

            if ((DateTime.Now - lastSearchRequest) >= defaultWaitSearch) {
			    await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
            }
		}
	}

    [SignalHandler("item_selected", nameof(_mirrorSite))]
    async void OnMirrorSiteSelected(int indx) {
        await Configure(_templatesBtn.Pressed);
        if (_category.GetItemCount() == 1)
            return;
        
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    AssetLib.Asset CreateAssetDirectory(string filepath, bool is_plugin) {
        AssetLib.Asset asset = new AssetLib.Asset();
        if (is_plugin) {
            ConfigFile cfg = new ConfigFile();
            cfg.Load(filepath);
            asset.Type = "addon";
            asset.Title = cfg.GetValue("plugin","name") as string;
            asset.Author = cfg.GetValue("plugin","author") as string;
            asset.VersionString = cfg.GetValue("plugin","version") as string;
            asset.Description = cfg.GetValue("plugin","description") as string;
            asset.IconUrl = "res://Assets/Icons/default_project_icon.png";
        } else {
            ProjectConfig pc = new ProjectConfig(filepath);
            pc.Load();
            asset.Type = "project";
            asset.Title = pc.GetValue("application","config/name");
            asset.Author = "Local User";
            asset.VersionString = "0.0.0";
            asset.Description = pc.GetValue("application","config/description");
            asset.IconUrl = "zip+res://icon.png";
        }
        asset.AssetId = $"local-{CentralStore.Settings.LocalAddonCount}";
        asset.AuthorId = "-1";
        asset.Version = "-1";
        asset.Category = "Local";
        asset.CategoryId = "-3";
        asset.GodotVersion = "3.4";
        asset.Rating = "";
        asset.Cost = "";
        asset.SupportLevel = "";
        asset.DownloadProvider = "local";
        asset.DownloadCommit = "";
        asset.BrowseUrl = $"file://{filepath.GetBaseDir()}";
        asset.IssuesUrl = "";
        asset.Searchable = "";
        asset.ModifyDate = DateTime.UtcNow.ToString();
        asset.DownloadUrl = $"file://{filepath.GetBaseDir()}";
        asset.Previews = new Array<AssetLib.Preview>();
        asset.DownloadHash = "";
        return asset;
    }

    AssetLib.Asset CreateAssetZip(string filepath, bool is_plugin) {
        AssetLib.Asset asset = new AssetLib.Asset();
        if (is_plugin) {
            ConfigFile cfg = new ConfigFile();
            bool found = false;
            
            using (var za = ZipFile.OpenRead(filepath)) {
                foreach(var zae in za.Entries) {
                    if (zae.FullName.EndsWith("plugin.cfg")) {
                        found = true;
                        cfg.Parse(zae.ReadFile());
                        break;
                    }
                }
            }

            if (!found)
                return null;
            
            
            asset.Type = "addon";
            asset.Title = cfg.GetValue("plugin","name") as string;
            asset.Author = cfg.GetValue("plugin","author") as string;
            asset.VersionString = cfg.GetValue("plugin","version") as string;
            asset.Description = cfg.GetValue("plugin","description") as string;
            asset.IconUrl = "res://Assets/Icons/default_project_icon.png";
        } else {
            ProjectConfig pc = new ProjectConfig();
            bool found = false;

            using (var za = ZipFile.OpenRead(filepath)) {
                foreach(var zae in za.Entries) {
                    if (zae.FullName.EndsWith("project.godot")) {
                        found = true;
                        pc.LoadBuffer(zae.ReadFile());
                        break;
                    }
                }
            }

            if (!found)
                return null;
            

            asset.Type = "project";
            asset.Title = pc.GetValue("application","config/name");
            asset.Author = "Local User";
            asset.VersionString = "0.0.0";
            asset.Description = pc.GetValue("application","config/description");
            asset.IconUrl = "zip+" + pc.GetValue("application","config/icon");
            
        }
        asset.AssetId = $"local-{CentralStore.Settings.LocalAddonCount}";
        asset.AuthorId = "-1";
        asset.Version = "-1";
        asset.Category = "Local";
        asset.CategoryId = "-3";
        asset.GodotVersion = "3.4";
        asset.Rating = "";
        asset.Cost = "";
        asset.SupportLevel = "";
        asset.DownloadProvider = "local";
        asset.DownloadCommit = "";
        asset.BrowseUrl = $"file://{filepath.GetBaseDir()}";
        asset.IssuesUrl = "";
        asset.Searchable = "";
        asset.ModifyDate = DateTime.UtcNow.ToString();
        asset.DownloadUrl = $"file://{filepath}";
        asset.Previews = new Array<AssetLib.Preview>();
        asset.DownloadHash = "";
        return asset;
    }

    async void PluginDirectoryImport(string filepath) {
        // "E:\Projects\Godot\EditorPlugins\addons\data_editor\plugin.cfg"
        string addonPath = filepath.GetBaseDir().NormalizePath();
        string addonName = addonPath.GetFile();
        // "E:\Projects\Godot\EditorPlugins\addons\data_editor"
        string zipFile = $"{CentralStore.Settings.CachePath}/AssetLib/local-{CentralStore.Settings.LocalAddonCount}-{addonName}.zip";
        using(var fh = new FileStream(zipFile, FileMode.Create)) {
            using (var afh = new ZipArchive(fh, ZipArchiveMode.Create)) {

                foreach (string entry in Directory.EnumerateFileSystemEntries(addonPath,"*",System.IO.SearchOption.AllDirectories)) {
                    if (entry == "." || entry == "..")
                        continue;
                    if (entry.EndsWith(".import"))
                        continue;
                    if (Directory.Exists(entry))
                        continue;
                    
                    var zipPath = $"addons/{addonName}".Join(entry.Substring(addonPath.Length+1));
                    afh.CreateEntryFromFile(entry, zipPath);
                }
            }
        }
        AssetPlugin plgn = new AssetPlugin();
        plgn.Asset = CreateAssetDirectory(filepath, true);
        plgn.Location = zipFile;
        AppDialogs.AddonInstaller.ShowDialog(plgn);
        while (AppDialogs.AddonInstaller.Visible)
            await this.IdleFrame();
        CentralStore.Plugins.Add(plgn);
        CentralStore.Settings.LocalAddonCount++;
        CentralStore.Instance.SaveDatabase();
    }

    void TemplateDirectoryImport(string filepath) {
        string templatePath = filepath.GetBaseDir().NormalizePath();
        string templateName = templatePath.GetFile();
        string zipFile = $"{CentralStore.Settings.CachePath}/AssetLib/local-{CentralStore.Settings.LocalAddonCount}-{templateName}.zip";
        using(var fh = new FileStream(zipFile, FileMode.Create)) {
            using(var afh = new ZipArchive(fh, ZipArchiveMode.Create)) {
                foreach (string entry in Directory.EnumerateFileSystemEntries(templatePath, "", System.IO.SearchOption.AllDirectories)) {
                    if (entry == "." || entry == "..")
                        continue;
                    if (entry.EndsWith(".import"))
                        continue;
                    if (Directory.Exists(entry))
                        continue;
                    
                    var zipPath = entry.Substring(templatePath.Length+1);
                    afh.CreateEntryFromFile(entry, zipPath);
                }
            }
        }
        AssetProject prj = new AssetProject();
        prj.Asset = CreateAssetDirectory(filepath, false);
        prj.Location = zipFile;
        CentralStore.Templates.Add(prj);
        CentralStore.Settings.LocalAddonCount++;
        CentralStore.Instance.SaveDatabase();
    }

    async void AssetZipImport(string filepath, bool is_plugin) {
        string zipFile = filepath.NormalizePath();
        string zipName = zipFile.GetFile().BaseName();
        string newZipFile = $"{CentralStore.Settings.CachePath}/AssetLib/local-{CentralStore.Settings.LocalAddonCount}-{zipName}.zip";
        SFile.Copy(zipFile, newZipFile);
        AssetLib.Asset asset = CreateAssetZip(filepath, is_plugin);
        if (is_plugin) {
            AssetPlugin plgn = new AssetPlugin();
            plgn.Asset = asset;
            plgn.Location = newZipFile;
            AppDialogs.AddonInstaller.ShowDialog(plgn);
            while (AppDialogs.AddonInstaller.Visible)
                await this.IdleFrame();
            CentralStore.Plugins.Add(plgn);
        } else {
            AssetProject prj = new AssetProject();
            prj.Asset = asset;
            prj.Location = newZipFile;
            CentralStore.Templates.Add(prj);
        }
        CentralStore.Settings.LocalAddonCount++;
        CentralStore.Instance.SaveDatabase();
    }

	private async Task Configure(bool projectsOnly)
	{
		AppDialogs.BusyDialog.UpdateHeader(Tr("Gathering information from GodotEngine Assetlib..."));
		AppDialogs.BusyDialog.UpdateByline(Tr("Connecting..."));
		AppDialogs.BusyDialog.ShowDialog();

        string url = (string)_mirrorSite.GetItemMetadata(_mirrorSite.Selected);

		AssetLib.AssetLib.Instance.Connect("chunk_received", this, "OnChunkReceived");
		var task = AssetLib.AssetLib.Instance.Configure(url,projectsOnly);
		while (!task.IsCompleted)
		{
			await this.IdleFrame();
		}

		AssetLib.AssetLib.Instance.Disconnect("chunk_received", this, "OnChunkReceived");

		AppDialogs.BusyDialog.UpdateHeader(Tr("Processing Data from GodotEngine Assetlib..."));
		AppDialogs.BusyDialog.UpdateByline(Tr("Processing..."));

		_category.Clear();
        _category.AddItem("All", 0);
		AssetLib.ConfigureResult configureResult = task.Result;

        if (configureResult == null) {
            PaginatedListing pl = _addonsBtn.Pressed ? _plAddons : _plTemplates;
            pl.ClearResults();
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage(Tr("Asset Library"),string.Format(Tr("Unable to connect to {0}."),url));
            return;
        }

		foreach (AssetLib.CategoryResult category in configureResult.Categories)
		{
			_category.AddItem(category.Name, category.Id.ToInt());
		}
        lastConfigureRequest = DateTime.Now;
	}

    private string[] GetSupport() {
        Array<string> support = new Array<string>();
        if (_supportPopup.IsItemChecked(0))
            support.Add("official");
        if (_supportPopup.IsItemChecked(1))
            support.Add("community");
        if (_supportPopup.IsItemChecked(2))
            support.Add("testing");
        string[] asupport = new string[support.Count];
        foreach(string t in support)
            asupport[support.IndexOf(t)] = t;
        return asupport;
    }

    public string GetGodotVersion()
    {
        return _godotVersion.GetItemText(_godotVersion.Selected).ToLower();
    }

	private async Task UpdatePaginatedListing(PaginatedListing pl)
	{
        if (pl == _plmAddons || pl == _plmTemplates) {
            pl.ClearResults();
            if (pl == _plmAddons)
                pl.UpdateAddons();
            else
                pl.UpdateTemplates();
        } else {
            AppDialogs.BusyDialog.UpdateHeader(Tr("Getting search results..."));
            AppDialogs.BusyDialog.UpdateByline(Tr("Connecting..."));
            AppDialogs.BusyDialog.ShowDialog();

            bool projectsOnly = (pl == _plTemplates);
            int sortBy = _sortBy.Selected;
            int categoryId = _category.GetSelectedId();
            string filter = _searchField.Text;
            string url = (string)_mirrorSite.GetItemMetadata(_mirrorSite.Selected);

            Task<AssetLib.QueryResult> stask = AssetLib.AssetLib.Instance.Search(url, 
                projectsOnly ? _pltCurrentPage : _plaCurrentPage,
                    GetGodotVersion(), projectsOnly, sortBy, GetSupport(), categoryId, filter);
            while (!stask.IsCompleted)
                await this.IdleFrame();

            if (stask.Result == null) {
                pl.ClearResults();
                AppDialogs.BusyDialog.HideDialog();
                AppDialogs.MessageDialog.ShowMessage(Tr("Asset Library"),
                    string.Format(Tr("Unable to connect to {0}."),url));
                return;
            }

            AppDialogs.BusyDialog.UpdateByline(Tr("Parsing results..."));
            pl.UpdateResults(stask.Result);
            AppDialogs.BusyDialog.HideDialog();
            lastSearchRequest = DateTime.Now;
        }
	}
}
