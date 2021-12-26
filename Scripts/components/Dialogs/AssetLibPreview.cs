using Godot;
using Godot.Collections;
using GodotSharpExtras;

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
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        dlq = new DownloadQueue();
        dlq.Connect("download_completed", this, "OnImageDownloaded");
        dlq.Connect("queue_finished", this, "OnQueueFinished");
        _PlayButton.Connect("gui_input", this, "OnGuiInput_PlayButton");
        AddChild(dlq);
        dldPreviews = new Array<ImageDownloader>();
    }

    public void ShowDialog(AssetLib.Asset asset) {
        _DialogTitle.Text = asset.Title;
        _PluginName.Text = asset.Title;
        _Author.Text = asset.Author;
        _Category.Text = asset.Category;
        _Version.Text = asset.VersionString;
        _License.Text = asset.Cost;
        _Description.BbcodeText = asset.Description;
        
        System.Uri uri = new System.Uri(asset.IconUrl);
        sIconPath = $"user://cache/images/{asset.AssetId}{uri.AbsolutePath.GetExtension()}";
        
        if (!System.IO.File.Exists(sIconPath.GetOSDir().NormalizePath())) {
            dldIcon = new ImageDownloader(asset.IconUrl, sIconPath);
            dlq.Push(dldIcon);
        } else {
            Texture icon = Util.LoadImage(sIconPath);
            if (icon == null)
                _Icon.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
            else
                _Icon.Texture = icon;
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
            System.Uri tnUri = new System.Uri(asset.Previews[i].Thumbnail);
            string iconPath = $"user://cache/images/{asset.AssetId}-{i}-{asset.Previews[i].PreviewId}{tnUri.AbsolutePath.GetExtension()}";
            if (!System.IO.File.Exists(iconPath.GetOSDir().NormalizePath())) {
                ImageDownloader dld = new ImageDownloader(asset.Previews[i].Thumbnail, iconPath);
                dldPreviews.Add(dld);
                preview.SetMeta("iconPath", iconPath);
                dlq.Push(dld);
            } else {
                Texture icon = Util.LoadImage(iconPath);
                if (icon == null)
                    preview.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
                else
                    preview.Texture = icon;
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
        
        dlq.StartDownload();
        Visible = true;
    }

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
		System.Uri tnUri = new System.Uri(rect.GetMeta("url") as string);
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

	public void OnImageDownloaded(ImageDownloader dld) {
        if (dld == dldIcon) {
            Texture icon = Util.LoadImage(sIconPath);
            if (icon == null)
                _Icon.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
            else
                _Icon.Texture = icon;
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

    public void OnQueueFinished() {
        dldPreviews.Clear();
    }

	private void UpdateThumbnail(int indx)
	{
		TextureRect preview = _Thumbnails.GetChild(indx) as TextureRect;
		string iconPath = preview.GetMeta("iconPath") as string;
		if (System.IO.File.Exists(iconPath.GetOSDir().NormalizePath()))
		{
			Texture icon = Util.LoadImage(iconPath);
			if (icon == null)
				icon = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
			preview.Texture = icon;
		}
		else
		{
			preview.Texture = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
		}
	}
}
