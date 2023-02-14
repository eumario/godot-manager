using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Dialogs;

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
		
		// Rest of Initialization Functions
		Embedded = _embedded;
		_actions.ButtonClicked += HandleActions;
	}
	#endregion
	
	#region Event Handlers

	void HandleActions(int index)
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
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}
