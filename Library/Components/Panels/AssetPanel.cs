using System.Collections.Generic;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;

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
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

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
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions

	private void SetupAddonSearch()
	{
		_searchLibrary.Visible = true;
		_manageDownloaded.Visible = false;
		_plAddons.Visible = true;
		_plTemplates.Visible = false;
	}

	private void SetupTemplatesSearch()
	{
		_searchLibrary.Visible = true;
		_manageDownloaded.Visible = false;
		_plAddons.Visible = false;
		_plTemplates.Visible = true;
	}

	private void SetupManage()
	{
		_searchLibrary.Visible = false;
		_manageDownloaded.Visible = true;
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}
