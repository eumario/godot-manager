using System;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data;

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
	private Managers.Github.Godot _godot = null;
	private Managers.Github.GodotManager _godotManager = null;
	private BusyDialog _dialog;
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

		_godot = new Managers.Github.Godot();
		_godot.ReleaseCount += (count) =>
		{
			_dialog.BylineText = $"Processing 0 of {count} releases...";
		};

		_godot.ReleaseProgress += (current, max) =>
		{
			_dialog.BylineText = $"Processing {current} of {max} releases...";
		};
		
		// Rest of Initialization Functions
		Embedded = _embedded;
		_actions.ButtonClicked += HandleActions;
		VisibilityChanged += VisibilityChanged_GodotPanel;

	}
	#endregion
	
	#region Event Handlers

	private void HandleActions(int index)
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
				// Check for Updates
				break;
		}
	}

	private async void VisibilityChanged_GodotPanel()
	{
		{
			if (!Visible) return;

			if (DateTime.UtcNow - Database.Settings.LastCheck >= Database.Settings.UpdateCheckInterval)
			{
				_dialog = BusyDialog.FromScene();
				_dialog.BylineText = "Fetching Release Information...";
				GetTree().Root.AddChild(_dialog);
				_dialog.PopupCentered(new Vector2I(352, 150));
				await _godot.UpdateDatabase();
				Database.Settings.LastCheck = DateTime.UtcNow;
				Database.FlushDatabase();
				_dialog.QueueFree();
			}

			foreach (var child in _available.ItemList.GetChildren())
				child.QueueFree();

			foreach (var version in Database.AllGithubVersions().OrderByDescending(ver => ver.Release.TagName))
			{
				var item = GodotLineItem.FromScene();
				item.GithubVersion = version;
				_available.ItemList.AddChild(item);
			}

			SortChildren();
		};
	}
	#endregion
	
	#region Private Support Functions

	private void SortChildren()
	{
		
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}
