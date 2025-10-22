using System;
using System.Collections.Generic;
using Godot;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using NotificationIcon.NET;

namespace GodotManager.Library.UI;

public partial class TrayIcon : Node
{
    [Notify, Export] public partial Texture2D Icon { get; set; }
    [Notify, Export] public partial NodePath Menu { get; set; }
    [Notify, Export] public partial string Tooltip { get; set; }
    [Notify, Export] public partial bool Visible { get; set; }

    #if GODOT_LINUXBSD
    private NotifyIcon _notifyIcon;
    private List<MenuItem> _menuItems = [];
    #else
    private StatusIndicator _statusIndicator;
    #endif

    public TrayIcon()
    {
#if !GODOT_LINUXBSD
        _statusIndicator = new StatusIndicator();
#endif
    }
    
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
#if GODOT_LINUXBSD
        
        _menuItems = BuildMenu();
        var tmpImg = OS.GetTempDir().PathJoin($"icon{DateTime.Now.Ticks}.png");
        Icon.GetImage().SavePng(tmpImg);
        _notifyIcon = NotifyIcon.Create(tmpImg, _menuItems);
#else
        _statusIndicator.Icon = Icon;
        _statusIndicator.Menu = Menu;
        _statusIndicator.Tooltip = Tooltip;
        _statusIndicator.Visible = Visible;
#endif
    }
    
#if GODOT_LINUXBSD
    public override partial void _Process(double delta);
    
    [GodotOverride]
    public void OnProcess(double delta)
    {
        _notifyIcon.MessageLoopIteration(false);
    }
    
    public List<MenuItem> BuildMenu(PopupMenu menu = null)
    {
        List<MenuItem> items = [];
        menu ??= GetNode<PopupMenu>(Menu);
        for (var i = 0; i < menu.ItemCount; i++)
        {
            if (menu.GetItemSubmenuNode(i) != null)
            {
                menu.GetItemSubmenuNode(i).MenuChanged += HandleMenuChanged;
            }
            items.Add(CreateMenuItem(menu, i, menu.GetItemSubmenuNode(i) != null));
        }

        menu.MenuChanged += HandleMenuChanged;

        return items;
    }

    private MenuItem CreateMenuItem(PopupMenu menu, int index, bool subMenu)
    {
        if (subMenu)
            return new MenuItem(menu.IsItemSeparator(index) ? "-" : menu.GetItemText(index))
            {
                IsChecked = menu.IsItemChecked(index),
                IsDisabled = menu.IsItemDisabled(index),
                Click = (s, e) => HandleClick(menu, index, menu.GetItemId(index)),
            };
        else
            return new MenuItem(menu.IsItemSeparator(index) ? "-" : menu.GetItemText(index))
            {
                IsChecked = menu.IsItemChecked(index),
                IsDisabled = menu.IsItemDisabled(index),
                Click = (s, e) => HandleClick(menu, index, menu.GetItemId(index)),
                SubMenu = BuildMenu(menu.GetItemSubmenuNode(index))
            };
    }

    private void HandleMenuChanged()
    {
        _menuItems = BuildMenu();
        _notifyIcon.MenuItems = _menuItems;
    }

    public void HandleClick(PopupMenu menu, int index, int id)
    {
        menu.EmitSignal(PopupMenu.SignalName.IndexPressed, index);
        menu.EmitSignal(PopupMenu.SignalName.IdPressed, id);
    }
#endif
}