using Godot;
using System;
using GodotManager.Library.Managers;
using GodotManager.Library.Models;
using GodotManager.Library.Util;

namespace GodotManager.Library.UI;

[SceneTree(root: "Nodes")]
public partial class DownloadItem : PanelContainer
{
    private DownloadPack _pack;
    private InstallPack? _install;
    #region Instantiate

    [OnInstantiate]
    public void Init(DownloadPack pack)
    {
        _pack = pack;
    }
    #endregion
    
    #region Signals

    [Signal]
    public delegate void InstallCompletedEventHandler();
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
        InstallManager.Instance.QueueTagInstall += tag =>
        {
            if (tag.Tag == _pack.Tag)
                _install = tag;
        };
        InstallManager.Instance.StartTagInstall += tag =>
        {
            if (_install.Tag != tag) return;
            ProgressText.Text = $"begining install...";
            DownloadProgress.Indeterminate = true;
        };
        InstallManager.Instance.InstallProgressChanged += (tag, percent) =>
        {
            if (_install.Tag != tag) return;
            ProgressText.Text = $"installing ({_install.CurrentStep + 1} of {_install.TotalSteps} completed)";
            DownloadProgress.Indeterminate = false;
            DownloadProgress.Value = percent;
        };
        InstallManager.Instance.InstallCompleted += (tag, step) =>
        {
            if (_install.Tag != tag) return;
            ProgressText.Text = $"completed ({step + 1} of {_install.TotalSteps})";
        };
        InstallManager.Instance.InstallTagCompleted += tag =>
        {
            if (_install.Tag != tag) return;
            ProgressText.Text = $"Install Completed.";
            EmitSignalInstallCompleted();
        };

        DownloadManager.Instance.StartTagDownload += tag =>
        {
            if (_pack.Tag != tag) return;
            ProgressText.Text = "starting download...";
        };
        DownloadManager.Instance.DownloadProgressChanged += (tag, percent) =>
        {
            if (_pack.Tag != tag) return;
            ProgressText.Text = $"in progress ({_pack.CurrentStep + 1} of {_pack.TotalSteps} completed)";
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
            if (_pack.Tag != tag) return;
            if (step < _pack.TotalSteps - 1)
            {
                ProgressText.Text = $"completed ({_pack.CurrentStep + 1} of {_pack.TotalSteps} completed)";
                return;
            }
            ProgressText.Text = $"download completed ({_pack.CurrentStep} of {_pack.TotalSteps})";
            GetTree().CreateTimer(0.5d).Timeout += BeginInstall;
        };
    }
    #endregion
    
    #region Private Methods
    private void BeginInstall()
    {
        if (_install == null)
        {
            GD.Print("We never received InstallPack!");
            return;
        }
        _install.IsReady = true;
    }
    #endregion
}
