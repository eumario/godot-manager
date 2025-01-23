using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Managers;

namespace GodotManager.Library.Components.Controls;

public partial class DownloadArea : PanelContainer
{
    [NodePath] private HBoxContainer? _items;
    
    #region Singleton

    [Singleton] private SignalBus _signalBus;
    #endregion

    public override void _Ready()
    {
        this.OnReady();

        _signalBus.DownloadBoxCreated += box =>
        {
            if (!Visible) Visible = true;
            _items!.AddChild(box);
            box.Dismissed += () =>
            {
                _items!.RemoveChild(box);
                box.QueueFree();
                Visible = _items!.GetChildCount() > 0;
            };
        };
        foreach(var item in _items!.GetChildren())
             item.QueueFree();
        Visible = false;
    }
}