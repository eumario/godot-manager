using Godot;
using GodotManager.Test.TestProjectConfig;

namespace GodotManager.Test;

[Tool,GlobalClass]
public partial class TestGodotProjectConfig : EditorScript
{
    [GodotOverride]
    public void OnRun()
    {
        var dlg = new Window();
        var pcv = ProjectConfigViewer.Instantiate();
        dlg.AddChild(pcv);
        dlg.CloseRequested += () =>
        {
            dlg.QueueFree();
            GD.Print("Project config viewer closed.");
        };
        EditorInterface.Singleton.PopupDialogCentered(dlg, new Vector2I(800,600));
    }

    public override partial void _Run();
}
