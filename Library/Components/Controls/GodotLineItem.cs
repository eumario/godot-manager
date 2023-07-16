using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;
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
	[Signal] public delegate void ExecuteClickEventHandler(GodotLineItem gli);
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
	
	#region Resources
	[Resource("res://Assets/Icons/svg/uninstall.svg")] private Texture2D _uninstall;
	#endregion
	
	#region Private Variables
	// Backer Variables for Public Properties (That Automate the UI)
	private GodotVersion _godotVersion;
	private GithubVersion _githubVersion;
	private TuxfamilyVersion _tuxfamilyVersion;
	private CustomEngineDownload _customEngineDownload;

	// Internal Settings that handle both Property Automation with UI, and internal functions
	private bool _settingsShare = false;
	private bool _settingsLinked = false;
	private bool _showMono = false;
	private bool _downloading = false;
	private List<long> _speedMarks = new List<long>();
	#endregion
	
	#region Public Properties

	public bool ShowMono
	{
		get => _showMono;
		set
		{
			_showMono = value;
			if (_versionTag is null) return;			// Is the UI Controls loaded?
			if (_godotVersion is not null)
			{
				GodotVersion = _godotVersion;
				return;
			}

			if (_githubVersion is not null)
			{
				GithubVersion = _githubVersion;
				return;
			}

			TuxfamilyVersion = _tuxfamilyVersion;
		}
	}

	// Read-Only variable to check _showMono for true/false, if true, is Mono Edition, else Standard Edition.
	public bool IsMono => _showMono;
	
	// Are we downloading this version of Godot?
	public bool Downloading
	{
		get => _downloading;
		set
		{
			_downloading = value;
			_download.Visible = value;
			_installed.Visible = !value;
		}
	}
	
	// If _godotVersion is not null, then it is installed, otherwise it is not.
	public bool IsInstalled => _godotVersion is not null;
	
	// Checks to see  if this version is the default version for Godot 3, or Godot 4.
	public bool IsDefault => IsInstalled && (_godotVersion == Database.Settings.DefaultEngine3 ||
	                                         _godotVersion == Database.Settings.DefaultEngine4);

	// Holds the Godot Version information, including where it was downloaded from.
	public GodotVersion GodotVersion
	{
		get => _godotVersion;
		set
		{
			_godotVersion = value;
			if (_versionTag is null) return;
			if (_godotVersion is null) return;
			
			_githubVersion = _godotVersion.GithubVersion;
			// _mirrorVersion = _godotVersion.MirrorVersion;
			_customEngineDownload = _godotVersion.CustomEngine;
			_installUninstall.Icon = _uninstall;
			_installUninstall.SelfModulate = Colors.Red;
			_linkSettings.Visible = IsInstalled;
			_shareSettings.Visible = IsInstalled;
		}
	}

	public TuxfamilyVersion TuxfamilyVersion
	{
		get => _tuxfamilyVersion;
		set
		{
			_tuxfamilyVersion = value;
			if (_versionTag is null) return;
			if (_tuxfamilyVersion is null) return;
			
			_installed.Visible = true;
			_linkSettings.Visible = IsInstalled;
			_shareSettings.Visible = IsInstalled;
			_loc.Visible = IsInstalled;
			var mono = _tuxfamilyVersion.SemVersion.Version.Major >= 4 && _showMono ? " Dotnet" : _showMono ? " Mono" : "";
			_versionTag.Text = $"Godot v{_tuxfamilyVersion.SemVersion.ToNormalizedStringNoSpecial()} ({_tuxfamilyVersion.ReleaseStage}{mono})";
			_godotTree.Text = $"{_tuxfamilyVersion.SemVersion.Version.Major}.x";

			_downloadUrl.Text = _showMono ? _tuxfamilyVersion.CSharpDownloadUrl : _tuxfamilyVersion.StandardDownloadUrl;
			_downloadFS.Text = "Size: " + Util.FormatSize(_showMono ? _tuxfamilyVersion.CSharpDownloadSize : _tuxfamilyVersion.StandardDownloadSize);
			if (_showMono)
			{
				Visible = _tuxfamilyVersion.CSharpDownloadSize > 0;
			}
			else
			{
				Visible = _tuxfamilyVersion.StandardDownloadSize > 0;
			}
		}
	}

	// Holds the Custom Engine Download information, which is user provided.
	public CustomEngineDownload CustomEngine
	{
		get => _customEngineDownload;
		set
		{
			_customEngineDownload = value;
			if (_versionTag is null) return;
			if (_customEngineDownload is null) return;

			_linkSettings.Visible = IsInstalled;
			_shareSettings.Visible = IsInstalled;
		}
	}
	
	// Holds the GitHub version information, from the Github Repository.
	public GithubVersion GithubVersion
	{
		get => _githubVersion;
		set
		{
			_githubVersion = value;
			if (_versionTag is null) return;
			if (_githubVersion is null) return;
			
			_installed.Visible = true;
			_linkSettings.Visible = IsInstalled;
			_shareSettings.Visible = IsInstalled;
			_loc.Visible = IsInstalled;
			var mono = ShowMono ? (_githubVersion.SemVersion.Version.Major == 4 ? " Dotnet" : " Mono") : string.Empty;
			_versionTag.Text = $"Godot v{_githubVersion.SemVersion.ToNormalizedStringNoSpecial()} (Stable{mono})";
			_godotTree.Text = $"{_githubVersion.SemVersion.Version.Major}.x";

			_downloadUrl.Text = IsInstalled
				? "github.com/godotengine/godot"
				: ShowMono
					? $"{_githubVersion.CSharpDownloadUrl}"
					: !string.IsNullOrEmpty(_githubVersion.StandardDownloadUrl) 
						? $"{_githubVersion.StandardDownloadUrl}"
						: $"{_githubVersion.Release.TarballUrl}";
			
			_downloadFS.Text = "Size: " + Util.FormatSize(ShowMono
				? _githubVersion.CSharpArchiveSize
				: !string.IsNullOrEmpty(_githubVersion.StandardDownloadUrl)
					? _githubVersion.StandardArchiveSize
					: 0);
		}
	}

	// Toggles if Settings are Shared with Other versions of Godot. (Based on 3.x/4.x)
	public bool SettingsShare
	{
		get => _settingsShare;
		set
		{
			_settingsShare = value;
			if (_shareSettings is null) return;
			_shareSettings.SelfModulate = _settingsShare ? Colors.YellowGreen : Colors.SlateGray;
		}
	}

	// Toggles if Settings are Linked with Other versions of Godot. (Based on 3.x/4.x)
	public bool SettingsLinked
	{
		get => _settingsLinked;
		set
		{
			_settingsLinked = value;
			if (_linkSettings is null) return;
			_linkSettings.SelfModulate = _settingsLinked ? Colors.Green : Colors.SlateGray;
		}
	}
	#endregion

	#region Godot Overrides
	// Handle setting up the UI event signals, and handling the function calls.
	public override void _Ready()
	{
		// Load Nodes
		this.OnReady();
		
		// Check if we are installed Godot version, or not.
		if (_godotVersion != null)
			GodotVersion = _godotVersion;	// Update UI, based upon Installed Information
		else
		{
			GithubVersion = _githubVersion;			// Update UI based upon Github Information
			TuxfamilyVersion = _tuxfamilyVersion;   // Update UI based upon Tuxfamily Information
			CustomEngine = _customEngineDownload;	// Update UI based upon Custom URL Information.
		}

		// Handle Highlighting a line when mouse is over a version line.
		MouseEntered += () => _hover.Visible = true;
		MouseExited += () => _hover.Visible = false;
		
		// Handle Install/Uninstall Signals when clicking on the icon to install/uninstall.
		_installUninstall.Pressed += () =>
			EmitSignal(IsInstalled ? SignalName.UninstallClicked : SignalName.InstallClicked, this);
		
		// Handle Toggle of SettingsLinked, and Emit the LinkedSettings signal.
		_linkSettings.Pressed += () =>
		{
			SettingsLinked = !SettingsLinked;
			EmitSignal(SignalName.LinkedSettings, SettingsLinked);
		};
		
		// Handle Toggle of SettingsShare, and Emit the SharedSettings signal.
		_shareSettings.Pressed += () =>
		{
			SettingsShare = !SettingsShare;
			EmitSignal(SignalName.SharedSettings, SettingsShare);
		};
	}

	// Handle Mouse Events
	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton iemb) return;
		// If MouseButton Right is pressed, Emit RightClick signal, to allow Context Menu to be shown.
		if (iemb.Pressed && iemb.ButtonIndex == MouseButton.Right)
			EmitSignal(SignalName.RightClick, this);
		
		// If MouseButton Left is double clicked, Emit ExecuteClick signal, to allow launching of a project.
		if (iemb.DoubleClick && iemb.ButtonIndex == MouseButton.Left)
			EmitSignal(SignalName.ExecuteClick, this);
	}

	#endregion
	
	#region Godot Event Handlers
	#endregion
	
	#region Public Functions

	public void SetupDownloadProgress()
	{
		var size = _githubVersion?.GetDownloadSize(IsMono) ?? _tuxfamilyVersion.GetDownloadSize(IsMono);
		_downloadProgress.MinValue = 0;
		_downloadProgress.MaxValue = size;
		_downloadProgress.Value = 0;
		_downloadUrl.Text = _githubVersion?.GetDownloadUrl(IsMono) ?? _tuxfamilyVersion.GetDownloadUrl(IsMono);
		_downloadETA.Text = "??:??";
		_downloadSize.Text = $"{Util.FormatSize(0)}/{Util.FormatSize(size)}";
		_downloadSpeed.Text = $"{Util.FormatSize(0)}/s";
		Downloading = true;
	}

	public void UpdateSpeed(DateTime start, long transferred, long downloaded)
	{
		Util.RunInMainThread(() =>
		{
			if (_downloadProgress.Value > downloaded)
			{
				GD.Print("Got event after Download completed.");
				return;
			}
			_downloadSize.Text = $"{Util.FormatSize(downloaded)}/{Util.FormatSize(_githubVersion?.GetDownloadSize(IsMono) ?? _tuxfamilyVersion.GetDownloadSize(IsMono))}";
			_speedMarks.Add(transferred);
			while (_speedMarks.Count > 10)
				_speedMarks.RemoveAt(0);
			
			var avg = _speedMarks.Sum() / _speedMarks.Count;
			_downloadSpeed.Text = $"Speed: {Util.FormatSize(avg)}/s";
			if (downloaded == 0) return;
		
			var elapsedTime = DateTime.Now - start;
			var estTime =
				TimeSpan.FromSeconds(
					((_githubVersion?.GetDownloadSize(IsMono) ?? _tuxfamilyVersion.GetDownloadSize(IsMono)) - downloaded) /
					(downloaded / elapsedTime.TotalSeconds));
			_downloadETA.Text = $"ETA: {estTime:hh':'mm':'ss}";
		});
	}

	public void UpdateProgress(long totalDownloaded)
	{
		Util.RunInMainThread(() =>
		{
			if (_downloadProgress.Value > totalDownloaded) return;
			_downloadProgress.Value = totalDownloaded;
		});
	}

	#endregion

	#region Private Functions

	#endregion
}