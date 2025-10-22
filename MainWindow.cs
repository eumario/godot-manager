#nullable enable
using System.Threading;
using Godot;
using GodotManager.Library.Database;

namespace GodotManager;

[SceneTree(root: "Nodes")]
public partial class MainWindow : Control
{
    private static MainWindow? _instance;
    public static MainWindow? GetInstance() => _instance;

    public AppContext Context { get; set; }

    [OnInstantiate]
    public void Initialize()
    {
        Context = AppContext.InitDatabase(ProjectSettings.GlobalizePath("user://central_database.sqlite3"));
        _instance = this;
    }

    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        Downloads.Position = new Vector2(-588, 0);
        Downloads.Visible = false;
        GetWindow().CloseRequested += () =>
        {
            GetTree().Quit();
        };
    }

    public void ShowDownloads()
    {
        var tween = CreateTween();
        tween.TweenProperty(Downloads, "visible", true, 0.0);
        tween.TweenProperty(Downloads, "position:x", 240.0, 0.5);
    }

    public void HideDownloads()
    {
        var tween = CreateTween();
        tween.TweenProperty(Downloads, "position:x", -588.0, 0.5);
        tween.TweenProperty(Downloads, "visible", false, 0.0);
    }

}
