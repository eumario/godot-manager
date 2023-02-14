using Godot;
using Godot.Sharp.Extras;

namespace GodotManager.Library.Components.Controls;

public partial class GodotLineItem : Control
{
	#region Signals
	[Signal] public delegate void InstallClickedEventHandler(GodotLineItem gli);
	[Signal] public delegate void UninstallClickedEventHandler(GodotLineItem gli);
	[Signal] public delegate void LinkedSettingsEventHandler(GodotLineItem gli, bool value);
	[Signal] public delegate void SharedSettingsEventHandler(GodotLineItem gli, bool value);
	[Signal] public delegate void RightClickEventHandler(GodotLineItem gli);
	#endregion
	
	#region Quick Create
	private static readonly PackedScene Packed = GD.Load<PackedScene>("res://Library/Components/Controls/GodotLineItem.tscn");
	public static GodotLineItem CreateControl() => Packed.Instantiate<GodotLineItem>();
	#endregion

	#region Node Paths
	[NodePath] private RichTextLabel VersionTag = null;
	
	[NodePath] private VBoxContainer Installed = null;
	[NodePath] private Label DownloadUrl = null;
	[NodePath] private Label DownloadFS = null;
	[NodePath] private Label InstalledLoc = null;

	[NodePath] private VBoxContainer Download = null;
	[NodePath] private Label DownloadLoc = null;
	[NodePath] private ProgressBar DownloadProgress = null;
	[NodePath] private Label DownloadSize = null;
	[NodePath] private Label DownloadETA = null;
	[NodePath] private Label DownloadSpeed = null;

	[NodePath] private Button LinkSettings = null;
	[NodePath] private Button ShareSettings = null;
	[NodePath] private Button InstallUninstall = null;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Properties
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
	}
	#endregion
	
	#region Godot Event Handlers
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Private Functions
	#endregion
}