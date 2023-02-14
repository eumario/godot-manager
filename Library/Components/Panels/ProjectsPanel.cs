using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Dialogs;
using Octokit;

// namespace
namespace GodotManager.Library.Components.Panels;

public partial class ProjectsPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths
	[NodePath] private ActionButtons _actionButtons;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion
	
	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		_actionButtons.ButtonClicked += OnButtonClicked_ActionButtons;
		//_actionButtons.SetHidden(3,4,5,6);
	}
	#endregion

	#region Event Handlers
	private void OnButtonClicked_ActionButtons(int index)
	{
		switch (index)
		{
			case 0: // New Project
				var np = CreateProject.FromScene();
				AddChild(np);
				np.PopupCentered(new Vector2I(600,470));
				break;
			case 1: // Import Project
				var dlg = ImportProject.FromScene();
				dlg.ImportCompleted += () => { dlg.QueueFree(); };
				AddChild(dlg);
				dlg.PopupCentered(new Vector2I(320,145));
				break;
			case 2: // Scan Folders
				break;
			case 3: // Add Category
				break;
			case 4: // Remove Category
				break;
			case 5: // Delete
				break;
			case 6: // Remove Missing
				break;
		}
	}
	
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}