using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.AssetLib;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Managers;
using GodotManager.Library.Utility;

// namespace

namespace GodotManager.Library.Components.Controls;

public partial class PaginatedListing : ScrollContainer
{
	#region Signals
	[Signal] public delegate void PageChangedEventHandler(int page);
	#endregion
	
	#region Node Paths
	[NodePath] private PaginationNav _topPageCount = null;
	[NodePath] private GridContainer _itemList = null;
	[NodePath] private PaginationNav _bottomPageCount = null;
	#endregion
	
	#region Private Variables
	private readonly Queue<ImageDownloader> ImageQueue = [];
	private readonly Dictionary<ImageDownloader, AssetLibEntry> QueueDict = [];
	#endregion
	
	#region Public Variables
	#endregion
	
	#region Resources
	[Resource("res://Library/Components/Controls/AssetLibEntry.tscn")]
	private PackedScene _assetEntry;
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		_topPageCount.PageChanged += (i) => EmitSignal(SignalName.PageChanged, i);
		_bottomPageCount.PageChanged += (i) => EmitSignal(SignalName.PageChanged, i);
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions

	public async void UpdateView(QueryResult result, bool template)
	{
		// foreach (var entry in _itemList.GetChildren())
		// {
		// 	if (entry is not AssetLibEntry) continue;
		// 	entry.QueueFree();
		// }
		if (_itemList.GetChildCount() == 0)
		{
			for (var i = 0; i < result.TotalItems; i++)
			{
				var ale = _assetEntry.Instantiate<AssetLibEntry>();
				_itemList.AddChild(ale);
			}
		}

		for (var i = 0; i < result.TotalItems; i++)
		{
			var ale = _itemList.GetChild<AssetLibEntry>(i);
			ale.Visible = false;
			ale.Icon = null;
		}

		_topPageCount.Visible = result.Pages > 1;
		_bottomPageCount.Visible = result.Pages > 1;
		
		_topPageCount.UpdateConfig(result.Pages);
		_bottomPageCount.UpdateConfig(result.Pages);
		_topPageCount.SetPage(result.Page);
		_bottomPageCount.SetPage(result.Page);

		ScrollVertical = 0;
		foreach (var (asset, index) in result.Result.WithIndex())
		{
			var ale = _itemList.GetChild<AssetLibEntry>(index);
			ale.Asset = asset;
			var hasAsset = template ? Database.HasTemplate(asset.AssetId) : Database.HasPlugin(asset.AssetId);
			var inAsset = (AssetGeneral)(template ? Database.GetTemplateById(asset.AssetId) : Database.GetPluginById(asset.AssetId));
			if (inAsset != null)
			{
				if (inAsset.Asset.VersionString != asset.VersionString ||
				    inAsset.Asset.Version != asset.Version ||
				    inAsset.Asset.ModifyDate != asset.ModifyDate)
					ale.UpdateAvailable = true;
				else
					ale.Downloaded = true;
			}

			ale.Visible = true;
			if (!string.IsNullOrEmpty(asset.IconUrl))
			{
				var uri = new Uri(asset.IconUrl);
				var iconPath = Path.Combine(Database.Settings.CachePath, "images",
					asset.AssetId + (uri.AbsolutePath.EndsWith(".svg") ? ".svg" : ""));

				if (!FileUtil.WildcardExists(iconPath))
				{
					var dld = new ImageDownloader(new Uri(asset.IconUrl), outputPath: iconPath);
					ImageQueue.Enqueue(dld);
					QueueDict[dld] = ale;
					dld.DownloadCompleted += OnImageDownloaded;
				}
				else
				{
					var icon = await Util.LoadImage(FileUtil.WildcardFilename(iconPath));
					ale.Icon = icon ?? GD.Load<Texture2D>("res://Assets/Icons/svg/missing_icon.svg");
				}
			}
			else
				ale.Icon = GD.Load<Texture2D>("res://Assets/Icons/svg/missing_icon.svg");
		}

		for (var i = 0; i < 3; i++)
		{
			if (ImageQueue.Count == 0) break;
			await ImageQueue.Dequeue().DownloadImage();
		}
	}

	private async void OnImageDownloaded(object? sender, string path)
	{
		var dld = sender as ImageDownloader;
		if (!QueueDict.ContainsKey(dld)) return; // Somehow we have no Image Downloader?????
		var ale = QueueDict[dld];
		
		Util.RunInMainThread(async () =>
		{
			if (File.Exists(path))
			{
				var icon = await Util.LoadImage(path) ?? GD.Load<Texture2D>("res://Assets/Icons/svg/missing_icon.svg");
				ale.Icon = icon;
			}
			else
				ale.Icon = GD.Load<Texture2D>("res://Assets/Icons/svg/missing_icon.svg");
		});

		QueueDict.Remove(dld);
		dld.Dispose();
		if (ImageQueue.Count == 0) return;
		dld = ImageQueue.Dequeue();
		if (dld == null) return;
		await dld.DownloadImage();
	}
	#endregion
}