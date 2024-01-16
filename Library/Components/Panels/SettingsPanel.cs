using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data;
using GodotManager.Library.Managers;
using GodotManager.Library.Managers.UndoManager;

// namespace

namespace GodotManager.Library.Components.Panels;

public partial class SettingsPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private ActionButtons _undoSave;
	[NodePath] private Button _general;
	[NodePath] private Button _projects;
	[NodePath] private Button _about;
	[NodePath] private Button _contributions;
	[NodePath] private Button _licenses;
	[NodePath] private TabContainer _optionPanels;
	
	#region General Tab
	#endregion
	
	
	#endregion
	
	#region Singleton

	[Singleton] private SignalBus _signalBus;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		var buttonGroup = new ButtonGroup();
		foreach (var button in new List<Button> { _general, _projects, _about, _contributions, _licenses })
		{
			button.ButtonGroup = buttonGroup;
		}
		_undoSave.ButtonClicked += (index) =>
		{
			if (index == 0)
			{
				// Handle Saving
				HistoryManager.Apply();
				Database.SaveSettings();
				Database.FlushDatabase();
				_undoSave.Visible = false;
			}
			else
			{
				// Handle Undoing
				HistoryManager.Undo();
				_undoSave.Visible = false;
			}
		};

		_undoSave.Visible = false;

		_signalBus.SettingsChanged += () => _undoSave.Visible = true;
		_signalBus.SettingsSaved += () => _undoSave.Visible = false;
		
		_general.Pressed += () => _optionPanels.CurrentTab = 0;
		_projects.Pressed += () => _optionPanels.CurrentTab = 1;
		_about.Pressed += () => _optionPanels.CurrentTab = 2;
		_contributions.Pressed += () => _optionPanels.CurrentTab = 3;
		_licenses.Pressed += () => _optionPanels.CurrentTab = 4;
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}