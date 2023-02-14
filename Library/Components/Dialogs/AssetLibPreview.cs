using Godot;
using Godot.Sharp.Extras;

// namespace
namespace GodotManager.Library.Components.Dialogs;

[Tool]
public partial class AssetLibPreview : AcceptDialog
{
	#region Signals
	#endregion

	#region Quick Create
	public static AssetLibPreview FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/AssetLibPreview.tscn");
		return scene.Instantiate<AssetLibPreview>();
	}
	#endregion

	#region Node Paths
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		AddCancelButton("Close");
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}
