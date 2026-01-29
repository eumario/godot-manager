#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Network;
using GodotManager.Library.Models;
using GodotManager.Library.Util;

namespace GodotManager.Library.Managers;

public partial class DownloadManager : Node
{
    #region Singleton
    private static DownloadManager? _instance;

    public static DownloadManager Instance
    {
        get
        {
            _instance ??= new DownloadManager();
            if (!_instance.IsInsideTree())
            {
                ((SceneTree)Engine.GetMainLoop()).Root.AddChild(_instance);
            }
            return _instance;
        }
    }
    #endregion
    
    #region Signals
    [Signal]
    public delegate void QueueTagDownloadEventHandler(DownloadPack pack);
    
    [Signal]
    public delegate void StartTagDownloadEventHandler(string tag);
    
    [Signal]
    public delegate void DownloadProgressChangedEventHandler(string tag, double progress);

    [Signal]
    public delegate void DownloadCompletedEventHandler(string tag, int step, string savePath);

    [Signal]
    public delegate void DownloadTagCompletedEventHandler(string tag);
    #endregion

    #region Private Variables
    private readonly Queue<DownloadPack> _queue = [];
    private DownloadInstance? _downloadInstance;

    #endregion
    
    #region Public Variables
    public int QueueSize => _queue.Count;
    public DownloadPack CurrentPack => _queue.Peek();
    public bool QueueEmpty => _queue.Count == 0;
    #endregion
    
    #region Godot Overrides
    public override partial void _Process(double delta);
    
    [GodotOverride]
    public async void OnProcess(double delta)
    {
        if (QueueEmpty) return;
        if (CurrentPack.IsDownloading) return;
        EmitSignalStartTagDownload(CurrentPack.Tag);
        CurrentPack.IsDownloading = true;
        await SetupDownloadInstance();
    }
    #endregion
    
    #region Private API

    private async Task SetupDownloadInstance()
    {
        _downloadInstance = new DownloadInstance(CurrentPack!.CurrentUrl);
        _downloadInstance.ProgressChanged += (_, change) =>
        {
            if (CurrentPack.CurrentSize == -1)
                this.EmitSignalDeferred(SignalName.DownloadProgressChanged, CurrentPack.Tag, -1);
            else
                this.EmitSignalDeferred(SignalName.DownloadProgressChanged, CurrentPack.Tag, (double)change.Total / CurrentPack.CurrentSize * 100.0d);
        };
        _downloadInstance.Completed += async (_, bytes) =>
        {
            if (!Directory.Exists(CurrentPack.CurrentSavePath.GetBaseDir()))
                Directory.CreateDirectory(CurrentPack.CurrentSavePath.GetBaseDir());
            await File.WriteAllBytesAsync(CurrentPack.CurrentSavePath, bytes);
            var currentTag = CurrentPack.Tag;
            var currentStep = CurrentPack.CurrentStep;
            var currentSavePath = CurrentPack.CurrentSavePath;
            this.EmitSignalDeferred(SignalName.DownloadCompleted, currentStep, currentSavePath);
            if (CurrentPack.CompleteCurrent())
            {
                _downloadInstance.Dispose();
                _downloadInstance = null;
                this.EmitSignalDeferred(SignalName.DownloadTagCompleted, CurrentPack.Tag);
                _queue.Dequeue();
                return;
            }
            
            _downloadInstance.Dispose();
            _downloadInstance = null;
            await SetupDownloadInstance();
        };
        
        if (CurrentPack.CurrentSize == -1)
        {
            var size = await _downloadInstance.GetDownloadSize();
            CurrentPack.CurrentSize = size;
        }

        this.EmitSignalDeferred(SignalName.StartTagDownload, CurrentPack.Tag);
        
        _downloadInstance.StartDownload();
    }
    #endregion

    #region Public API
    public void QueueDownload(string tag, string url, long size, string savePath)
    {
        var tags = _queue.ToList();
        DownloadPack? pack = tags.FirstOrDefault(x => x.Tag == tag);
        if (pack == null)
        {
            pack = new DownloadPack();
            pack.Tag = tag;
            _queue.Enqueue(pack);
            EmitSignal(SignalName.QueueTagDownload, pack);
        }
        pack.Urls.Add(url);
        pack.Sizes.Add(size);
        pack.SavePath.Add(savePath);
    }
    #endregion
}