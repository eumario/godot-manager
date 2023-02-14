using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Panels;

public partial class SettingsPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private Button _general;
	[NodePath] private Button _projects;
	[NodePath] private Button _about;
	[NodePath] private Button _contributions;
	[NodePath] private Button _licenses;
	[NodePath] private TabContainer _optionPanels;
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
		_general.Pressed += () => _optionPanels.CurrentTab = 0;
		_projects.Pressed += () => _optionPanels.CurrentTab = 1;
		_about.Pressed += () => _optionPanels.CurrentTab = 2;
		_contributions.Pressed += () => _optionPanels.CurrentTab = 3;
		_licenses.Pressed += () => _optionPanels.CurrentTab = 4;
		GD.Print(_general.ButtonGroup.GetButtons().ToList().Count);
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}