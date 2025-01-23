using Godot;
using Godot.Sharp.Extras;

// namespace
namespace GodotManager.Library.Components.Dialogs;

[Tool]
[SceneNode("res://Library/Components/Dialogs/AddonMirror.tscn")]
public partial class AddonMirror : AcceptDialog
{
	#region Signals
	[Signal]
	public delegate void AddMirrorEventHandler(string url);
	#endregion
	
	#region Node Paths
	[NodePath] private OptionButton _protocol;
	[NodePath] private LineEdit _domain;
	[NodePath] private LineEdit _path;
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
		TreeExited += QueueFree;
		Confirmed += () =>
		{
			EmitSignal(SignalName.AddMirror, $"{_protocol.Text}://{_domain.Text}/{_path.Text}");
		};
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}
