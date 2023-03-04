using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using System.Collections.Generic;
using Godot.Sharp.Extras;

namespace GodotManager.Library.Utility;

public static class UI
{
    public static void MessageBox(string title, string message)
    {
        var dlg = new AcceptDialog();
        dlg.Title = title;
        dlg.DialogText = message;
        dlg.DialogAutowrap = true;
        dlg.VisibilityChanged += dlg.QueueFree;
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(dlg);
        dlg.PopupCentered();
    }

    public static async Task<bool> YesNoBox(string title, string message)
    {
        var accepted = false;
        var dlg = new AcceptDialog();
        dlg.Title = title;
        dlg.DialogText = message;
        dlg.DialogAutowrap = true;
        dlg.AddCancelButton("No");
        dlg.OkButtonText = "Yes";
        dlg.DialogHideOnOk = true;
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(dlg);
        dlg.Confirmed += () => accepted = true;
        dlg.Canceled += () => accepted = false;
        dlg.PopupCentered();
        await dlg.ToSignal(dlg, AcceptDialog.SignalName.VisibilityChanged);
        dlg.QueueFree();
        return accepted;
    }

    public static async Task<string> BrowseFolder(string title, string startPath)
    {
        var dlg = new FileDialog();
        dlg.FileMode = FileDialog.FileModeEnum.OpenDir;
        dlg.Access = FileDialog.AccessEnum.Filesystem;
        dlg.Title = title;
        dlg.CurrentDir = startPath;
        var dir = "";
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(dlg);
        dlg.DirSelected += (value) => dir = value;
        dlg.Canceled += () => dlg.Hide();
        dlg.VisibilityChanged += () =>
        {
            if (!dlg.Visible)
                dlg.QueueFree();
        };
        dlg.PopupCentered(new Vector2I(500,400));
        await dlg.ToSignal(dlg, FileDialog.SignalName.VisibilityChanged);
        return dir;
    }

    public static async Task<string> BrowseFile(string title, string startPath, Dictionary<string, string> filters)
    {
        var dlg = new FileDialog();
        dlg.FileMode = FileDialog.FileModeEnum.OpenFile;
        dlg.Access = FileDialog.AccessEnum.Filesystem;
        dlg.Title = title;
        dlg.CurrentDir = startPath;
        dlg.ClearFilters();
        foreach(var kv in filters)
            dlg.AddFilter(kv.Key, kv.Value);
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(dlg);
        var file = "";
        dlg.FileSelected += (value) => file = value;
        dlg.Canceled += () => dlg.Hide();
        dlg.VisibilityChanged += () =>
        {
            if (!dlg.Visible)
                dlg.QueueFree();
        };
        dlg.PopupCentered(new Vector2I(500,400));
        await dlg.ToSignal(dlg, FileDialog.SignalName.VisibilityChanged);
        return file;
    }

    public static async Task<string[]> BrowseFiles(string title, string startPath, Dictionary<string, string> filters)
    {
        var dlg = new FileDialog();
        dlg.FileMode = FileDialog.FileModeEnum.OpenFile;
        dlg.Access = FileDialog.AccessEnum.Filesystem;
        dlg.Title = title;
        dlg.CurrentDir = startPath;
        dlg.ClearFilters();
        foreach(var kv in filters)
            dlg.AddFilter(kv.Key, kv.Value);
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(dlg);
        var files = Array.Empty<string>();
        dlg.FilesSelected += (value) => files = value;
        dlg.Canceled += () => dlg.Hide();
        dlg.VisibilityChanged += () =>
        {
            if (!dlg.Visible)
                dlg.QueueFree();
        };
        dlg.PopupCentered(new Vector2I(500, 400));
        await dlg.ToSignal(dlg, FileDialog.SignalName.VisibilityChanged);
        return files;
    }
}