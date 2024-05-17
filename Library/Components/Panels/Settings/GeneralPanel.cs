using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Managers;
using GodotManager.Library.Managers.UndoManager;
using GodotManager.Library.Utility;
using GodotManager.Scenes;

// namespace

namespace GodotManager.Library.Components.Panels.Settings;

public partial class GeneralPanel : MarginContainer
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private LineEditTimeout _godotPath;
	[NodePath] private Button _godotBrowse;
	[NodePath] private LineEditTimeout _cachePath;
	[NodePath] private Button _cacheBrowse;
	[NodePath] private OptionButton _projectView;
	[NodePath] private OptionButton _defaultEngine3;
	[NodePath] private OptionButton _defaultEngine4;

	[NodePath] private CheckBox _checkForUpdates;
	[NodePath] private OptionButton _checkInterval;
	
	[NodePath] private CheckBox _useProxy;
	[NodePath] private HBoxContainer _proxyContainer;
	[NodePath] private LineEditTimeout _proxyHost;
	[NodePath] private LineEditTimeout _proxyPort;

	[NodePath] private CheckBox _useSystem;
	
	[NodePath] private HBoxContainer _desktopEntry;
	[NodePath] private Button _createEntry;
	[NodePath] private Button _removeEntry;

	[NodePath] private CheckBox _useLastMirror;

	[NodePath] private CheckBox _noConsole;
	[NodePath] private CheckBox _selfContained;

	[NodePath] private ItemListWithButtons _assetMirrors;

	#endregion
	
	#region Singletons

	[Singleton]
	private SignalBus _signalBus;
	#endregion

	#region Private Variables

	private bool _setup = false;
	private string _enterInstall = "";
	private string _enterCache = "";
	private string _enterHost = "";
	private string _enterPort = "";
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		if (Platform.Get() != PlatformType.LinuxBSD)
			_desktopEntry.Visible = false;
		// Rest of Initialization Functions
		LoadSettings();
		SetupEventHandlers();
	}
	#endregion
	
	#region Event Handlers

	private void SetupEventHandlers()
	{
		var ProjectViewText = new List<string>() { "List View", "Icon View", "Category View" };
		var CheckIntervals = new List<double>() { 1, 12, 24, 168, 336, 720 };

		_godotPath.TextUpdated += (text) => HistoryManager.Push(new UndoItem<string>(
			Database.Settings.EnginePath,
			text,
			(newVal) => Database.Settings.EnginePath = newVal.NormalizePath(),
			(oldVal) =>
			{
				Database.Settings.EnginePath = oldVal.NormalizePath();
				_godotPath.Text = oldVal.NormalizePath();
			}
		));

		_godotBrowse.Pressed += async () =>
		{
			MainWindow.BrowseFolderDialog("Godot Install Directory", _godotPath.Text.NormalizePath(), _godotPath.Text.NormalizePath(), true,
				DisplayServer.FileDialogMode.OpenDir, new string[] { },
				Callable.From<bool, string[], int>((b, strings, i) => HandleBrowseDialog(b, strings, i, _godotPath)));
		};

		_cachePath.TextUpdated += (text) => HistoryManager.Push(new UndoItem<string>(
			Database.Settings.CachePath,
			text,
			(newVal) => Database.Settings.CachePath = newVal.NormalizePath(),
			(oldVal) =>
			{
				Database.Settings.EnginePath = oldVal.NormalizePath();
				_cachePath.Text = oldVal.NormalizePath();
			})
		);

		_cacheBrowse.Pressed += async () =>
		{
			MainWindow.BrowseFolderDialog("Godot Install Directory", _cachePath.Text.NormalizePath(), _cachePath.Text.NormalizePath(), true,
				DisplayServer.FileDialogMode.OpenDir, new string[] { },
				Callable.From<bool, string[], int>((b, strings, i) => HandleBrowseDialog(b, strings, i, _cachePath)));
		};

		_projectView.ItemSelected += index =>
		{
			if (_setup) return;
			HistoryManager.Push(new UndoItem<string>(
					Database.Settings.DefaultView,
					_projectView.GetItemText((int)index),
					(newVal) => Database.Settings.DefaultView = newVal,
					(oldVal) =>
					{
						Database.Settings.DefaultView = oldVal;
						_projectView.Selected = ProjectViewText.IndexOf(Database.Settings.DefaultView);
					}
				)
			);
		};

		_defaultEngine3.ItemSelected += index =>
		{
			var oldValue = Database.Settings.DefaultEngine3;
			var newValue = Database.GetVersion(_defaultEngine3.GetItemMetadata((int)index).AsInt32());
			HistoryManager.Push(new UndoItem<GodotVersion>(
					oldValue,
					newValue,
					(newVal) => Database.Settings.DefaultEngine3 = newVal,
					(oldVal) =>
					{
						Database.Settings.DefaultEngine3 = oldVal;
						for (var i = 0; i < _defaultEngine3.ItemCount; i++)
						{
							if (oldVal.Id != _defaultEngine3.GetItemMetadata(i).AsInt32()) continue;
							_defaultEngine3.Selected = i;
							break;
						}
					}
				)
			);
		};
		
		_defaultEngine4.ItemSelected += index =>
		{
			var oldValue = Database.Settings.DefaultEngine4;
			var newValue = Database.GetVersion(_defaultEngine4.GetItemMetadata((int)index).AsInt32());
			HistoryManager.Push(new UndoItem<GodotVersion>(
					oldValue,
					newValue,
					(newVal) => Database.Settings.DefaultEngine4 = newVal,
					(oldVal) =>
					{
						Database.Settings.DefaultEngine4 = oldVal;
						for (var i = 0; i < _defaultEngine4.ItemCount; i++)
						{
							if (oldVal.Id != _defaultEngine4.GetItemMetadata(i).AsInt32()) continue;
							_defaultEngine4.Selected = i;
							break;
						}
					}
				)
			);
		};

		_checkForUpdates.Toggled += (toggle) =>
		{
			if (_setup) return;
			HistoryManager.Push(new UndoItem<bool>(
					!toggle,
					toggle,
					(newVal) => Database.Settings.CheckForEngineUpdates = newVal,
					(oldVal) =>
					{
						_setup = true;
						Database.Settings.CheckForEngineUpdates = oldVal;
						_checkForUpdates.ButtonPressed = oldVal;
						_setup = false;
					}
				)
			);
		};

		_checkInterval.ItemSelected += (index) => HistoryManager.Push(new UndoItem<int>(
				CheckIntervals.IndexOf(Database.Settings.UpdateCheckInterval.TotalHours),
				(int)index,
				(newVal) => Database.Settings.UpdateCheckInterval = TimeSpan.FromHours(CheckIntervals[newVal]),
				(oldVal) =>
				{
					Database.Settings.UpdateCheckInterval = TimeSpan.FromHours(CheckIntervals[oldVal]);
					_checkInterval.Selected = oldVal;
				}
			)
		);

		_useProxy.Toggled += (toggle) =>
		{
			if (_setup) return;
			_proxyContainer.Visible = toggle;
			HistoryManager.Push(new UndoItem<bool>(
					!toggle,
					toggle,
					(newVal) => Database.Settings.UseProxy = newVal,
					(oldVal) =>
					{
						_setup = true;
						Database.Settings.UseProxy = oldVal;
						_useProxy.ButtonPressed = oldVal;
						_proxyContainer.Visible = oldVal;
						_setup = false;
					}
				)
			);
		};

		_proxyHost.TextUpdated += (value) => HistoryManager.Push(new UndoItem<string>(
				Database.Settings.ProxyHost,
				value,
				(newVal) => Database.Settings.ProxyHost = newVal,
				(oldVal) =>
				{
					Database.Settings.ProxyHost = oldVal;
					_proxyHost.Text = oldVal;
				}
			)
		);

		_proxyPort.TextUpdated += (value) =>
		{
			if (!int.TryParse(value, out var intVal))
				intVal = 80;

			HistoryManager.Push(new UndoItem<int>(
					Database.Settings.ProxyPort,
					intVal,
					(newVal) => Database.Settings.ProxyPort = newVal,
					(oldVal) =>
					{
						Database.Settings.ProxyPort = oldVal;
						_proxyPort.Text = $"{oldVal}";
					}
				)
			);
		};

		_useSystem.Toggled += (value) =>
		{
			if (_setup) return;
			HistoryManager.Push(new UndoItem<bool>(
					!value,
					value,
					(newVal) => Database.Settings.UseSystemTitlebar = newVal,
					(oldVal) =>
					{
						_setup = true;
						Database.Settings.UseSystemTitlebar = oldVal;
						_useSystem.ButtonPressed = oldVal;
						_setup = false;
					}
				)
			);
		};

		_createEntry.Pressed += () => { };
		_removeEntry.Pressed += () => { };

		_useLastMirror.Toggled += (toggle) =>
		{
			if (_setup) return;
			HistoryManager.Push(new UndoItem<bool>(
				!toggle,
				toggle,
				(newVal) => Database.Settings.UseLastMirror = newVal,
				(oldVal) =>
				{
					_setup = true;
					Database.Settings.UseLastMirror = oldVal;
					_useLastMirror.ButtonPressed = oldVal;
					_setup = false;
				})
			);
		};

		_noConsole.Toggled += (toggle) =>
		{
			if (_setup) return;
			HistoryManager.Push(new UndoItem<bool>(
				!toggle,
				toggle,
				(newVal) => Database.Settings.NoConsole = newVal,
				(oldVal) =>
				{
					_setup = true;
					Database.Settings.NoConsole = oldVal;
					_noConsole.ButtonPressed = oldVal;
					_setup = false;
				})
			);
		};

		_selfContained.Toggled += (toggle) =>
		{
			if (_setup) return;
			HistoryManager.Push(new UndoItem<bool>(
				!toggle,
				toggle,
				(newVal) => Database.Settings.SelfContainedEditors = newVal,
				(oldVal) =>
				{
					_setup = true;
					Database.Settings.SelfContainedEditors = oldVal;
					_selfContained.ButtonPressed = oldVal;
					_setup = false;
				})
			);
		};
	}
	
	private void HandleBrowseDialog(bool status, string[] selectedPaths, int filterIndex, LineEditTimeout updateNode)
	{
		if (selectedPaths.Length <= 0) return;
		
		updateNode.Text = selectedPaths[0].NormalizePath();
		updateNode.EmitSignal(LineEditTimeout.SignalName.TextUpdated, selectedPaths[0].NormalizePath());
	}
	#endregion
	
	#region Private Support Functions

	public void LoadSettings()
	{
		var ProjectViewText = new List<string>() { "List View", "Icon View", "Category View", "Last View Used" };
		var CheckIntervals = new List<double>() { 1, 12, 24, 168, 336, 720 };
		_setup = true;
		_godotPath.Text = Database.Settings.EnginePath.NormalizePath();
		_cachePath.Text = Database.Settings.CachePath.NormalizePath();
		
		// Handle Populating Godot 3.x Versions
		
		// Handle Populating Godot 4.x Versions

		_checkForUpdates.ButtonPressed = Database.Settings.CheckForEngineUpdates;
		_projectView.Selected = ProjectViewText.IndexOf(Database.Settings.DefaultView);
		_checkInterval.Selected = CheckIntervals.IndexOf(Database.Settings.UpdateCheckInterval.TotalHours);

		_useProxy.ButtonPressed = Database.Settings.UseProxy;
		_proxyContainer.Visible = Database.Settings.UseProxy;
		_proxyHost.Text = Database.Settings.ProxyHost;
		_proxyPort.Text = Database.Settings.ProxyPort.ToString();
		
		_useSystem.ButtonPressed = Database.Settings.UseSystemTitlebar;
		_useLastMirror.ButtonPressed = Database.Settings.UseLastMirror;
		_noConsole.ButtonPressed = Database.Settings.NoConsole;
		_selfContained.ButtonPressed = Database.Settings.SelfContainedEditors;
		
		PopulateInstalled();

		var defEng3 = Database.Settings.DefaultEngine3;
		var defEng4 = Database.Settings.DefaultEngine4;

		if (defEng3 == null)
			_defaultEngine3.Selected = 0;
		else
		{
			for (var i = 0; i < _defaultEngine3.ItemCount; i++)
			{
				if (defEng3.Id == _defaultEngine3.GetItemMetadata(i).AsInt32())
					_defaultEngine3.Selected = i;
			}
		}

		if (defEng4 == null)
			_defaultEngine4.Selected = 0;
		else
		{
			for (var i = 0; i < _defaultEngine4.ItemCount; i++)
			{
				if (defEng4.Id == _defaultEngine4.GetItemMetadata(i).AsInt32())
					_defaultEngine4.Selected = i;
			}
		}
		_setup = false;
	}

	private void PopulateInstalled()
	{
		_defaultEngine3.Clear();
		_defaultEngine4.Clear();
		
		_defaultEngine3.AddItem("None Selected");
		_defaultEngine3.SetItemMetadata(0, "-1");
		foreach (var version in Database.AllVersion3())
		{
			_defaultEngine3.AddItem(version.GetHumanReadableVersion());
			_defaultEngine3.SetItemMetadata(_defaultEngine3.ItemCount - 1, version.Id);
		}

		_defaultEngine4.AddItem("None Selected");
		_defaultEngine4.SetItemMetadata(0, "-1");
		foreach (var version in Database.AllVersion4())
		{
			_defaultEngine4.AddItem(version.GetHumanReadableVersion());
			_defaultEngine4.SetItemMetadata(_defaultEngine4.ItemCount - 1, version.Id);
		}
	}
	#endregion

	#region Public Support Functions

	#endregion
}