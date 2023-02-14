using Godot;
using Godot.Sharp.Extras;

// namespace
namespace GodotManager.Library.Components.Dialogs;

public partial class AddonInstaller : AcceptDialog
{
	#region Signals
	#endregion
	
	#region Quick Create
	public static AddonInstaller FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/AddonInstaller.tscn");
		return scene.Instantiate<AddonInstaller>();
	}
	#endregion
	
	#region Node Paths

	[NodePath] private Label _detailLabel;
	[NodePath] private Tree _addonTree;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		TreeExited += QueueFree;
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}
