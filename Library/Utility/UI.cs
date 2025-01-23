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
        dlg.CloseRequested += dlg.QueueFree;
        dlg.Confirmed += dlg.QueueFree;
        dlg.Size = new Vector2I(300, 200);
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(dlg);
        dlg.PopupCentered();
    }

    public static async Task<bool> YesNoBox(string title, string message, Vector2I size = default)
    {
        var dlg = new InputDialog();
        dlg.SetupYesNo(title, message, "Yes", "No");
        dlg.Size = size == default ? new Vector2I(320, 240) : size;
        ((SceneTree)Engine.GetMainLoop()).Root.AddChild(dlg);
        dlg.PopupCentered();
        await dlg.ToSignal(dlg, InputDialog.SignalName.Closed);
        var accepted = dlg.BoolResult;
        dlg.QueueFree();
        return accepted;
    }
}