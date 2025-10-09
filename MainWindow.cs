#nullable enable
using Godot;
using GodotManager.Library.Database;

namespace GodotManager;

[SceneTree(root: "Nodes")]
public partial class MainWindow : Control
{
    private static MainWindow? _instance;
    public static MainWindow? GetInstance() => _instance;

    public AppContext Context { get; set; }
    
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        GD.Print("Loading Main Window...");
        Context = AppContext.InitDatabase(ProjectSettings.GlobalizePath("user://central_database.sqlite3"));
        _instance = this;
        GD.Print("Main Window Ready.");
    }

}
