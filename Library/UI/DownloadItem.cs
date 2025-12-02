using Godot;
using System;
using GodotManager.Library.Managers;
using GodotManager.Library.Models;

namespace GodotManager.Library.UI;

[SceneTree(root: "Nodes")]
public partial class DownloadItem : PanelContainer
{
    private DownloadPack _pack;
    #region Instantiate

    [OnInstantiate]
    public void Init(DownloadPack pack)
    {
        _pack = pack;
    }
    #endregion
    
    #region Godot Overrides
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        DownloadTag.Text = _pack.Tag;
        ProgressText.Text = "queued for download...";
        DownloadProgress.MinValue = 0;
        DownloadProgress.MaxValue = 100;
        DownloadProgress.Value = 0;
        DownloadManager.Instance.StartTagDownload += tag =>
        {
            if (_pack.Tag == tag)
            {
                ProgressText.Text = "starting download...";
            }
        };
        DownloadManager.Instance.DownloadProgressChanged += (tag, percent) =>
        {
            ProgressText.Text = $"in progress ({_pack.CurrentStep} of {_pack.TotalSteps} completed)";
            switch (percent)
            {
                case -1 when !DownloadProgress.Indeterminate:
                    DownloadProgress.Indeterminate = true;
                    break;
                case >= 0 when DownloadProgress.Indeterminate:
                    DownloadProgress.Indeterminate = false;
                    DownloadProgress.Value = percent;
                    break;
                case >= 0:
                    DownloadProgress.Value = percent;
                    break;
            }
        };
        
        DownloadManager.Instance.DownloadCompleted += (tag, step, completed) =>
        {
            if (step < _pack.TotalSteps)
                ProgressText.Text = $"completed ({_pack.CurrentStep} of {_pack.TotalSteps} completed)";
            else
                ProgressText.Text = $"download completed {_pack.TotalSteps}";
        };
    }
    #endregion
}
