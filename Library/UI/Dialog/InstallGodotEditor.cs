using System.Linq;
using Godot;
using GodotManager;
using GodotManager.Library.Database;
using GodotManager.Library.Managers;
using GodotManager.Library.Util;

[SceneTree(root: "Nodes")]
public partial class InstallGodotEditor : PanelContainer
{
    private AppContext _appContext;
    [OnInstantiate]
    public void Initialize()
    {
        _appContext = MainWindow.GetInstance()!.Context;
    }
    
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        CloseButton.Pressed += QueueFree;

        var lastVersion = SemanticVersion.Parse("0.0.0");
        VBoxContainer latestContainer = new VBoxContainer();
        var discard = latestContainer;
        
        ReleaseList.QueueFreeAllChildren();
        
        foreach (var release in _appContext.EngineReleases.Where(x => x.Repo == "godot").OrderByDescending(x => x.Version))
        {
            if (lastVersion.Version.Major != release.Version.Version.Major ||
                lastVersion.Version.Minor != release.Version.Version.Minor)
            {
                lastVersion = release.Version;
                var header = new FoldableContainer();
                header.Title = $"Godot {lastVersion.Version.Major}.{lastVersion.Version.Minor}";
                latestContainer = new VBoxContainer();
                header.AddChild(latestContainer);

                header.Folded = !_appContext.IsLatestVersion(release);
                
                ReleaseList.AddChild(header);
            }

            var item = ReleaseItem.Instantiate(release);
            item.LatestRelease = _appContext.IsLatestVersion(release);
            latestContainer.AddChild(item);
        }
        discard.QueueFree();
    }
}
