using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using Uri = System.Uri;
using File = System.IO.File;

public class AssetLibPreview : ReferenceRect
{
#region Node Paths
    [NodePath("PC/CC/P/VB/MC/TitleBarBG/HB/Title")]
    Label _DialogTitle = null;
    
    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/Icon")]
    TextureRect _Icon = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/PluginInfo/PluginName")]
    Label _PluginName = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/PluginInfo/GC/Category")]
    Label _Category = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/PluginInfo/GC/Author")]
    Label _Author = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/PluginInfo/GC/License")]
    Label _License = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/PluginInfo/GC/Version")]
    Label _Version = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/PC/SC/Description")]
    RichTextLabel _Description = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/ScreenShots/Preview")]
    TextureRect _Preview = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/ScreenShots/Preview/PlayButton")]
    TextureRect _PlayButton = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/ScreenShots/Preview/MissingThumbnails")]
    TextureRect _MissingThumbnails = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/ScreenShots/PC/SC/Thumbnails")]
    HBoxContainer _Thumbnails = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/DownloadBtn")]
    Button _Download = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CloseBtn")]
    Button _Close = null;
#endregion

#region Private Variables
    private string sIconPath;
    private DownloadQueue dlq = null;
    private ImageDownloader dldIcon = null;
    private Array<ImageDownloader> dldPreviews = null;
    private AssetLib.Asset _asset;

    private Array<string> Templates = new Array<string> {"Templates", "Projects", "Demos"};
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        dlq = new DownloadQueue();
        AddChild(dlq);
        this.OnReady();
        dldPreviews = new Array<ImageDownloader>();
    }

    [SignalHandler("meta_clicked", nameof(_Description))]
    void OnDescription_MetaClicked(object meta) {
        OS.ShellOpen((string)meta);
    }

    [SignalHandler("pressed", nameof(_Close))]
    void OnClosePressed() {
        Visible = false;
    }
    
    [SignalHandler("pressed", nameof(_Download))]
    async void OnDownloadPressed() {
        // LOGIC: If asset is an Addon, after download is complete, popup Installer Reference creator
        // to allow user to select what files to be installed when selecting this addon.
        // If asset is a Template/Project/Demo, this will be added to templates for creating New Projects from.
        // Two new central repositories needed for Addons / Projects when saving to the user's computer.

        AppDialogs.DownloadAddon.Asset = _asset;
        AppDialogs.DownloadAddon.LoadInformation();
        AppDialogs.DownloadAddon.Connect("download_complete", this, "OnDownloadAddonCompleted");
        await AppDialogs.DownloadAddon.StartDownload();
    }

    void OnDownloadAddonCompleted(AssetLib.Asset asset, AssetProject ap, AssetPlugin apl) {
        AppDialogs.DownloadAddon.Disconnect("download_complete", this, "OnDownloadAddonCompleted");
        if (apl != null) {
            AppDialogs.AddonInstaller.ShowDialog(apl);
        }
    }

    public void ShowDialog(AssetLib.Asset asset) {
        _DialogTitle.Text = asset.Title;
        _PluginName.Text = asset.Title;
        _Author.Text = asset.Author;
        _Category.Text = asset.Category;
        _Version.Text = asset.VersionString;
        _License.Text = asset.Cost;
        _Description.BbcodeText = $"[table=1][cell][color=lime]Support[/color][/cell][cell][color=aqua][url={asset.BrowseUrl}]Homepage[/url][/color][/cell][cell][color=aqua][url={asset.IssuesUrl}]Issue/Support Page[/url][/color][/cell][/table]\n\n{asset.Description}";
        _asset = asset;
        
        Uri uri = new Uri(asset.IconUrl);
        sIconPath = $"user://cache/images/{asset.AssetId}{uri.AbsolutePath.GetExtension()}";
        
        if (!File.Exists(sIconPath.GetOSDir().NormalizePath())) {
            dldIcon = new ImageDownloader(asset.IconUrl, sIconPath);
            dlq.Push(dldIcon);
        } else {
            if (sIconPath.EndsWith(".gif")) {
                GifTexture gif = new GifTexture(sIconPath);
                _Icon.Texture = gif;
            } else {
                Texture icon = Util.LoadImage(sIconPath);
                if (icon == null)
                    _Icon.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
                else
                    _Icon.Texture = icon;
            }
        }
        _Preview.Texture = GD.Load<Texture>("res://Assets/Icons/icon_thumbnail_wait.svg");
        _MissingThumbnails.Visible = false;
        _PlayButton.Visible = false;
        
        foreach(TextureRect rect in _Thumbnails.GetChildren()) {
            _Thumbnails.RemoveChild(rect);
            rect.QueueFree();
        }

        for (int i = 0; i < asset.Previews.Count; i++) {
            TextureRect preview = new TextureRect();
            preview.RectMinSize = new Vector2(120,120);
            preview.Texture = GD.Load<Texture>("res://Assets/Icons/icon_thumbnail_wait.svg");
            preview.Expand = true;
            preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            preview.Connect("gui_input", this, "OnGuiInput_Preview", new Array { preview });
            _Thumbnails.AddChild(preview);
            Uri tnUri = new Uri(asset.Previews[i].Thumbnail);
            string iconPath = $"user://cache/images/{asset.AssetId}-{i}-{asset.Previews[i].PreviewId}{tnUri.AbsolutePath.GetExtension()}";
            if (!File.Exists(iconPath.GetOSDir().NormalizePath())) {
                ImageDownloader dld = new ImageDownloader(asset.Previews[i].Thumbnail, iconPath);
                dldPreviews.Add(dld);
                preview.SetMeta("iconPath", iconPath);
                dlq.Push(dld);
            } else {
                if (iconPath.EndsWith(".gif")) {
                    GifTexture gif = new GifTexture(iconPath);
                    preview.Texture = gif;
                } else {
                    Texture icon = Util.LoadImage(iconPath);
                    if (icon == null)
                        preview.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
                    else
                        preview.Texture = icon;
                }
            }

            preview.SetMeta("url",asset.Previews[i].Link);
        }
        
        if (_Thumbnails.GetChildCount() > 0) {
            if (_Thumbnails.GetChild(0) is TextureRect fp && fp.Texture != null) {
                if (fp.Texture.ResourcePath != "res://Assets/Icons/icon_thumbnail_wait.svg") {
                    UpdatePreview(fp);
                }
            }
        } else {
            _Preview.Texture = null;
            _MissingThumbnails.Visible = true;
        }

        if (Templates.IndexOf(asset.Category) != -1) {
            _Download.Disabled = CentralStore.Instance.HasTemplate(asset.Title);
        } else {
            _Download.Disabled = CentralStore.Instance.HasPlugin(asset.Title);
        }
        
        dlq.StartDownload();
        Visible = true;
    }

    [SignalHandler("gui_input", nameof(_PlayButton))]
    void OnGuiInput_PlayButton(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iembEvent) {
            if (iembEvent.Pressed && iembEvent.ButtonIndex == (int)ButtonList.Left) {
                string url = _Preview.GetMeta("url") as string;
                OS.ShellOpen(url);
            }
        }
    }

    void OnGuiInput_Preview(InputEvent inputEvent, TextureRect rect) {
        if (inputEvent is InputEventMouseButton iembEvent) {
            if (iembEvent.Pressed && iembEvent.ButtonIndex == (int)ButtonList.Left)
			{
				UpdatePreview(rect);
			}
		}
    }

	private void UpdatePreview(TextureRect rect)
	{
		_Preview.Texture = rect.Texture;
		_Preview.SetMeta("url", rect.GetMeta("url"));
		Uri tnUri = new Uri(rect.GetMeta("url") as string);
        _MissingThumbnails.Visible = false;
		if (tnUri.Host.IndexOf("youtube.com") != -1)
		{
			_PlayButton.Visible = true;
		}
		else
		{
			_PlayButton.Visible = false;
		}
	}

    [SignalHandler("download_completed", nameof(dlq))]
	void OnImageDownloaded(ImageDownloader dld) {
        if (dld == dldIcon) {
            if (sIconPath.EndsWith(".gif")) {
                GifTexture gif = new GifTexture(sIconPath);
                _Icon.Texture = gif;
            } else {
                Texture icon = Util.LoadImage(sIconPath);
                if (icon == null)
                    _Icon.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
                else
                    _Icon.Texture = icon;
            }
        } else {
            if (dldPreviews.Contains(dld))
			{
				int indx = dldPreviews.IndexOf(dld);
				UpdateThumbnail(indx);
                if (indx == 0) {
                    UpdatePreview(_Thumbnails.GetChild(indx) as TextureRect);
                }
			}
		}
    }

    [SignalHandler("queue_finished", nameof(dlq))]
    void OnQueueFinished() {
        dldPreviews.Clear();
    }

	private void UpdateThumbnail(int indx)
	{
		TextureRect preview = _Thumbnails.GetChild(indx) as TextureRect;
		string iconPath = preview.GetMeta("iconPath") as string;
		if (File.Exists(iconPath.GetOSDir().NormalizePath()))
		{
            if (iconPath.EndsWith(".gif")) {
                GifTexture gif = new GifTexture(iconPath);
                preview.Texture = gif;
            } else {
                Texture icon = Util.LoadImage(iconPath);
                if (icon == null)
                    icon = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
                preview.Texture = icon;
            }
		}
		else
		{
			preview.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
		}
	}
}
