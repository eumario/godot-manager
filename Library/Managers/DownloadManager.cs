
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Godot;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Managers;

public class DownloadManager
{
    private static readonly DownloadManager _instance;
    public static readonly DownloadManager Instance = _instance ??= new DownloadManager();

    public delegate void DownloadCancelledEventHandler(GodotLineItem item);

    public event DownloadCancelledEventHandler Cancelled;

    public record struct Timestamp(DateTime Start, DateTime LastMeasure, long LastByteCount);
    
    private readonly Dictionary<GodotLineItem, Timestamp> _timestamps = new Dictionary<GodotLineItem, Timestamp>();
    private readonly Dictionary<GodotLineItem, DownloadInstance> _downloads = new Dictionary<GodotLineItem, DownloadInstance>();

    public void StartDownload(GodotLineItem item)
    {
        var githubVersion = item.GithubVersion;

        var url = githubVersion?.GetDownloadUrl(item.IsMono);
        var size = githubVersion?.GetDownloadSize(item.IsMono);

        var downloader = new DownloadInstance(url);
        downloader.Completed += (data) => DownloadCompleted(item, data);
        downloader.Progress += (chunkSize, totalFetched) => ProgressUpdate(item, chunkSize, totalFetched);
        downloader.Failed += () => DownloadFailed(item);
        downloader.Cancelled += () => DownloadCancelled(item);
        _timestamps.Add(item, new Timestamp(DateTime.Now, DateTime.Now, 0));
        _downloads.Add(item, downloader);
        item.InstallClicked += HandleDownloadCancelled;
        item.SetupDownloadProgress();
        downloader.StartDownload();
    }

    private void DownloadFailed(GodotLineItem item)
    {
        GD.Print($"Failed to download {item.GithubVersion?.Release.TagName}");
        item.InstallClicked -= HandleDownloadCancelled;
        _timestamps.Remove(item);
    }

    private void ProgressUpdate(GodotLineItem item, long chunkSize, long totalDownloaded)
    {
        var timestamp = _timestamps[item];
        if (DateTime.Now - timestamp.LastMeasure > TimeSpan.FromSeconds(1))
        {
            var totalTransferred = totalDownloaded - timestamp.LastByteCount;
            timestamp.LastMeasure = DateTime.Now;
            timestamp.LastByteCount = totalDownloaded;
            item.UpdateSpeed(timestamp.Start, totalTransferred, totalDownloaded);
        }

        item.UpdateProgress(totalDownloaded);
    }

    private void DownloadCompleted(GodotLineItem item, byte[] buffer)
    {
        var timestamp = _timestamps[item];
        var totalTransferred = buffer.Length - timestamp.LastByteCount;
        item.UpdateSpeed(timestamp.Start, totalTransferred, buffer.Length);
        item.UpdateProgress(buffer.Length);
        item.InstallClicked -= HandleDownloadCancelled;
        _timestamps.Remove(item);
        _downloads.Remove(item);
        InstallManager.Instance.InstallVersion(item, buffer);
    }

    private void HandleDownloadCancelled(GodotLineItem item)
    {
        if (!_downloads.ContainsKey(item))
        {
            item.InstallClicked -= HandleDownloadCancelled;
            return;
        }
        _downloads[item].CancelDownload();
        _downloads.Remove(item);
        _timestamps.Remove(item);
        item.InstallClicked -= HandleDownloadCancelled;
        item.Downloading = false;
        _downloads.Remove(item);
        Cancelled?.Invoke(item);
    }

    private void DownloadCancelled(GodotLineItem item)
    {
        item.Downloading = false;
        _downloads.Remove(item);
        Cancelled?.Invoke(item);
    }
}