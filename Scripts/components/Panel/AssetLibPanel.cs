using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using TimeSpan = System.TimeSpan;
using DateTime = System.DateTime;

public class AssetLibPanel : Panel
{
#region Nodes Path
    #region Search Switcher
    [NodePath("VC/MC/HC/PC/HC/Addons")]
    Button _addonsBtn = null;

    [NodePath("VC/MC/HC/PC/HC/Templates")]
    Button _templatesBtn = null;
    #endregion

    #region Search Fields
    [NodePath("VC/HC/SearchField")]
    LineEdit _searchField = null;

    [NodePath("VC/HC/Import")]
    Button _import = null;

    [NodePath("VC/HC2/SortBy")]
    OptionButton _sortBy = null;

    [NodePath("VC/HC2/Category")]
    OptionButton _category = null;

    [NodePath("VC/HC2/MirrorSite")]
    OptionButton _mirrorSite = null;

    [NodePath("VC/HC2/Support")]
    Button _support = null;

    [NodePath("VC/HC2/Support/SupportPopup")]
    PopupMenu _supportPopup = null;
    #endregion

    #region Paginated Listings for Addons and Templates
    [NodePath("VC/VC/plAddons")]
    PaginatedListing _plAddons = null;

    [NodePath("VC/VC/plTemplates")]
    PaginatedListing _plTemplates = null;
    #endregion

    #region Timers
    [NodePath("ExecuteDelay")]
    Timer _executeDelay = null;
    #endregion
#endregion

#region Private Variables
    int _plaCurrentPage = 0;
    int _pltCurrentPage = 0;
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
    }

    [SignalHandler("pressed", nameof(_import))]
    void OnImportPressed() {
        // TODO: Implement Importing Addons/Plugins/Projects that are either just folders that need to be zipped up, or a zip file that isn't on a website, but stored locally.
    }

    [SignalHandler("id_pressed", nameof(_supportPopup))]
    async void OnSupportPopup_IdPressed(int id) {
        _supportPopup.SetItemChecked(id, !_supportPopup.IsItemChecked(id));
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons: _plTemplates);
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
        _plTemplates.Visible = false;
        _plAddons.Visible = true;
        await Configure(false);
        await UpdatePaginatedListing(_plAddons);
    }

    [SignalHandler("pressed", nameof(_templatesBtn))]
    async void OnTemplatesPressed() {
        _addonsBtn.Pressed = false;
        _plTemplates.Visible = true;
        _plAddons.Visible = false;
        await Configure(true);
        await UpdatePaginatedListing(_plTemplates);
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

	private async Task Configure(bool projectsOnly)
	{
		AppDialogs.BusyDialog.UpdateHeader("Gathering information from GodotEngine Assetlib...");
		AppDialogs.BusyDialog.UpdateByline("Connecting...");
		AppDialogs.BusyDialog.ShowDialog();

        string url = (string)_mirrorSite.GetItemMetadata(_mirrorSite.Selected);

		AssetLib.AssetLib.Instance.Connect("chunk_received", this, "OnChunkReceived");
		var task = AssetLib.AssetLib.Instance.Configure(url,projectsOnly);
		while (!task.IsCompleted)
		{
			await this.IdleFrame();
		}

		AssetLib.AssetLib.Instance.Disconnect("chunk_received", this, "OnChunkReceived");

		AppDialogs.BusyDialog.UpdateHeader("Processing Data from GodotEngine Assetlib...");
		AppDialogs.BusyDialog.UpdateByline("Processing...");

		_category.Clear();
        _category.AddItem("All", 0);
		AssetLib.ConfigureResult configureResult = task.Result;

        if (configureResult == null) {
            PaginatedListing pl = _addonsBtn.Pressed ? _plAddons : _plTemplates;
            pl.ClearResults();
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Asset Library",$"Unable to connect to {url}.");
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

	private async Task UpdatePaginatedListing(PaginatedListing pl)
	{
		AppDialogs.BusyDialog.UpdateHeader("Getting search results...");
		AppDialogs.BusyDialog.UpdateByline("Connecting...");
        AppDialogs.BusyDialog.ShowDialog();

        bool projectsOnly = (pl == _plTemplates);
        int sortBy = _sortBy.Selected;
        int categoryId = _category.GetSelectedId();
        string filter = _searchField.Text;
        string url = (string)_mirrorSite.GetItemMetadata(_mirrorSite.Selected);

		Task<AssetLib.QueryResult> stask = AssetLib.AssetLib.Instance.Search(url, projectsOnly ? _pltCurrentPage : _plaCurrentPage, projectsOnly, sortBy,
                GetSupport(), categoryId, filter);
		while (!stask.IsCompleted)
			await this.IdleFrame();

        if (stask.Result == null) {
            pl.ClearResults();
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Asset Library",$"Unable to connect to {url}.");
            return;
        }

		AppDialogs.BusyDialog.UpdateByline("Parsing results...");
		pl.UpdateResults(stask.Result);
		AppDialogs.BusyDialog.HideDialog();
        lastSearchRequest = DateTime.Now;
	}
}
