using System.Linq;
using Godot;
using GodotManager.Library.Managers;
using GodotManager.Library.Util;

namespace GodotManager;

[SceneTree(root: "Nodes")]
public partial class SplashScreen : Control
{
    public override partial void _Ready();
    [GodotOverride]
    public async void OnReady()
    {
        var version = ProjectSettings.GetSetting("application/config/version", "0.1.0").AsString();
        Version.Text = $"Version v{version}";
        DisplayServer.WindowSetMinSize(new Vector2I(1025, 600));
        var winTitle = $"Godot Manager v{version}";
        
        #if DEBUG
        winTitle += " (DEBUG)";
        #endif
        
        DisplayServer.WindowSetTitle(winTitle);
        
        GlobalSettings.LoadSettings();
        GlobalSettings.EnsureDirectories();
        
        await ToSignal(GetTree().CreateTimer(.5f), "timeout");
        var mainWin = MainWindow.Instantiate();
        mainWin.Visible = false;
        mainWin.Ready += () =>
        {
            mainWin.Visible = true;
            QueueFree();
        };

        Callable.From(async void () =>
        {
            if (!mainWin.Context.EngineReleases.Any())
            {
                await GithubManager.FetchReleases("godotengine", "godot");
                await GithubManager.FetchReleases("godotengine", "godot-builds");
            }
        }).CallDeferred();

        GetTree().Root.AddChild(mainWin);
        
    }
}
