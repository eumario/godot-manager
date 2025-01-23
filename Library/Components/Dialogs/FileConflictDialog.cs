using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Dialogs;

[SceneNode("res://Library/Components/Dialogs/FileConflictDialog.tscn")]
public partial class FileConflictDialog : Window
{
	#region Signals

	[Signal]
	public delegate void YesPressedEventHandler();

	[Signal]
	public delegate void YesAllPressedEventHandler();

	[Signal]
	public delegate void NoPressedEventHandler();

	[Signal]
	public delegate void AbortPressedEventHandler();
	#endregion
	
	#region Node Paths
	[NodePath] private Label _archiveFile;
	[NodePath] private Label _destinationFile;

	[NodePath] private Button _yes;
	[NodePath] private Button _yesAll;
	[NodePath] private Button _no;
	[NodePath] private Button _abort;
	#endregion
	
	#region Private Variables

	private string _archivePath;
	private string _destinationPath;
	#endregion
	
	#region Public Variables

	public string ArchiveFile
	{
		get => _archivePath;
		set
		{
			_archivePath = value;
			if (_archiveFile != null)
				_archiveFile.Text = $"Archive File: {value}";
		}
	}

	public string DestinationFile
	{
		get => _destinationPath;
		set
		{
			_destinationPath = value;
			if (_destinationFile != null)
				_destinationFile.Text = $"Destination: {value}";
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		_yes.Pressed += () => EmitSignal(SignalName.YesPressed);
		_yesAll.Pressed += () => EmitSignal(SignalName.YesAllPressed);
		_no.Pressed += () => EmitSignal(SignalName.NoPressed);
		_abort.Pressed += () => EmitSignal(SignalName.AbortPressed);
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}