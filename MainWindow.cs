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

    private PopupMenu? _recentProjects = null;

    public AppContext Context { get; set; }

    [OnInstantiate]
    public void Initialize()
    {
        Context = AppContext.InitDatabase(ProjectSettings.GlobalizePath("user://central_database.sqlite3"));
        TrayIconMenu.IdPressed += HandleTrayMenu;
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
        GetWindow().SizeChanged += Downloads.SizeChangedHandler;
        //TODO: Implement Recent Projects Submenu
        TrayIconMenu.SetItemDisabled(0, true);
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

    public void HandleTrayMenu(long index)
    {
        switch (index)
        {
            case 200:       // Show Window
                break;
            case 201:       // About
                break;
            case 202:       // Exit
                GetTree().Quit();
                break;
            default:        // Load Project
                break;
        }
    }
}
