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

            ActiveDownloads.AddChild(di);
            
            if (!NoDownloads.Visible) return;
            NoDownloads.Visible = false;
            DownloadActivity.Visible = true;
            if (!Visible)
                MainWindow.GetInstance()!.ShowDownloads();
        };
        
        DownloadManager.Instance.StartTagDownload += tag =>
        {
            
        };
    }
}
