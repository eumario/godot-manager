using System.Collections.Generic;
using Godot;

namespace GodotManager.Library.Models;

public partial class DownloadPack : Godot.RefCounted
{
    public string Tag;
    public List<string> Urls = [];
    public List<long> Sizes = [];
    public List<string> SavePath = [];
    private int _current = 0;
    public int CurrentStep => _current;
    public int TotalSteps => Urls.Count;
    
    public string CurrentUrl => Urls[_current];

    public long CurrentSize
    {
        get => Sizes[_current];
        set => Sizes[_current] = value;
    }
    public string CurrentSavePath => SavePath[_current];

    public bool CompleteCurrent()
    {
        _current++;
        _current = Mathf.Min(_current, Urls.Count);
        return _current >= Urls.Count;
    }
}