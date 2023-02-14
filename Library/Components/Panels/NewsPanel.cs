using Godot;
using Godot.Sharp.Extras;

// namespace
namespace GodotManager.Library.Components.Panels;

public partial class NewsPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths
	[NodePath] private Button _refreshNews;
	[NodePath] private VBoxContainer _newsList;
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
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}
