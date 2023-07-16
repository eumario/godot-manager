using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
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
	#endregion
	
	#region Private Variables
	private bool _embedded = false;
	private Managers.Github.Godot _githubGodot;
	private Managers.Tuxfamily.Godot _tuxfamilyGodot;
	private BusyDialog _dialog;
	private bool _showMono = false;
	private string[] _tags = {"Stable", "Developer Preview", "Alpha", "Beta", "Release Candidate"};
	private string[] _tagsShort = { "Stable", "Dev", "Alpha", "Beta", "RC" };
	private string _currentTag = "Stable";
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

		_githubGodot = new Managers.Github.Godot();
		_githubGodot.ReleaseCount += (count) =>
		{
			_dialog.BylineText = $"Processing 0 of {count} releases...";
		};

		_githubGodot.ReleaseProgress += (current, max) =>
		{
			_dialog.BylineText = $"Processing {current} of {max} releases...";
		};

		_tuxfamilyGodot = new Managers.Tuxfamily.Godot();
		_tuxfamilyGodot.ReleaseCount += (count) =>
		{
			_dialog.BylineText = $"Processing 0 of {count} releases...";
		};

		_tuxfamilyGodot.ReleaseProgress += (current, max) =>
		{
			_dialog.BylineText = $"Processing {current} of {max} releases...";
		};

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
				case 7:
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
			switch (index)
			{
				case 0:
					SetOptionsDisabled();
					PopulateGithub();
					break;
				case 1:
					SetOptionsDisabled(false);
					PopulateTuxfamily();
					break;
			}
		};
	}
	#endregion
	
	#region Event Handlers
	private async void HandleActions(int index)
	{
		switch (index)
		{
			case 0:
				// Add Custom Godot
				var dlg = AddCustomGodot.FromScene();
				AddChild(dlg);
				dlg.PopupCentered(new Vector2I(320,170));
				break;
			case 1:
				// Download from Website
				break;
			case 2:
				await CheckForUpdates();
				ClearAllCategories();
				
				if (_downloadSource.Selected == 0)
					PopulateGithub();
				else
					PopulateTuxfamily();
				
				SortChildren();
				break;
		}
	}

	private async Task CheckForUpdates()
	{
		_dialog = BusyDialog.FromScene();
		_dialog.BylineText = "Fetching Github Release Information...";
		GetTree().Root.AddChild(_dialog);
		_dialog.PopupCentered(new Vector2I(352, 150));
		await _githubGodot.UpdateDatabase();
		Database.Settings.LastCheck = DateTime.UtcNow;
		Database.FlushDatabase();

		_dialog.BylineText = "Fetching Tuxfamily Release Information...";
		await _tuxfamilyGodot.UpdateDatabase();
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

		if (_downloadSource.Selected == 0)
			PopulateGithub();
		else
			PopulateTuxfamily();

		SortChildren();
	}

	private void HandleInstallCompleted(GodotLineItem item, GodotVersion version)
	}
	private void HandleDownloadCancelled(GodotLineItem item)
	}
	#endregion
	#region Private Support Functions
	private void SetOptionsDisabled(bool enabled = true)
	{
		for (var i = 2; i < 7; i++)
			_tagSelection.GetPopup().SetItemDisabled(i, enabled);
	}
	
	private void PopulateGithub()
	{
		foreach (var version in Database.AllGithubVersions().OrderByDescending(ver => ver.Release.TagName))
		{
			switch (_showMono)
			{
				case true when version.CSharpArchiveSize == 0:
				case false when version.StandardArchiveSize == 0:
					continue;
			}

			var item = GodotLineItem.FromScene();
			item.GithubVersion = version;
			item.ShowMono = _showMono;
			SetupGLIEvents(item);
			_available.ItemList.AddChild(item);
		}
	}
	
	private void PopulateTuxfamily()
	{
		foreach (var version in Database.AllTuxfamilyVersions().Where(x => x.ReleaseStage.Contains(_currentTag))
			         .OrderByDescending(x => x.SemVersion, SemVersionCompare.Instance))
		{
			switch (_showMono)
			{
				case true when version.CSharpDownloadSize == 0:
				case false when version.StandardDownloadSize == 0:
					continue;
			}

			if (!version.ReleaseStage.Contains(_currentTag)) continue;

			var installed = Database.AllVersions().Where(t => t.IsMono == _showMono).ToArray();
			if (installed.FirstOrDefault(x => x.TuxfamilyVersion == version) != null ||
			    (installed.FirstOrDefault(x => x.GithubVersion.Release.TagName == version.TagName) != null &&
			     version.ReleaseStage == "Stable"))
				continue;

			var item = GodotLineItem.FromScene();
			item.ShowMono = _showMono;
			item.TuxfamilyVersion = version;
			SetupGLIEvents(item);
			_available.ItemList.AddChild(item);
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
		for (var i = 2; i < 7; i++)
			_tagSelection.GetPopup().SetItemChecked(i, i == id+2);
		ClearAllCategories();
		PopulateTuxfamily();
	}

	private void UpdateAvailable()
	{
		foreach (var child in _available.ItemList.GetChildren<GodotLineItem>())
			child.ShowMono = _showMono;
	}

	private void PopulateInstalled()
	{
		foreach (var version in Database.AllVersions().OrderByDescending(ver => ver.Tag))
		{
			var item = GodotLineItem.FromScene();
			item.GodotVersion = version;
			SetupGLIEvents(item);
			_installed.ItemList.AddChild(item);
		}
	}

	private void PopulateAvailable()
	{
		switch(_downloadSource.Selected)
		{
			case 0: // Github
				PopulateGithub();
				break;
			case 1: // TuxFamily
				PopulateTuxfamily();
				break;
		}
	}

	private void SetupGLIEvents(GodotLineItem item)
	{
		item.InstallClicked += (gli) =>
		{
		};
		item.UninstallClicked += (gli) =>
		{

		};
		item.LinkedSettings += (gli, toggle) =>
		{

		};
		item.SharedSettings += (gli, toggle) =>
		{

		};
		item.RightClick += (gli) =>
		{

		};
		item.ExecuteClick += (gli) =>
		{

		};
	}
	
	private void ClearAllCategories()
	{
		foreach (var child in _installed.ItemList.GetChildren())
			child.QueueFree();
		foreach (var child in _available.ItemList.GetChildren())
			child.QueueFree();
	}
	private void SortChildren()
	{
		var ordered = _available.ItemList.GetChildren<GodotLineItem>().OrderByDescending(
			i => i.GithubVersion?.SemVersion ?? i.TuxfamilyVersion.SemVersion, SemVersionCompare.Instance);
		foreach (var item in ordered)
		{
			_available.ItemList.RemoveChild(item);
			_available.ItemList.AddChild(item);
		}

		ordered = _installed.ItemList.GetChildren<GodotLineItem>().OrderByDescending(
			i => i.GodotVersion.GithubVersion?.SemVersion ?? i.GodotVersion.TuxfamilyVersion.SemVersion,
			SemVersionCompare.Instance);
		foreach (var item in ordered)
		{
			_installed.ItemList.RemoveChild(item);
			_installed.ItemList.AddChild(item);
		}
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}
