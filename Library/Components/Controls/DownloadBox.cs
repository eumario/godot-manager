using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.AssetLib;
using GodotManager.Library.Managers;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Controls;

[SceneNode("res://Library/Components/Controls/DownloadBox.tscn")]
public partial class DownloadBox : PanelContainer
{
    [NodePath] private TextureRect? _assetIcon;
    [NodePath] private Label? _assetName;
    [NodePath] private Label? _finishedText;
    [NodePath] private GridContainer? _progressInfo;
    [NodePath] private Label? _downloadSize;
    [NodePath] private Label? _downloadSpeed;
    [NodePath] private Label? _downloadEta;
    [NodePath] private ProgressBar? _downloadProgress;
    [NodePath] private Button? _dismiss;
    [NodePath] private Button? _configureInstall;

    private Asset? _asset;
    private Texture2D? _icon;
    private bool _downloading;
    private bool _failed;
    private DownloadInstance? _instance;
    private List<long> _speedMarks = new();

    [Signal]
    public delegate void DismissedEventHandler();

    public Asset? Asset
    {
        get => _asset;
        set
        {
            _asset = value;
            if (this.IsNodesReady() && !_assetName!.IsQueuedForDeletion() && _asset != null)
                _assetName!.Text = _asset.Title;
        }
    }

    public Texture2D? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            if (this.IsNodesReady() && !_assetIcon!.IsQueuedForDeletion() && _icon != null)
                _assetIcon!.Texture = _icon;
        }
    }

    public override void _Ready()
    {
        this.OnReady();
        
        _dismiss!.Pressed += () =>
        {
            if (_downloading)
                _instance!.CancelDownload();
            EmitSignal(SignalName.Dismissed);
        };
        _configureInstall!.Pressed += HandleConfigureInstall;

        Asset = _asset;
        Icon = _icon;
        StartDownload();
    }

    private async void StartDownload()
    {
        if (_asset == null) return;
        var uri = new Uri(_asset!.DownloadUrl);
        _configureInstall!.Disabled = true;
        _instance = new DownloadInstance(uri);
        var fileSize = await _instance.GetDownloadSize();
        if (fileSize > 0)
        {
            _downloadSize!.Text = $"0b/{Util.FormatSize(fileSize)}";
            _downloadProgress!.Indeterminate = false;
            _downloadProgress!.MinValue = 0;
            _downloadProgress!.MaxValue = 0;
            _downloadProgress!.Value = 0;
        }
        else
        {
            _downloadSize!.Text = "0b";
            _downloadProgress!.Indeterminate = true;
        }

        var timestamp = new DownloadManager.Timestamp(DateTime.Now, DateTime.Now, 0);
        _instance.Progress += (chunkSize, totalDownloaded) =>
        {
            if (DateTime.Now - timestamp.LastMeasure < TimeSpan.FromMilliseconds(50)) return;
            var totalTransferred = totalDownloaded - timestamp.LastByteCount;
            timestamp.LastMeasure = DateTime.Now;
            timestamp.LastByteCount = totalDownloaded;
            UpdateSpeed(timestamp.Start, totalTransferred, totalDownloaded, fileSize);
            Util.RunInMainThread(() =>
            {
                try { if (_downloadProgress!.IsQueuedForDeletion()) return; } catch (ObjectDisposedException) { return; }
                _downloadProgress!.Value = totalDownloaded;
            });
        };

        _instance.Completed += buffer => HandleSave(uri, buffer);

        _instance.Cancelled += () =>
        {
            UI.MessageBox("Download Cancelled", "The Download has been cancelled.");
        };

        _instance.Failed += () =>
        {
            _downloading = false;
            _failed = true;
            _speedMarks.Clear();
            Util.RunInMainThread(() =>
            {
                try { if (_configureInstall!.IsQueuedForDeletion()) return; } catch (ObjectDisposedException) { return; }

                _configureInstall!.Text = "Retry Download";
                _configureInstall!.Disabled = false;
                UI.MessageBox("Download Failed", "The Download has failed.");
            });
        };
        _downloading = true;
        _instance.StartDownload();
    }

    private void HandleSave(Uri uri, byte[] buffer)
    {
        var path = Database.Settings.CachePath.PathJoin($"AssetLib/{_asset!.AssetId}-{uri.AbsolutePath.GetFilename()}").NormalizePath();
        var dirs = path.GetBaseDir();
        if (!Directory.Exists(dirs))
            Directory.CreateDirectory(dirs);

        File.WriteAllBytes(path, buffer);
        _downloading = false;
        _failed = false;
        _speedMarks.Clear();
        Util.RunInMainThread(() =>
        {
            try { if (_configureInstall!.IsQueuedForDeletion()) return; } catch (ObjectDisposedException) { return; }

            _configureInstall!.Disabled = false;
            _configureInstall!.Text = "Configure Install...";
            UpdateUi();
        });
    }

    private void UpdateSpeed(DateTime start, long transferred, long downloaded, long total)
    {
        Util.RunInMainThread(() =>
        {
            try { if (_downloadProgress!.IsQueuedForDeletion()) return; } catch (ObjectDisposedException) { return; }

            if (!_downloadProgress!.Indeterminate && _downloadProgress!.Value > downloaded)
            {
                GD.Print("Got event after download completed.");
                return;
            }
            _downloadSize!.Text = total == -1 ? $"{Util.FormatSize(downloaded)}" : $"{Util.FormatSize(downloaded)} / {Util.FormatSize(total)}";
            _speedMarks.Add(transferred);
            while (_speedMarks.Count > 10)
                _speedMarks.RemoveAt(0);

            var avg = _speedMarks.Sum() / _speedMarks.Count;
            _downloadSpeed!.Text = $"{Util.FormatSize(avg)}/s";

            if (total == -1)
            {
                _downloadEta!.Text = "Unknown";
                return;
            }
            var elapsed = DateTime.Now - start;
            var estTime = TimeSpan.FromSeconds((total - downloaded) / (downloaded / elapsed.TotalSeconds));
            _downloadEta!.Text = $"{estTime:hh':'mm':'ss}";
        });
    }

    private void UpdateUi()
    {
        try { if (_finishedText!.IsQueuedForDeletion()) return; } catch (ObjectDisposedException) { return; }
        _finishedText!.Text = _failed ? "Download failed, retry?" : "Download complete, configure install.";
        _finishedText!.Visible = true;
        _progressInfo!.Visible = false;
        _downloadProgress!.Modulate = Colors.Transparent;
        _configureInstall!.Disabled = false;
    }

    private void HandleConfigureInstall()
    {
        try { if (_configureInstall!.IsQueuedForDeletion()) return; } catch (ObjectDisposedException) { return; }
        if (_configureInstall!.Text == "Retry Download")
        {
            _configureInstall!.Text = "Configure Install...";
            _finishedText!.Visible = false;
            _progressInfo!.Visible = true;
            _downloadProgress!.Modulate = Colors.White;
            StartDownload();
            return;
        }
        
        UI.MessageBox("Configure Install Dialog", "This is where Configure Install Dialog will be.");
    }
}