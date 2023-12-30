using Godot;
using System;

public partial class PluginLine : Control
{
	#region Signals
	#endregion
	
	#region NodePaths
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Properties
	#endregion
	
	#region FromScene()
	public static PluginLine FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Controls/PluginLine.tscn");
		return scene.Instantiate<PluginLine>();
	}
	#endregion
	
	#region Godot Overrides
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Private Functions
	#endregion
}
