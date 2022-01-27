using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotSharpExtras;

public class AssetLibPanel : Panel
{
#region Nodes Path
    [NodePath("VC/MC/HC/PC/HC/Addons")]
    Button _addonsBtn = null;

    [NodePath("VC/MC/HC/PC/HC/Templates")]
    Button _templatesBtn = null;

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

    [NodePath("VC/VC/plAddons")]
    PaginatedListing _plAddons = null;

    [NodePath("VC/VC/plTemplates")]
    PaginatedListing _plTemplates = null;

    [NodePath("ExecuteDelay")]
    Timer _executeDelay = null;
#endregion

#region Private Variables
    int _plaCurrentPage = 0;
    int _pltCurrentPage = 0;
    string lastSearch = "";
#endregion

    public override void _Ready()
    {
        this.OnReady();
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
        _addonsBtn.Connect("pressed", this, "OnAddonsPressed");
        _templatesBtn.Connect("pressed", this, "OnTemplatesPressed");
        _plAddons.Connect("page_changed", this, "OnPLAPageChanged");
        _plTemplates.Connect("page_changed", this, "OnPLTPageChanged");
        _category.Connect("item_selected", this, "OnCategorySelected");
        _sortBy.Connect("item_selected", this, "OnSortBySelected");
        _searchField.Connect("text_changed", this, "OnSearchField_TextChanged");
        _searchField.Connect("text_entered", this, "OnSearchField_TextEntered");
        _executeDelay.Connect("timeout", this, "OnExecuteDelay_Timeout");
        _import.Connect("pressed", this, "OnImportPressed");
        _support.Connect("pressed", this, "OnSupportPressed");
        _supportPopup.Connect("id_pressed", this, "OnSupportPopup_IdPressed");
        _mirrorSite.Clear();
        _mirrorSite.AddItem("godotengine.org");
        _mirrorSite.AddItem("localhost");
    }

    void OnImportPressed() {
        // TODO: Implement Importing Addons/Plugins/Projects that are either just folders that need to be zipped up, or a zip file that isn't on a website, but stored locally.
    }

    async void OnSupportPopup_IdPressed(int id) {
        _supportPopup.SetItemChecked(id, !_supportPopup.IsItemChecked(id));
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons: _plTemplates);
    }

    void OnSupportPressed() {
        _supportPopup.Popup_(new Rect2(_support.RectGlobalPosition + new Vector2(0,_support.RectSize.y), _supportPopup.RectSize));
    }

    async void OnExecuteDelay_Timeout() {
        if (lastSearch == _searchField.Text)
            return;
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
        lastSearch = _searchField.Text;
    }

    void OnSearchField_TextChanged(string text) {
        _executeDelay.Start();
        //GD.Print($"New Text: {text}");
    }

    async void OnSearchField_TextEntered(string text) {
        if (!_executeDelay.IsStopped())
            _executeDelay.Stop();
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    async void OnCategorySelected(int index) {
        _plaCurrentPage = 0;
        _pltCurrentPage = 0;
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    async void OnSortBySelected(int index) {
        _plaCurrentPage = 0;
        _pltCurrentPage = 0;
        await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
    }

    async void OnPLAPageChanged(int page) {
        _plaCurrentPage = page;
        await UpdatePaginatedListing(_plAddons);
    }

    async void OnPLTPageChanged(int page) {
        _pltCurrentPage = page;
        await UpdatePaginatedListing(_plTemplates);
    }

    async void OnAddonsPressed() {
        _templatesBtn.Pressed = false;
        _plTemplates.Visible = false;
        _plAddons.Visible = true;
        await Configure(false);
        await UpdatePaginatedListing(_plAddons);
    }

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
			await Configure(_templatesBtn.Pressed);

			await UpdatePaginatedListing(_addonsBtn.Pressed ? _plAddons : _plTemplates);
		}
	}

	private async Task Configure(bool projectsOnly)
	{
		AppDialogs.BusyDialog.UpdateHeader("Gathering information from GodotEngine Assetlib...");
		AppDialogs.BusyDialog.UpdateByline("Connecting...");
		AppDialogs.BusyDialog.ShowDialog();

		AssetLib.AssetLib.Instance.Connect("chunk_received", this, "OnChunkReceived");
		var task = AssetLib.AssetLib.Instance.Configure(projectsOnly);
		while (!task.IsCompleted)
		{
			await this.IdleFrame();
		}

		AssetLib.AssetLib.Instance.Disconnect("chunk_received", this, "OnChunkReceived");

		AppDialogs.BusyDialog.UpdateHeader("Processing Data from GodotEngine Assetlib...");
		AppDialogs.BusyDialog.UpdateByline("Processing...");

		_category.Clear();
		AssetLib.ConfigureResult configureResult = task.Result;
		_category.AddItem("All", 0);
		foreach (AssetLib.CategoryResult category in configureResult.Categories)
		{
			_category.AddItem(category.Name, category.Id.ToInt());
		}
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

		Task<AssetLib.QueryResult> stask = AssetLib.AssetLib.Instance.Search(projectsOnly ? _pltCurrentPage : _plaCurrentPage, projectsOnly, sortBy,
                GetSupport(), categoryId, filter);
		while (!stask.IsCompleted)
			await this.IdleFrame();

		AppDialogs.BusyDialog.UpdateByline("Parsing results...");
		pl.UpdateResults(stask.Result);
		AppDialogs.BusyDialog.Visible = false;
	}
}
