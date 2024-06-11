using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using Uri = System.Uri;
using File = System.IO.File;

public class AssetLibPreview : ReferenceRect
{
#region Signals
    [Signal]
    public delegate void installed_addon(bool update);

    [Signal]
    public delegate void uninstalled_addon();

    [Signal]
    public delegate void preview_closed();
#endregion

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
    
    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/PluginInfo/GC/AddonId")]
    Label _AddonId = null;

    [NodePath("PC/CC/P/VB/MCContent/HSC/InfoPanel/HBoxContainer/PluginInfo/GC/GodotVersion")]
    private Label _GodotVersion = null;

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

    [NodePath("PC/CC/P/VB/MCButtons/HB/Uninstall")]
    Button _Uninstall = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/Sep3")]
    Control _Sep3 = null;

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

    async void OnDownloadAddonCompleted(AssetLib.Asset asset, AssetProject ap, AssetPlugin apl) {
        AppDialogs.DownloadAddon.Disconnect("download_complete", this, "OnDownloadAddonCompleted");
        if (apl != null) {
            AppDialogs.AddonInstaller.ShowDialog(apl);
        }
        while (AppDialogs.AddonInstaller.Visible)
            await this.IdleFrame();
        EmitSignal("installed_addon", (_Download.Text != "Download"));
        Visible = false;
        AppDialogs.MessageDialog.ShowMessage((apl == null ? Tr("Template Download") : Tr("Plugin Download")), 
            string.Format(Tr("Download of {0} completed."),asset.Title));
    }

    [SignalHandler("hide")]
    void OnHide_AssetLibPreview() {
        EmitSignal("preview_closed");
    }

    public void ShowDialog(AssetLib.Asset asset) {
        _DialogTitle.Text = asset.Title;
        _PluginName.Text = asset.Title;
        _Author.Text = asset.Author;
        _Category.Text = asset.Category;
        _Version.Text = asset.VersionString;
        _AddonId.Text = asset.AssetId;
        _GodotVersion.Text = "v" + asset.GodotVersion;
        _License.Text = asset.Cost;
        _Description.BbcodeText = "[table=1][cell][color=lime]" + 
        Tr("Support") + $"[/color][/cell][cell][color=aqua][url={asset.BrowseUrl}]" + 
        Tr("Homepage") + $"[/url][/color][/cell][cell][color=aqua][url={asset.IssuesUrl}]" +
        Tr("Issue/Support Page") + $"[/url][/color][/cell][/table]\n\n{asset.Description.Replace("\r","")}";
        _Description.ScrollToLine(0);
        _asset = asset;
        
        if (asset.IconUrl == null || asset.IconUrl == "") {
            sIconPath = "res://Assets/Icons/missing_icon.svg";
        } else {
            Uri uri = new Uri(asset.IconUrl);
            sIconPath = $"user://cache/images/{asset.AssetId}{uri.AbsolutePath.GetExtension()}";
        }
        
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
        dldPreviews.Clear();

        for (int i = 0; i < asset.Previews.Count; i++) {
            TextureRect preview = new TextureRect();
            preview.RectMinSize = new Vector2(120,120);
            preview.Texture = GD.Load<Texture>("res://Assets/Icons/icon_thumbnail_wait.svg");
            preview.Expand = true;
            preview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            preview.Connect("gui_input", this, "OnGuiInput_Preview", new Array { preview });
            _Thumbnails.AddChild(preview);
            Uri tnUri;
            try {
                tnUri = new Uri(asset.Previews[i].Thumbnail);
            }
            catch (System.UriFormatException) {
                try {
                    tnUri = new Uri(asset.Previews[i].Link);
                }
                catch {
                    tnUri = new Uri("http://localhost/missing_icon.svg");
                }
            }
            string iconPath = $"user://cache/images/{asset.AssetId}-{i}-{asset.Previews[i].PreviewId}{tnUri.AbsolutePath.GetExtension()}";
            if (!File.Exists(iconPath.GetOSDir().NormalizePath())) {
                ImageDownloader dld = new ImageDownloader(tnUri, iconPath);
                dldPreviews.Add(dld);
                preview.SetMeta("iconPath", iconPath);
                dlq.Push(dld);
            } else {
                dldPreviews.Add(null);
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
            if (CentralStore.Instance.HasTemplateId(asset.AssetId)) {
                _Uninstall.Visible = true;
                _Sep3.Visible = true;
                AssetProject tasset = CentralStore.Instance.GetTemplateId(asset.AssetId);
                if (tasset.Asset.VersionString != asset.VersionString ||
                    tasset.Asset.Version != asset.Version ||
                    tasset.Asset.ModifyDate != asset.ModifyDate) {
                    _Download.Disabled = false;
                    _Download.Text = Tr("Update Template");
                } else {
                    _Download.Disabled = true;
                    _Download.Text = Tr("Download");
                }
            } else {
                _Uninstall.Visible = false;
                _Sep3.Visible = false;
                _Download.Disabled = false;
                _Download.Text = Tr("Download");
            }
        } else {
            if (CentralStore.Instance.HasPluginId(asset.AssetId)) {
                _Uninstall.Visible = true;
                _Sep3.Visible = true;
                AssetPlugin passet = CentralStore.Instance.GetPluginId(asset.AssetId);
                if (passet.Asset.VersionString != asset.VersionString ||
                    passet.Asset.Version != asset.Version ||
                    passet.Asset.ModifyDate != asset.ModifyDate) {
                    _Download.Disabled = false;
                    _Download.Text = Tr("Update Plugin");
                } else {
                    _Download.Disabled = true;
                    _Download.Text = Tr("Download");
                }
            } else {
                _Uninstall.Visible = false;
                _Sep3.Visible = false;
                _Download.Disabled = false;
                _Download.Text = Tr("Download");
            }
        }
        
        dlq.StartDownload();
        Visible = true;
    }

    [SignalHandler("gui_input", nameof(_AddonId))]
    void OnGuiInput_AddonId(InputEvent @event)
    {
        if (@event is InputEventMouseButton iemb && iemb.ButtonIndex == (int)ButtonList.Left)
        {
            OS.ShellOpen($"https://godotengine.org/asset-library/asset/{_AddonId.Text}");
        }
    }

    [SignalHandler("pressed", nameof(_Uninstall))]
    async void OnPressed_Uninstall() {
        if (CentralStore.Instance.HasPluginId(_asset.AssetId)) {
            // Handle Asset Uninstall
            Array<ProjectFile> usingPlugin = new Array<ProjectFile>();
            foreach(ProjectFile pf in CentralStore.Projects) {
                if (pf.Assets == null)
                    continue;
                if (pf.Assets.Contains(_asset.AssetId))
                    usingPlugin.Add(pf);
            }

            if (usingPlugin.Count > 0) {
                bool res = await AppDialogs.YesNoDialog.ShowDialog(Tr("Uninstall - Plugin in Use"),
                    string.Format(Tr("The plugin {0} is currently used in {1} project(s). Uninstalling" +
                                     " will remove tracking of this plugin, continue?"),
                    _asset.Title,usingPlugin.Count));
                if (!res) {
                    return;
                }
                foreach(ProjectFile pf in usingPlugin) {
                    pf.Assets.Remove(_asset.AssetId);
                }
            }
            AssetPlugin plg = CentralStore.Instance.GetPluginId(_asset.AssetId);
            CentralStore.Plugins.Remove(plg);
            AppDialogs.MessageDialog.ShowMessage(Tr("Plugin Uninstall"),
                string.Format(Tr("{0} has been uninstalled.  Any projects referencing it, no longer have a reference." +
                "  The addon files still remain in the Addons folder of the project, and will need to be removed manually."),_asset.Title));
        } else if (CentralStore.Instance.HasTemplateId(_asset.AssetId)) {
            // Handle Template Uninstall
            AssetProject prj = CentralStore.Instance.GetTemplateId(_asset.AssetId);
            if (prj == null) {
                AppDialogs.MessageDialog.ShowMessage(Tr("Asset Uninstall"), string.Format(Tr("{0} is not found in plugins or Templates."),_asset.Title));
                return;
            }

            CentralStore.Templates.Remove(prj);
            AppDialogs.MessageDialog.ShowMessage(Tr("Template Uninstall"),string.Format(Tr("{0} has been uninstalled."),_asset.Title));
        }
        EmitSignal("uninstalled_addon");
        Visible = false;
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

    private void InvalidUri()
    {
        _MissingThumbnails.Visible = true;
        _PlayButton.Visible = false;
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

                dldPreviews[indx] = null;
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
        if (!preview.HasMeta("iconPath"))
            return;
        object iconMeta = preview.GetMeta("iconPath");
        if (iconMeta is null)
            return;
        string iconPath = iconMeta as string;
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
