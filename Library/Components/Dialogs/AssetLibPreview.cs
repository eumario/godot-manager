using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.AssetLib;
using GodotManager.Library.Managers;
using GodotManager.Library.Utility;
using FileAccess = Godot.FileAccess;

// namespace
namespace GodotManager.Library.Components.Dialogs;

[Tool]
[SceneNode("res://Library/Components/Dialogs/AssetLibPreview.tscn")]
public partial class AssetLibPreview : AcceptDialog
{
	#region Node Paths

	[NodePath] private TextureRect? _icon;
	[NodePath] private Label? _pluginName;
	[NodePath] private Label? _category;
	[NodePath] private LinkButton? _author;
	[NodePath] private Label? _license;
	[NodePath] private Label? _version;
	[NodePath] private LinkButton? _assetId;
	[NodePath] private Label? _godotVersion;
	[NodePath] private Button? _viewFiles;
	[NodePath] private RichTextLabel? _description;
	[NodePath] private VBoxContainer? _screenshots;
	[NodePath] private TextureRect? _preview;
	[NodePath] private TextureRect? _playButton;
	[NodePath] private TextureRect? _missingThumbnail;
	[NodePath] private HBoxContainer? _thumbnails;
	#endregion
	
	#region Private Variables
	private Asset? _asset;
	private readonly Queue<ImageDownloader> _imageQueue = [];
	private readonly Dictionary<ImageDownloader, TextureRect> _previewList = [];
	private string _videoUrl = string.Empty;
	#endregion
	
	#region Public Variables
	public Asset? Asset
	{ 
		get => _asset;
		set
		{
			_asset = value;
			if (_asset == null) return;
			if (!this.IsNodesReady()) return;
			Task.Run(async () =>
			{
				var uri = new Uri(_asset.IconUrl);
				var iconPath = Path.Combine(Database.Settings.CachePath, "images",
					_asset.AssetId + (uri.AbsolutePath.EndsWith(".svg") ? ".svg" : ""));
				var texture = await Util.LoadImage(iconPath);
				Util.RunInMainThread(() => _icon!.Texture = texture);
					
			});
			_pluginName!.Text = _asset.Title;
			_pluginName!.TooltipText = _asset.Title;
			_category!.Text = _asset.Category;
			_author!.Text = _asset.Author;
			_author!.Uri = $"https://godotengine.org/asset-library/asset?user={_asset.Author.URIEncode()}";
			_license!.Text = _asset.Cost;
			_version!.Text = _asset.Version;
			_assetId!.Text = _asset.AssetId;
			_assetId!.Uri = $"https://godotengine.org/asset-library/asset/{_asset.AssetId}";
			_godotVersion!.Text = _asset.GodotVersion;
			_description!.Text = _asset.Description;
			LoadThumbnails();
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		AddCancelButton("Close");
		_playButton!.GuiInput += @event =>
		{
			if (@event is not InputEventMouseButton iemb) return;
			if (iemb.Pressed && iemb.ButtonIndex == MouseButton.Left)
				OS.ShellOpen(_videoUrl);
		};

		_viewFiles!.Pressed += () =>
		{
			OS.ShellOpen(Asset.BrowseUrl);
		};
		
		Asset = _asset;
	}
	#endregion

	#region Private Support Function
	private async void LoadThumbnails()
	{
		var baseDir = Path.Combine(Database.Settings.CachePath, "images", "thumbnails");
		if (!DirAccess.DirExistsAbsolute(baseDir))
			DirAccess.MakeDirRecursiveAbsolute(baseDir);

		if (Asset!.Previews.Count == 0)
		{
			// No Previews, hide preview stuff...
			_screenshots!.Visible = false;
			return;
		}
		foreach (var (preview,i) in Asset!.Previews.WithIndex())
		{
			var outputPath = Path.Combine(baseDir, preview.PreviewId);
			if (FileAccess.FileExists(outputPath))
			{
				var nxtctl = new TextureRect();
				nxtctl.Name = $"Preview{i+1}";
				nxtctl.Texture = await Util.LoadImage(outputPath);
				nxtctl.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
				nxtctl.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				nxtctl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				nxtctl.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
				nxtctl.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
				nxtctl.GuiInput += @event =>
				{
					if (@event is not InputEventMouseButton iemb) return;
					if (!iemb.Pressed || iemb.ButtonIndex != MouseButton.Left) return;
					_preview!.Texture = nxtctl.Texture;
					_playButton!.Visible = (preview.Type == "video");
					_videoUrl = preview.Link;
				};
				_thumbnails!.AddChild(nxtctl);
				if (nxtctl.Name == "Preview1")
				{
					_preview!.Texture = nxtctl.Texture;
					_playButton!.Visible = (preview.Type == "video");
					_videoUrl = preview.Link;
				}
				continue;
			}
			var url = string.IsNullOrEmpty(preview.Thumbnail) ? preview.Link : preview.Thumbnail;
			var dld = new ImageDownloader(new Uri(url), outputPath: outputPath);
			_imageQueue.Enqueue(dld);
			var txtctl = new TextureRect();
			txtctl.Name = $"Preview{i+1}";
			txtctl.Texture = GD.Load<Texture2D>("res://Assets/Icons/svg/icon_thumbnail_wait.svg");
			txtctl.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			txtctl.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			txtctl.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			txtctl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			txtctl.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			txtctl.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
			txtctl.GuiInput += @event =>
			{
				if (@event is not InputEventMouseButton iemb) return;
				if (iemb is not { Pressed: true, ButtonIndex: MouseButton.Left }) return;
				_preview!.Texture = txtctl.Texture;
				_playButton!.Visible = (preview.Type == "video");
				_videoUrl = preview.Link;
			};
			_thumbnails!.AddChild(txtctl);
			_previewList[dld] = txtctl;
			dld.DownloadCompleted += OnImageDownloaded;
		}

		if (_imageQueue.Count > 0)
			await _imageQueue.Dequeue().DownloadImage();
	}

	private async void OnImageDownloaded(object? sender, string path)
	{
		if (sender is not ImageDownloader dld) return;
		if (!_previewList.TryGetValue(dld, out var txtctl)) return;
		Util.RunInMainThread(async () =>
		{
			var img = await Util.LoadImage(path);
			txtctl.Texture = img;
			if (txtctl.Name != "Preview1") return;
			_preview!.Texture = img;
			_playButton!.Visible = Asset!.Previews[0].Type == "video";
			_videoUrl = Asset.Previews[0].Link;
		});
		if (_imageQueue.Count == 0) return;
		var img = _imageQueue.Dequeue();
		await img.DownloadImage();
	}
	
	
	#endregion
}
