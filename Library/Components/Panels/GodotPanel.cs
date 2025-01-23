using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Managers;
using GodotManager.Library.Utility;

// namespace
namespace GodotManager.Library.Components.Panels;

[Tool]
public partial class GodotPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths
	[NodePath] private Label _header;
	[NodePath] private ActionButtons _actions;
	[NodePath] private MenuButton _tagSelection;
	[NodePath] private OptionButton _downloadSource;

	[NodePath] private CategoryList _installed;
	[NodePath] private CategoryList _downloading;
	[NodePath] private CategoryList _available;
	[NodePath] private PopupMenu _enginePopup;
	#endregion
	
	#region Private Variables
	private bool _embedded = false;
	private Managers.Github.Godot _githubGodot;
	private Managers.Github.GodotBuilds _godotBuilds;
	private BusyDialog _dialog;
	private bool _showMono = false;
	private string[] _tags = {"Developer Preview", "Alpha", "Beta", "Release Candidate"};
	private string[] _tagsShort = {"dev", "alpha", "beta", "rc" };
	private string _currentTag = "dev";
	#endregion
	
	#region Public Variables
	[Export]
	public bool Embedded
	{
		get => _embedded;
		set
		{
			_embedded = value;
			if (_actions != null)
				_actions.Visible = !value;
			if (_header != null)
				_header.Visible = !value;
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		InstallManager.Instance.InstallCompleted += HandleInstallCompleted;
		InstallManager.Instance.UninstallCompleted += HandleUninstallCompleted;
		DownloadManager.Instance.Cancelled += HandleDownloadCancelled;

		_githubGodot = new Managers.Github.Godot();
		_githubGodot.ReleaseCount += (count) => _dialog.BylineText = $"Processing 0 of {count} releases...";
		_githubGodot.ReleaseProgress += (current, max) => _dialog.BylineText = $"Processing {current} of {max} releases...";

		_godotBuilds = new Managers.Github.GodotBuilds();
		_godotBuilds.ReleaseCount += (count) => _dialog.BylineText = $"Processing 0 of {count} releases...";
		_godotBuilds.ReleaseProgress += (current, max) => _dialog.BylineText = $"Processing {current} of {max} releases...";

		_tagSelection.GetPopup().SetItemChecked(2,true);
		SetOptionsDisabled();

		_tagSelection.GetPopup().IndexPressed += (index) =>
		{
			switch (index)
			{
				case 0: // Mono/C# Viewing
					ToggleMono();
					break;
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
					ToggleTag((int)index - 2);
					break;
			}
		};
		
		// Rest of Initialization Functions
		Embedded = _embedded;
		_actions.ButtonClicked += HandleActions;
		VisibilityChanged += VisibilityChanged_GodotPanel;
		_downloadSource.ItemSelected += index =>
		{
			ClearAllCategories();
			PopulateInstalled();
			switch (index)
			{
				case 0:
					SetOptionsDisabled();
					PopulateGithub("godot");
					break;
				case 1:
					UpdateCurrentTag();
					SetOptionsDisabled(false);
					PopulateGithub("godot-builds");
					break;
			}
		};

		_enginePopup.IndexPressed += (indx) =>
		{
			var gli = (GodotLineItem)_enginePopup.GetMeta("gli");
			switch (indx)
			{
				case 0L: // Install/Uninstall
					gli.EmitSignal(
						gli.IsInstalled
							? GodotLineItem.SignalName.UninstallClicked
							: GodotLineItem.SignalName.InstallClicked, gli);
					break;
				case 2L: // Make Default
					gli.EmitSignal(GodotLineItem.SignalName.MakeDefaultClick, gli);
					break;
				case 3L: // Share Settings
					gli.SettingsShare = !gli.SettingsShare;
					gli.EmitSignal(GodotLineItem.SignalName.SharedSettings, gli.SettingsShare);
					break;
				case 4L: // Link Settings
					gli.SettingsLinked = !gli.SettingsLinked;
					gli.EmitSignal(GodotLineItem.SignalName.LinkedSettings, gli.SettingsLinked);
					break;
				case 6L: // Copy Engine Path
					DisplayServer.Singleton.ClipboardSet(gli.GodotVersion.Location);
					OS.Alert($"Copied Path for {gli.GodotVersion.Tag} to clipboard.", "Copy Engine Path");
					break;
				case 7L: // Open Engine Path
					Util.LaunchWeb(gli.GodotVersion.Location);
					break;
				case 8L: // Launch Engine
					GodotRunner.RunEngine(gli.GodotVersion);
					break;
			}
		};
	}

	public override void _ExitTree()
	{
		foreach (var child in _installed.ItemList.GetChildren())
		{
			_installed.ItemList.RemoveChild(child);
			child.QueueFree();
		}
	
		foreach (var child in _downloading.ItemList.GetChildren())
		{
			_downloading.RemoveChild(child);
			child.QueueFree();
		}
	
		foreach (var child in _available.ItemList.GetChildren())
		{
			_available.ItemList.RemoveChild(child);
			child.QueueFree();
		}
	}

	#endregion
	
	#region Event Handlers
	private async void HandleActions(int index)
	{
		switch (index)
		{
			case 0:
				// Add Custom Godot
				var dlg = SceneNode<AddCustomGodot>.FromScene();
				AddChild(dlg);
				dlg.PopupCentered(new Vector2I(320,170));
				break;
			case 1:
				// Download from Website
				break;
			case 2:
				await CheckForUpdates();
				ClearAllCategories();
				
				PopulateInstalled();
				
				PopulateAvailable();
				
				SortChildren();
				break;
		}
	}

	private async Task CheckForUpdates()
	{
		_dialog = SceneNode<BusyDialog>.FromScene();
		_dialog.BylineText = "Fetching Github Release Information...";
		GetTree().Root.AddChild(_dialog);
		_dialog.PopupCentered(new Vector2I(352, 150));
		await _githubGodot.UpdateDatabase();
		Database.Settings.LastCheck = DateTime.UtcNow;
		Database.FlushDatabase();

		await _godotBuilds.UpdateDatabase();
		Database.Settings.LastMirrorCheck = DateTime.UtcNow;
		Database.FlushDatabase();
		_dialog.QueueFree();
	}

	private async void VisibilityChanged_GodotPanel()
	{
		if (!Visible) return;

		if (DateTime.UtcNow - Database.Settings.LastCheck >= Database.Settings.UpdateCheckInterval)
			await CheckForUpdates();
		
		ClearAllCategories();
		
		PopulateInstalled();
		
		PopulateAvailable();

		SortChildren();
	}

	private void HandleInstallCompleted(GodotLineItem item, GodotVersion version)
	{
		Util.RunInMainThread(() => 
		{
			_downloading.RemoveLineItem(item);
			_available.AddLineItem(item);
			item.Downloading = false;
			item.Visible = false;
			item = SceneNode<GodotLineItem>.FromScene();
			item.GodotVersion = version;
			SetupGLIEvents(item);
			_installed.AddLineItem(item);
			if (_downloading.GetItemCount() == 0) _downloading.Visible = false;
			
			Database.AddVersion(version);

			SortChildren();
		});
	}

	private void HandleUninstallCompleted(GodotLineItem item)
	{
		_installed.ItemList.RemoveChild(item);
		Database.RemoveVersion(item.GodotVersion);
		item.QueueFree();
		ClearAllCategories();
		PopulateInstalled();
		PopulateAvailable();
		SortChildren();
	}

	private void HandleDownloadCancelled(GodotLineItem item)
	{
		_downloading.ItemList.RemoveChild(item);
		_available.ItemList.AddChild(item);
		if (_downloading.GetItemCount() == 0) _downloading.Visible = false;
		SortChildren();
	}

	#endregion
	
	#region Private Support Functions
	private void SetOptionsDisabled(bool enabled = true)
	{
		for (var i = 2; i < 6; i++)
			_tagSelection.GetPopup().SetItemDisabled(i, enabled);
	}

	private void UpdateCurrentTag()
	{
		for (var i = 2; i < 6; i++)
		{
			if (_tagSelection.GetPopup().IsItemChecked(i))
				_currentTag = _tagsShort[i-2];
		}
	}
	
	private void PopulateGithub(string repo)
	{
		var installed = Database.AllVersions().ToArray();
		var tag = repo == "godot" ? "stable" : _currentTag;
		foreach (var version in Database.AllGithubVersions(repo).OrderByDescending(ver => ver.SemVersion, SemVersionCompare.Instance).Where(x => x.SemVersion.SpecialVersion.Contains(tag)))
		{
			switch (_showMono)
			{
				case true when version.CSharpArchiveSize == 0:
				case false when version.StandardArchiveSize == 0:
					continue;
			}
			
			var res = installed.FirstOrDefault(x => x.SemVersion == version.SemVersion && x.IsMono == _showMono);

			var item = SceneNode<GodotLineItem>.FromScene();
			item.GithubVersion = version;
			item.ShowMono = _showMono;
			SetupGLIEvents(item);
			_available.AddLineItem(item);
			item.Visible = (res == null);
		}
	}

	private void ToggleMono()
	{
		_showMono = !_showMono;
		_tagSelection.GetPopup().SetItemChecked(0, _showMono);
		UpdateAvailable();
	}

	private void ToggleTag(int id)
	{
		var tag = _tags[id];
		var stag = _tagsShort[id];
		_currentTag = stag;
		for (var i = 2; i < 6; i++)
			_tagSelection.GetPopup().SetItemChecked(i, i == id+2);
		ClearAllCategories();
		PopulateInstalled();
		PopulateGithub("godot-builds");
	}

	private void UpdateAvailable()
	{
		var installed = Database.AllVersions().ToArray();
		foreach (var child in _available.ItemList.GetChildren<GodotLineItem>())
		{
			child.ShowMono = _showMono;
			if (child.GithubVersion != null)
			{
				var res = installed.FirstOrDefault(x =>
					x.SemVersion == child.GithubVersion.SemVersion && x.IsMono == _showMono);
				child.Visible = (res == null);
			}
		}
	}

	private void PopulateInstalled()
	{
		foreach (var version in Database.AllVersions().OrderByDescending(ver => ver.Tag))
		{
			var item = SceneNode<GodotLineItem>.FromScene();
			item.GodotVersion = version;
			SetupGLIEvents(item);
			_installed.AddLineItem(item);
		}
	}

	private void PopulateAvailable()
	{
		switch(_downloadSource.Selected)
		{
			case 0: // Release
				PopulateGithub("godot");
				break;
			case 1: // Godot Builds
				PopulateGithub("godot-builds");
				break;
		}
	}

	private void SetupGLIEvents(GodotLineItem item)
	{
		item.InstallClicked += (gli) =>
		{
			if (gli.Downloading) return;
			_available.RemoveLineItem(gli);
			_downloading.Visible = true;
			_downloading.AddLineItem(gli);
			DownloadManager.Instance.StartDownload(gli);
		};
		item.UninstallClicked += async (gli) =>
		{
			var res = await UI.YesNoBox($"Uninstall {gli.GodotVersion.SemVersion}",
				"Are you sure you want to remove this version of Godot?", new Vector2I(320, 120));
			if (!res) return;
			GD.Print($"Removing Version {gli.GodotVersion.SemVersion}");
			InstallManager.Instance.UninstallVersion(gli);
		};
		item.MakeDefaultClick += gli =>
		{
			// Handle Setting up Godot Line Item as Default Engine for v3/v4 of Godot Engine.
			if (gli.GodotVersion is null) return;
			if (gli.IsDefault) return;
			if (gli.GodotVersion.SemVersion is null) return;
			switch (gli.GodotVersion.SemVersion.Version.Major)
			{
				case 4:
					Database.Settings.DefaultEngine4 = gli.GodotVersion;
					OS.Alert($"Updated Default engine for Godot 4.x to {gli.GodotVersion.Tag}", "Make Default Engine");
					break;
				case 3:
					Database.Settings.DefaultEngine3 = gli.GodotVersion;
					OS.Alert($"Updated Default engine for Godot 3.x to {gli.GodotVersion.Tag}", "Make Default Engine");
					break;
				default:
					OS.Alert("You cannot set a version less then 3 as Default Engine.", "Make Default Engine");
					break;
			}
			Database.FlushDatabase();

			foreach (var node in _installed.ItemList.GetChildren())
			{
				if (node is not GodotLineItem gle) continue;
				gle.Refresh();
			}
		};
		
		item.LinkedSettings += (gli, toggle) =>
		{

		};
		item.SharedSettings += (gli, toggle) =>
		{

		};
		item.RightClick += (gli) =>
		{
			if (!gli.IsInstalled)
			{
				foreach(var indx in new List<int> {2, 3, 4, 6, 7, 8})
					_enginePopup.SetItemDisabled(indx, true);
				_enginePopup.SetItemText(0, "Install");
			}
			else
			{
				foreach (var indx in new List<int> { 2, 3, 4, 6, 7, 8 })
					_enginePopup.SetItemDisabled(indx, false);
				_enginePopup.SetItemText(0, "Uninstall");
			}

			_enginePopup.SetMeta("gli", gli);
			_enginePopup.Popup(new Rect2I((GetScreenPosition() + GetLocalMousePosition()).ToVector2I(), Vector2I.Zero));
		};
		item.ExecuteClick += (gli) =>
		{
			GodotRunner.RunEngine(gli.GodotVersion);
		};
	}
	
	private void ClearAllCategories()
	{
		foreach (var child in _installed.GetGodotLineItems())
			child.QueueFree();
		foreach (var child in _available.GetGodotLineItems())
			child.QueueFree();
	}
	private void SortChildren()
	{
		var ordered = _available.GetGodotLineItems();
		if (ordered.Count == 0)
			return; // Safety just in case we have no version information downloaded.
		ordered = new Array<GodotLineItem>(ordered.OrderByDescending(i => i.GithubVersion.SemVersion,
			SemVersionCompare.Instance));
		foreach (var item in ordered)
		{
			_available.RemoveLineItem(item);
			_available.AddLineItem(item);
		}

		ordered = _installed.GetGodotLineItems();
		ordered = new Array<GodotLineItem>(ordered.OrderByDescending(i => i.GodotVersion.SemVersion,
			SemVersionCompare.Instance));
		foreach (var item in ordered)
		{
			_installed.RemoveLineItem(item);
			_installed.AddLineItem(item);
		}
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}
