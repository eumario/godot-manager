using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Controls;

public partial class LineEditTimeout : LineEdit
{
	#region Signals

	[Signal]
	public delegate void TextUpdatedEventHandler(string text);
	#endregion
	
	#region Quick Create
	public static LineEditTimeout FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Controls/LineEditTimeout.tscn");
		return scene.Instantiate<LineEditTimeout>();
	}
	#endregion
	
	#region Resources
	#endregion
	
	#region Exports

	[Export] public float WaitTimeout = 1.0f;
	#endregion
	
	#region Node Paths

	[NodePath] private Timer _timer;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion
	
	#region Constants
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		_timer.OneShot = true;
		_timer.WaitTime = WaitTimeout;

		TextChanged += (text) => _timer.Start();
		TextSubmitted += (text) =>
		{
			if (!_timer.IsStopped())
				_timer.Stop();
			EmitSignal(SignalName.TextUpdated, Text);
		};
		_timer.Timeout += () => EmitSignal(SignalName.TextUpdated, Text);
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}