using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;

// namespace

namespace GodotManager.Library.Components.Dialogs;

[Tool]
[SceneNode("res://Library/Components/Dialogs/ManageCustomDownloads.tscn")]
public partial class ManageCustomDownloads : AcceptDialog
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private ItemListWithButtons _customVersionList;
	
	[NodePath] private LineEdit _name;
	[NodePath] private LineEdit _url;
	[NodePath] private OptionButton _interval;
	[NodePath] private LineEdit _tagName;
	[NodePath] private Button _saveUpdates;
	
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
		AddCancelButton("Cancel");
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}