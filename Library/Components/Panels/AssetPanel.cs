using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data;
using GodotManager.Library.Managers;

// namespace
namespace GodotManager.Library.Components.Panels;

public partial class AssetPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private Button _addons;
	[NodePath] private Button _templates;
	[NodePath] private Button _manage;

	[NodePath] private VBoxContainer _searchLibrary;
	[NodePath] private LineEdit _searchField;
	[NodePath] private Button _import;
	[NodePath] private OptionButton _sortBy;
	[NodePath] private OptionButton _category;
	[NodePath] private OptionButton _godotVersion;
	[NodePath] private OptionButton _mirrorSite;
	[NodePath] private Button _support;
	[NodePath] private PopupMenu _supportPopup;
	[NodePath] private PaginatedListing _plAddons;
	[NodePath] private PaginatedListing _plTemplates;

	[NodePath] private VBoxContainer _manageDownloaded;
	[NodePath] private Button _installedAddons;
	[NodePath] private Button _installedTemplates;
	[NodePath] private LineEdit _searchInstalled;
	[NodePath] private PaginatedListing _plmAddons;
	[NodePath] private PaginatedListing _plmTemplates;
	#endregion
	
	#region Private Variables
	private bool _configure = false;
	private DateTime _lastConfigureRequest;
	private DateTime _lastSearchRequest;
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		foreach (var item in Database.AllAssetMirrors())
		{
			var i = _mirrorSite.GetItemCount();
			_mirrorSite.AddItem(item.Name);
			_mirrorSite.SetItemMetadata(i, item.Url);
		}

		var buttonGroup = new ButtonGroup();
		foreach (var button in new List<Button> { _addons, _templates, _manage })
			button.ButtonGroup = buttonGroup;

		buttonGroup = new ButtonGroup();
		foreach (var button in new List<Button> { _installedAddons, _installedTemplates })
			button.ButtonGroup = buttonGroup;

		_addons.Pressed += () => SetupAddonSearch();
		_templates.Pressed += () => SetupTemplatesSearch();
		_manage.Pressed += () => SetupManage();

		_installedAddons.Pressed += () =>
		{
			_plmAddons.Visible = true;
			_plmTemplates.Visible = false;
		};
		_installedTemplates.Pressed += () =>
		{
			_plmAddons.Visible = false;
			_plmTemplates.Visible = true;
		};

		var versions = Database.AllGithubMajorMinorVersions("godot-builds");

		_godotVersion.AddItem("Any");
		foreach (var version in versions)
		{
			if (version is "1.0" or "1.1") continue;
			_godotVersion.AddItem(version);
		}

		GetParent<TabContainer>().TabChanged += OnPageChanged;
		_plAddons.PageChanged += (i) => OnPLPageChanged(_plAddons, i);
		_plTemplates.PageChanged += (i) => OnPLPageChanged(_plTemplates, i);
	}
	#endregion
	
	#region Event Handlers
	private async void OnPageChanged(long i)
	{
		if (GetParent<TabContainer>().GetCurrentTabControl() != this) return;
		if (DateTime.Now - _lastConfigureRequest >= TimeSpan.FromHours(2))
		{
			await ConfigureAssetLibrary(_templates.ButtonPressed);
			if (_category.GetItemCount() == 1)
				return;
		}

		if (DateTime.Now - _lastSearchRequest >= TimeSpan.FromMinutes(5))
		{
			await UpdateSearchResults(_addons.ButtonPressed ? _plAddons : _plTemplates);
		}
	}
	
	private async void OnPLPageChanged(PaginatedListing listing, int indx)
	{
		var dlg = BusyDialog.FromScene();
		dlg.HeaderText = "Searching Asset Library for results...";
		dlg.BylineText = "Connecting...";
		GetTree().Root.AddChild(dlg);
		dlg.PopupCentered();
		var result = await AssetLibManager.SearchPage(indx);
		dlg.BylineText = "Processing results...";
		listing.UpdateView(result, AssetLibManager.LastQuery.SearchTemplates);
		dlg.QueueFree();
	}
	#endregion
	
	#region Private Support Functions

	private async void SetupAddonSearch()
	{
		_searchLibrary.Visible = true;
		_manageDownloaded.Visible = false;
		_plAddons.Visible = true;
		_plTemplates.Visible = false;
		await ConfigureAssetLibrary(false);
	}

	private async void SetupTemplatesSearch()
	{
		_searchLibrary.Visible = true;
		_manageDownloaded.Visible = false;
		_plAddons.Visible = false;
		_plTemplates.Visible = true;
		await ConfigureAssetLibrary(true);
	}

	private void SetupManage()
	{
		_searchLibrary.Visible = false;
		_manageDownloaded.Visible = true;
	}

	private List<string> GetSupport()
	{
		List<string> support = [];
		if (_supportPopup.IsItemChecked(0)) support.Add("official");
		if (_supportPopup.IsItemChecked(1)) support.Add("community");
		if (_supportPopup.IsItemChecked(2)) support.Add("testing");
		return support;
	}

	private string GetGodotVersion() => _godotVersion.GetItemText(_godotVersion.Selected).ToLower();

	private async Task ConfigureAssetLibrary(bool templates = false)
	{
		var dlg = BusyDialog.FromScene();
		dlg.HeaderText = "Gathering information from GodotEngine Assetlib...";
		dlg.BylineText = "Connecting...";
		GetTree().Root.AddChild(dlg);
		dlg.PopupCentered();
		var url = _mirrorSite.GetItemMetadata(_mirrorSite.Selected).AsString();
		var res = await AssetLibManager.Configure(url, _templates.ButtonPressed);
		_category.Clear();
		_category.AddItem("All", 0);
		dlg.BylineText = $"Processing 0/{res.Categories.Count}";
		foreach (var (category, indx) in res.Categories.WithIndex())
		{
			dlg.BylineText = $"Processing {indx}/{res.Categories.Count}";
			_category.AddItem(category.Name, category.Id.ToInt());
		}

		_lastConfigureRequest = DateTime.Now;
		dlg.QueueFree();
	}

	private async Task UpdateSearchResults(PaginatedListing listing)
	{
		if (listing == _plmAddons || listing == _plmTemplates)
		{
			// Handle Loading Downloaded Templates and Addons.
			return;
		}
		
		var dlg = BusyDialog.FromScene();
		dlg.HeaderText = "Searching Asset Library for results...";
		dlg.BylineText = "Connecting...";
		GetTree().Root.AddChild(dlg);
		dlg.PopupCentered();

		var projectsOnly = (listing == _plTemplates);
		var sortBy = _sortBy.Selected;
		var categoryId = _category.GetSelectedId();
		var filter = _searchField.Text;
		var url = _mirrorSite.GetItemMetadata(_mirrorSite.Selected).AsString();

		var result = await AssetLibManager.Search(url, GetGodotVersion(), projectsOnly, sortBy, GetSupport(),
			categoryId, filter);

		dlg.BylineText = "Processing results...";
		listing.UpdateView(result, projectsOnly);
		dlg.QueueFree();
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}
