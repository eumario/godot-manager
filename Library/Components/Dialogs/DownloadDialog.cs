using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Dialogs;

public partial class DownloadDialog : AcceptDialog
{
	#region Signals
	#endregion
	
	#region Quick Create
	public static DownloadDialog FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/DownloadDialog.tscn");
		return scene.Instantiate<DownloadDialog>();
	}
	#endregion
	
	#region Node Paths

	[NodePath] private Label _fileName;
	[NodePath] private Label _server;
	[NodePath] private Label _fileSize;
	[NodePath] private Label _speed;
	[NodePath] private Label _eta;
	[NodePath] private ProgressBar _downloadProgress;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		Confirmed += HandleCancel;
	}
	#endregion
	
	#region Event Handlers

	void HandleCancel()
	{
		
	}
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions

	public void StartDownload(string url, int filesize)
	{
		
	}
	#endregion
}