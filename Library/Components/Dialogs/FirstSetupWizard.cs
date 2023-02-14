using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Panels;

// namespace

namespace GodotManager.Library.Components.Dialogs;

public partial class FirstSetupWizard : Window
{
	#region Signals
	#endregion
	
	#region Quick Create
	public static FirstSetupWizard FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/FirstSetupWizard.tscn");
		return scene.Instantiate<FirstSetupWizard>();
	}
	#endregion
	
	#region Node Paths
	[NodePath] private TabContainer _wizard;
	
	[NodePath] private BrowseLine _engineLoc;
	[NodePath] private BrowseLine _cacheLoc;
	[NodePath] private BrowseLine _projectLoc;

	[NodePath] private CheckBox _createDesktop;
	[NodePath] private CheckBox _allUsers;

	[NodePath] private GodotPanel _godotPanel;

	[NodePath] private Button _previous;
	[NodePath] private Button _cancel;
	[NodePath] private Button _next;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		_previous.Pressed += () =>
		{
			if (_wizard.CurrentTab - 1 == 0)
				_previous.Disabled = true;
			if (_wizard.CurrentTab - 1 >= 0) _wizard.CurrentTab -= 1;
			if (_next.Text == "Finished") _next.Text = "Next";
		};
		_next.Pressed += () =>
		{
			if (_previous.Disabled) _previous.Disabled = false;
			if (_wizard.CurrentTab + 1 == _wizard.GetTabCount() - 1) _next.Text = "Finished";
			if (_wizard.CurrentTab + 1 > _wizard.GetTabCount() - 1) FinishSetup();
			if (_wizard.CurrentTab + 1 <= _wizard.GetTabCount() - 1) _wizard.CurrentTab += 1;
		};
		_cancel.Pressed += () =>
		{
			Hide();
			QueueFree();
		};
		CloseRequested += () =>
		{
			Hide();
			QueueFree();
		};
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	void FinishSetup()
	{
		GD.Print("Finishing Wizard Setup.");
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}