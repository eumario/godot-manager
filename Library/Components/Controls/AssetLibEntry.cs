using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data.POCO.AssetLib;
using GodotManager.Library.Managers;

namespace GodotManager.Library.Components.Controls;

[SceneNode("res://Library/Components/Controls/AssetLibEntry.tscn")]
public partial class AssetLibEntry : ColorRect
{
	#region Signals
	#endregion
	
	#region Node Paths
	[NodePath] private TextureRect? _icon;
	[NodePath] private Label? _title;
	[NodePath] private Label? _category;
	[NodePath] private Label? _license;
	[NodePath] private Label? _author;
	[NodePath] private TextureRect? _downloaded;
	[NodePath] private TextureRect? _updateAvailable;
	#endregion

	#region Singleton

	[Singleton] private SignalBus _signalBus;
	#endregion

	#region Resources
	#endregion
	
	#region Private Variables
	private AssetResult? _asset;
	private Asset? _assetInfo;
	private bool _hasUpdate;
	private bool _isInstalled;
	private Texture2D? _assetIcon;
	private bool _dlgShown = false;
	#endregion
	
	#region Public Variables
	[Export]
	public bool DialogShown { get => _dlgShown; set => _dlgShown = value; }

	public AssetResult? Asset
	{
		get => _asset;
		set
		{
			_asset = value;
			_dlgShown = false;
			Downloaded = false;
			if (value == null) return;
			if (this.IsNodesReady())
			{
				_title!.Text = value.Title;
				_category!.Text = $"Category: {value.Category}";
				_author!.Text = $"Author: {Asset?.Author ?? "Unknown Author"}";
				_license!.Text = $"License: {Asset?.Cost ?? "Unknown License"}";
			}
		}
	}

	public bool UpdateAvailable
	{
		get => _hasUpdate;
		set
		{
			_hasUpdate = value;
			if (this.IsNodesReady()) _updateAvailable!.Visible = value;
		}
	}

	public bool Downloaded
	{
		get => _isInstalled;
		set
		{
			_isInstalled = value;
			if (this.IsNodesReady()) _downloaded!.Visible = value;
		}
	}

	public Texture2D? Icon
	{
		get => _assetIcon;
		set
		{
			_assetIcon = value;
			if (this.IsNodesReady() && !_icon!.IsQueuedForDeletion()) _icon.Texture = _assetIcon;
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		_downloaded!.Visible = false;
		_updateAvailable!.Visible = false;
		Asset = _asset;
		Icon = _assetIcon;
	}

	public override async void _GuiInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton iembEvent) return;
		if (iembEvent is not { Pressed: true, ButtonIndex: MouseButton.Left }) return;
		if (_asset!.AssetId.StartsWith("local-")) return;
		if (_dlgShown) return;
		_dlgShown = true;
		_assetInfo = await AssetLibManager.GetAsset(_asset.AssetId);
		var dlg = SceneNode<AssetLibPreview>.FromScene();
		dlg.Asset = _assetInfo;
		GetTree().Root.AddChild(dlg);
		dlg.Show();
		dlg.Canceled += () =>
		{
			_dlgShown = false;
			dlg.QueueFree();
		};
		dlg.Confirmed += () =>
		{
			// Start download
			_dlgShown = false;
			var box = SceneNode<DownloadBox>.FromScene();
			box.Asset = _assetInfo;
			box.Icon = _assetIcon;
			_signalBus.EmitSignal(SignalBus.SignalName.DownloadBoxCreated, box);
			dlg.QueueFree();
		};
		dlg.CloseRequested += () =>
		{
			_dlgShown = false;
			dlg.QueueFree();
		};
	}

	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}