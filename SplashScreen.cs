using Godot;

namespace GodotManager;

[SceneTree(root: "Nodes")]
public partial class SplashScreen : Control
{
    [GodotOverride]
    public async void OnReady()
    {
        var version = ProjectSettings.GetSetting("application/config/version", "0.1.0").AsString();
        Version.Text = $"Version v{version}";
        await ToSignal(GetTree().CreateTimer(.5f), "timeout");
        GetTree().ChangeSceneToFile("res://MainWindow.tscn");
        DisplayServer.WindowSetMinSize(new Vector2I(1025, 600));
        var winTitle = $"Godot Manager v{version}";
        #if DEBUG
        winTitle += " (DEBUG)";
        #endif
        DisplayServer.WindowSetTitle(winTitle);
    }

    public override partial void _Ready();
}
