using Godot;
using System;
using GodotManager;
using GodotManager.Library.Managers;
using GodotManager.Library.UI;

[SceneTree(root: "Nodes")]
public partial class Downloads : PanelContainer
{
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        CloseButton.Pressed += () =>
        {
            CloseButton.ReleaseFocus();
            MainWindow.GetInstance()!.HideDownloads();
        };
        NoDownloads.Visible = true;
        DownloadActivity.Visible = false;
        DownloadManager.Instance.QueueTagDownload += pack =>
        {
            var di = DownloadItem.Instantiate(pack);

            di.InstallCompleted += () => di.Reparent(CompletedDownloads, false);

            ActiveDownloads.AddChild(di);

            if (NoDownloads.Visible)
            {
                NoDownloads.Visible = false;
                DownloadActivity.Visible = true;
            }
            if (!Visible)
                MainWindow.GetInstance()!.ShowDownloads();
        };
        SizeChangedHandler();
    }

    public void SizeChangedHandler()
    {
        var size = GetWindow().Size;
        var dsize = Size;
        dsize.Y = size.Y;
        Size = dsize;
        Position = new Vector2(Visible ? 240.0f : -588.0f, 0);
    }
}
