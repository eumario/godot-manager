using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Controls;

[Tool]
public partial class Spinner : TextureRect
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private Timer _timer;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		PivotOffset = new Vector2(Size.X/2, Size.Y/2);
		RotationDegrees = 0;
		// Rest of Initialization Functions
		_timer.Timeout += () =>
		{
			RotationDegrees += 30;
		};
		_timer.Start();
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}