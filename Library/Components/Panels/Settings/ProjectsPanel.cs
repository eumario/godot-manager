using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data;
using GodotManager.Library.Managers.UndoManager;
using GodotManager.Library.Utility;
using GodotManager.Scenes;

// namespace

namespace GodotManager.Library.Components.Panels.Settings;

public partial class ProjectsPanel : MarginContainer
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private LineEditTimeout _projectPath;
	[NodePath] private Button _browseProject;
	[NodePath] private CheckBox _exitOnLaunch;
	[NodePath] private CheckBox _scanOnStartup;
	[NodePath] private ItemListWithButtons _scanDirs;
	#endregion
	
	#region Private Variables

	private bool _setup = false;
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		LoadSettings();
		SetupEventHandlers();
	}
	#endregion
	
	#region Event Handlers

	private void SetupEventHandlers()
	{
		_projectPath.TextUpdated += (text) => HistoryManager.Push(new UndoItem<string>(
				Database.Settings.ProjectPath,
				text,
				(newVal) => Database.Settings.ProjectPath = newVal.NormalizePath(),
				(oldVal) =>
				{
					Database.Settings.ProjectPath = oldVal.NormalizePath();
					_projectPath.Text = oldVal.NormalizePath();
				}
			)
		);
		_browseProject.Pressed += async () =>
		{
			MainWindow.BrowseFolderDialog("Default Project Path", _projectPath.Text, _projectPath.Text, true,
				DisplayServer.FileDialogMode.OpenDir, new string[] { },
				Callable.From<bool, string[], int>(HandleBrowseDialog));
		};

		_exitOnLaunch.Toggled += (toggle) => HistoryManager.Push(new UndoItem<bool>(
				!toggle,
				toggle,
				(newVal) => Database.Settings.CloseManagerOnEdit = newVal,
				(oldVal) =>
				{
					_setup = true;
					Database.Settings.CloseManagerOnEdit = oldVal;
					_exitOnLaunch.ButtonPressed = oldVal;
					_setup = false;
				}
			)
		);

		_scanOnStartup.Toggled += (toggle) => HistoryManager.Push(new UndoItem<bool>(
				!toggle,
				toggle,
				(newVal) => Database.Settings.EnableAutoScan = newVal,
				(oldVal) =>
				{
					_setup = true;
					Database.Settings.EnableAutoScan = oldVal;
					_scanOnStartup.ButtonPressed = oldVal;
					_setup = false;
				}
			)
		);

		_scanDirs.ItemAdd += () =>
		{
			MainWindow.BrowseFolderDialog("Add Project Directory", _projectPath.Text, _projectPath.Text, true,
				DisplayServer.FileDialogMode.OpenDir, new string[] { },
				Callable.From<bool, string[], int>(HandleAddProjectDir));
		};
		_scanDirs.ItemEdit += () =>
		{
			var item = _scanDirs.GetSelected();
			if (item < 0) return;
			var oldDir = _scanDirs.GetItemText(item);
			MainWindow.BrowseFolderDialog("Edit Project Directory", oldDir, oldDir, true, DisplayServer.FileDialogMode.OpenDir, new string[] {},
				Callable.From<bool, string[], int>(HandleEditProjectDir));
		};
		_scanDirs.ItemRemove += async () =>
		{
			var item = _scanDirs.GetSelected();
			if (item < 0) return;
			var dirRem = _scanDirs.GetItemText(item);
			var res = await UI.YesNoBox("Remove Project Directory", $"Are you sure you want to remove '{dirRem}' from list of folders to monitor?");
			if (res)
			{
				HistoryManager.Push(new UndoItem<string>(
					dirRem,
					dirRem,
					(newValue) =>
					{
						Database.Settings.ScanDirs.Remove(newValue);
					},
					(oldValue) =>
					{
						Database.Settings.ScanDirs.Insert(item, oldValue);
						var newIndex = _scanDirs.GetItemCount();
						_scanDirs.AddItem(oldValue);
						_scanDirs.MoveItem(newIndex, item);
					}
					));
				_scanDirs.RemoveItem(item);
			}
		};
	}
	#endregion
	
	#region Private Support Functions

	private void HandleBrowseDialog(bool status, string[] selectedPaths, int filterIndex)
	{
		if (selectedPaths.Length <= 0) return;
		
		_projectPath.Text = selectedPaths[0].NormalizePath();
		_projectPath.EmitSignal(LineEditTimeout.SignalName.TextUpdated, selectedPaths[0].NormalizePath());
	}

	private void HandleAddProjectDir(bool status, string[] selectedPaths, int filterIndex)
	{
		foreach (var path in selectedPaths)
		{
			HistoryManager.Push(new UndoItem<string>(
					path,
					path,
					(newVal) =>
					{
						Database.Settings.ScanDirs.Add(newVal);
					},
					(oldVal) =>
					{
						Database.Settings.ScanDirs.Remove(oldVal);
						foreach (var (item, index) in _scanDirs.GetAllItems().WithIndex())
						{
							if (item != oldVal) continue;
							_scanDirs.RemoveItem(index);
							break;
						}
					})
			);
			_scanDirs.AddItem(path);
		}
	}

	private void HandleEditProjectDir(bool status, string[] selectedPaths, int filterIndex)
	{
		var indx = _scanDirs.GetSelected();
		var oldPath = _scanDirs.GetItemText(indx);
		var newPath = selectedPaths[0];
		HistoryManager.Push(new UndoItem<string>(
			oldPath,
			newPath,
			(newVal) =>
			{
				Database.Settings.ScanDirs[indx] = newVal;
			},

			(oldVal) =>
			{
				Database.Settings.ScanDirs[indx] = oldVal;
				_scanDirs.SetItemText(indx, oldVal);
			}
			));
		_scanDirs.SetItemText(indx, newPath);
	}
	
	public void LoadSettings()
	{
		_setup = true;
		_projectPath.Text = Database.Settings.ProjectPath;
		_exitOnLaunch.ButtonPressed = Database.Settings.CloseManagerOnEdit;
		_scanOnStartup.ButtonPressed = Database.Settings.EnableAutoScan;

		_scanDirs.Clear();

		foreach (var dir in Database.Settings.ScanDirs)
			_scanDirs.AddItem(dir);
		_setup = false;
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}