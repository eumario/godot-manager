using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.Internal;

namespace GodotManager.Library.Components.Controls;

public partial class ProjectIconItem : Control, IProjectIcon
{
    
    #region Signals
    #endregion
    
    #region Quick Create
    public static ProjectIconItem FromScene()
    {
        var scene = GD.Load<PackedScene>("res://Library/Components/Controls/ProjectIconItem.tscn");
        return scene.Instantiate<ProjectIconItem>();
    }
    #endregion
    
    #region Node Paths
    #endregion
    
    #region Public Properties
    public bool MissingProject { get; set; }
    public GodotVersion GodotVersion { get; set; }
    public ProjectFile ProjectFile { get; set; }
    #endregion
    
    #region Godot Overrides
    public override void _Ready()
    {
        this.OnReady();
    }
    #endregion
    
    #region Godot Event Handlers
    #endregion
    
    #region Public Methods
    #endregion
    
    #region Private Methods
    #endregion
}