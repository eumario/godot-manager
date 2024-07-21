using Godot;
using System;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.AssetLib;

public partial class AssetLibEntry : ColorRect
{
	#region Signals
	#endregion
	
	#region Node Paths
	[NodePath] private TextureRect _icon;
	[NodePath] private Label _title;
	[NodePath] private Label _category;
	[NodePath] private Label _license;
	[NodePath] private Label _author;
	[NodePath] private TextureRect _downloaded;
	[NodePath] private TextureRect _updateAvailable;
	#endregion

	#region Singletons
	#endregion

	#region Resources
	#endregion
	
	#region Private Variables
	private AssetResult _asset;
	private bool _hasUpdate;
	private bool _isInstalled;
	private Texture2D _assetIcon;
	#endregion
	
	#region Public Variables

	public AssetResult Asset
	{
		get => _asset;
		set
		{
			_asset = value;
			if (value == null) return;
			if (_title != null) _title.Text = Asset.Title;
			if (_category != null) _category.Text = $"Category: {Asset.Category}";
			if (_author != null) _author.Text = $"Author: {Asset.Author}";
			if (_license != null) _license.Text = $"License: {Asset.Cost}";
			
		}
	}

	public bool UpdateAvailable
	{
		get => _hasUpdate;
		set
		{
			_hasUpdate = value;
			if (_updateAvailable != null) _updateAvailable.Visible = value;
		}
	}

	public bool Downloaded
	{
		get => _isInstalled;
		set
		{
			_isInstalled = value;
			if (_downloaded != null) _downloaded.Visible = value;
		}
	}

	public Texture2D Icon
	{
		get => _assetIcon;
		set
		{
			_assetIcon = value;
			if (_icon != null && !_icon.IsQueuedForDeletion()) _icon.Texture = _assetIcon;
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		_downloaded.Visible = false;
		_updateAvailable.Visible = false;
		Asset = _asset;
		Icon = _assetIcon;
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}

