using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Utility;

// namespace

namespace GodotManager.Library.Components.Panels.Settings;

public partial class ContributionsPanel : MarginContainer
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private RichTextLabel _links;
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
		_links.MetaClicked += (meta) => Util.LaunchWeb(meta.AsString());
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}