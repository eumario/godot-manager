using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Utility;

// namespace

namespace GodotManager.Library.Components.Panels.Settings;

public partial class LicensesPanel : MarginContainer
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private RichTextLabel _mitLicense;
	[NodePath] private RichTextLabel _apacheLicense;
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
		_mitLicense.Name = "MIT License";
		_apacheLicense.Name = "Apache License";

		_mitLicense.MetaClicked += (meta) => Util.LaunchWeb(meta.ToString());
		_apacheLicense.MetaClicked += (meta) => Util.LaunchWeb(meta.ToString());
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}