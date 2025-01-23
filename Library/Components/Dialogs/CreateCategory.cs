using Godot;
using Godot.Sharp.Extras;

// namespace
namespace GodotManager.Library.Components.Dialogs;

[Tool]
[SceneNode("res://Library/Components/Dialogs/CreateCategory.tscn")]
public partial class CreateCategory : AcceptDialog
{
	#region Signals
	[Signal]
	public delegate void CategoryAnswerEventHandler(string name);
	#endregion
	
	#region Node Paths
	[NodePath] private LineEdit _categoryName;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		AddCancelButton("Cancel");
		Canceled += QueueFree;
		Confirmed += () => EmitSignal(SignalName.CategoryAnswer, _categoryName.Text);
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}
