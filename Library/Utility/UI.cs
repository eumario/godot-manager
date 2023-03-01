using System.Threading.Tasks;
using Godot;

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
        await dlg.ToSignal(dlg, "visibility_changed");
        dlg.QueueFree();
        return accepted;
    }
}