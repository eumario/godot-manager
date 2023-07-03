using System;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

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

	public static GodotLineItem FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Controls/GodotLineItem.tscn");
		return scene.Instantiate<GodotLineItem>();
	}
	#endregion

	#region Node Paths

	[NodePath] private ColorRect _hover = null;
	[NodePath] private RichTextLabel _versionTag = null;
	
	[NodePath] private VBoxContainer _installed = null;
	[NodePath] private Label _downloadUrl = null;
	[NodePath] private Label _downloadFS = null;
	[NodePath] private HBoxContainer _loc = null;
	[NodePath] private Label _installedLoc = null;

	[NodePath] private VBoxContainer _download = null;
	[NodePath] private Label _downloadLoc = null;
	[NodePath] private ProgressBar _downloadProgress = null;
	[NodePath] private Label _downloadSize = null;
	[NodePath] private Label _downloadETA = null;
	[NodePath] private Label _downloadSpeed = null;

	[NodePath] private Button _linkSettings = null;
	[NodePath] private Label _godotTree = null;
	[NodePath] private Button _shareSettings = null;
	[NodePath] private Button _installUninstall = null;
	#endregion
	
	#region Private Variables
	// Backer Variables for Public Properties (That Automate the UI)
	private GodotVersion _godotVersion;
	private GithubVersion _githubVersion;
	private CustomEngineDownload _customEngineDownload;
	#endregion
	
	#region Public Properties

	public bool ShowMono = false;
	public GithubVersion GithubVersion
	{
		get => _githubVersion;
		set
		{
			_githubVersion = value;
			if (_versionTag is null) return;
			
			_download.Visible = false;
			_installed.Visible = true;
			_loc.Visible = false;
			_versionTag.Text = $"Godot v{_githubVersion.Release.TagName}";
			_godotTree.Text = _githubVersion.Release.TagName.Contains("3.") ? "3.x" : "4.x";

			if (ShowMono)
			{
				_downloadUrl.Text = $"Source: {_githubVersion.CSharpDownloadUrl}";
				_downloadFS.Text = Util.FormatSize(_githubVersion.CSharpArchiveSize);
			}
			else
			{
				_downloadUrl.Text = $"Source: {_githubVersion.StandardDownloadUrl}";
				_downloadFS.Text = Util.FormatSize(_githubVersion.StandardArchiveSize);
			}
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		GithubVersion = _githubVersion;
		MouseEntered += () => _hover.Visible = true;
		MouseExited += () => _hover.Visible = false;
	}

	#endregion
	
	#region Godot Event Handlers
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Private Functions
	#endregion
}