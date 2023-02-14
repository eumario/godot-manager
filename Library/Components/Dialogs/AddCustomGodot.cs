using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;

// namespace
namespace GodotManager.Library.Components.Dialogs;

[Tool]
public partial class AddCustomGodot : AcceptDialog
{
	#region Signals
	#endregion
	
	#region Quick Create
	public static AddCustomGodot FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/AddCustomGodot.tscn");
		return scene.Instantiate<AddCustomGodot>();
	}
	#endregion
	
	#region Node Paths
	[NodePath] private LineEdit _tagName;
	[NodePath] private LineEdit _location;
	[NodePath] private Button _browseLocation;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		AddCancelButton("Cancel");
		_browseLocation.Pressed += () =>
		{
			var dlg = new FileDialog();
			dlg.CurrentDir = Database.Settings.EnginePath + "/";
			dlg.Access = FileDialog.AccessEnum.Filesystem;
			#if GODOT_WINDOWS
			dlg.AddFilter("*.exe", "Godot Executable");
			#elif GODOT_MACOS || GODOT_OSX
			dlg.AddFilter("*.app", "Godot AppDir")
			#else
			dlg.AddFilter("*.x86_64, *.x86_32, *.64, *.32", "Godot Executable");
			#endif
			dlg.TreeExited += () => dlg.QueueFree();
			dlg.FileSelected += (file) => _location.Text = file;
			AddChild(dlg);
			dlg.PopupCentered(new Vector2I(600,300));
		};
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}
