#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Network;
using GodotManager.Library.Models;

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
    public DownloadPack? CurrentPack { get; private set; }
    #endregion
    
    #region Godot Overrides
    public override partial void _Process(double delta);
    
    [GodotOverride]
    public async void OnProcess(double delta)
    {
        if (_downloadInstance != null) return;
        if (_queue.Count == 0) return;
        CurrentPack = _queue.Dequeue();
        EmitSignalStartTagDownload(CurrentPack.Tag);
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
                Callable.From(() => EmitSignalDownloadProgressChanged(CurrentPack.Tag, -1)).CallDeferred();
            else
                Callable.From(() => EmitSignalDownloadProgressChanged(CurrentPack.Tag, (change.Total / CurrentPack.CurrentSize) * 100)).CallDeferred();
        };
        _downloadInstance.Completed += async (_, bytes) =>
        {
            if (!Directory.Exists(CurrentPack.CurrentSavePath.GetBaseDir()))
                Directory.CreateDirectory(CurrentPack.CurrentSavePath.GetBaseDir());
            await File.WriteAllBytesAsync(CurrentPack.CurrentSavePath, bytes);
            Callable.From(() =>
            {
                EmitSignalDownloadCompleted(CurrentPack.Tag, CurrentPack.CurrentStep, CurrentPack.CurrentSavePath);
            }).CallDeferred();
            if (CurrentPack.CompleteCurrent())
            {
                _downloadInstance.Dispose();
                _downloadInstance = null;
                Callable.From(() =>
                {
                    EmitSignalDownloadTagCompleted(CurrentPack.Tag);
                    CurrentPack = null;
                }).CallDeferred();
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
        
        _downloadInstance.StartDownload();
    }
    #endregion

    #region Public API
    public void QueueDownload(string tag, string url, long size, string savePath)
    {
        DownloadPack? pack = _queue.FirstOrDefault(x => x.Tag == tag);
        if (pack == null)
        {
            pack = new DownloadPack();
            _queue.Enqueue(pack);
        }
        pack.Urls.Add(url);
        pack.Sizes.Add(size);
        pack.SavePath.Add(savePath);
        EmitSignalQueueTagDownload(pack);
    }
    #endregion
}