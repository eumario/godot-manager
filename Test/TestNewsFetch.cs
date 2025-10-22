#if TOOLS
using Godot;
using GodotManager.Test.TestNews;

[Tool, GlobalClass]
public partial class TestNewsFetch : EditorScript
{
    public override partial void _Run();
    
    [GodotOverride]
    public void OnRun()
    {
        var dlg = new Window();
        var news = TestNews.Instantiate();
        dlg.AddChild(news);
        dlg.CloseRequested += () =>
        {
            dlg.QueueFree();
            GD.Print("Test News Fetch closed.");
        };
        EditorInterface.Singleton.PopupDialogCentered(dlg, new Vector2I(800,600));
    }
}
#endif