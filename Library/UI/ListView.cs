using Godot;
using System;

namespace GodotManager.Library.UI;

[GlobalClass, Tool, SceneTree(root: "Nodes")]
public partial class ListView : Control
{
    #region Signals
    #endregion
    
    #region Editor Properties
    [Notify, Export] public int ColumnCount { get; set; }
    #endregion
    
    #region Private Variables
    
    #endregion
    
    
    #region Godot Overrides
    [GodotOverride]
    public void OnReady()
    {
        
    }

    public override partial void _Ready();
    #endregion
    
    #region Public Functions
    #endregion
}
