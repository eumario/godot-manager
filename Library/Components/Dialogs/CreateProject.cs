using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;

// namespace

namespace GodotManager.Library.Components.Dialogs;

[SceneNode("res://Library/Components/Dialogs/CreateProject.tscn")]
public partial class CreateProject : AcceptDialog
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private LineEdit _projectName;
	[NodePath] private Button _createFolder;
	
	[NodePath] private LineEdit _projectLocation;
	[NodePath] private TextureRect _validIcon;
	[NodePath] private Button _browseLocation;

	[NodePath] private CheckBox _godot3Version;
	[NodePath] private CheckBox _godot4Version;

	[NodePath] private OptionButton _templateSelection;
	[NodePath] private OptionButton _godotVersionSelection;

	[NodePath] private HBoxContainer _godot3Rendering;
	[NodePath("%GLES3")] private CheckBox _gles3;
	[NodePath("%GLES2")] private CheckBox _gles2;

	[NodePath] private HBoxContainer _godot4Rendering;
	[NodePath] private CheckBox _forwardPlus;
	[NodePath] private CheckBox _mobile;
	[NodePath] private CheckBox _compatability;
	[NodePath] private RichTextLabel _renderingInfo;
	#endregion
	
	#region Private Variables
	private const string ForwardPlusDesc = "" +
			"•  Supports desktop platforms only.\n" +
			"•  Advaned 3D graphics available.\n" +
			"•  Can scale to large complex scenes.\n" +
			"•  Use RenderingDevice backend.\n" +
			"•  Slower rendering of simple scenes.";

	private const string MobileDesc = "" +
			"•  Supports desktop + mobile platforms.\n" +
			"•  Less advanced 3D graphics.\n" +
			"•  Less scalable for complex scenes.\n" + 
			"•  Uses RenderingDevice backend.\n" +
			"•  Fast rendering of simple scenes.";

	private const string CompatabilityDesc = "" +
			"•  Supports desktop, mobile + web platforms.\n" +
			"•  Least advanced 3D graphics (currently work-in-progress).\n" +
			"•  Intended for low-end/older devices.\n" +
			"•  Uses OpenGL 3 backend (OpenGL 3.3/ES 3.0/WebGL2).\n" +
			"•  Fastest rendering of simple scenes.";
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		_godot3Version.Pressed += () =>
		{
			_godot3Rendering.Visible = _godot3Version.ButtonPressed;
			_godot4Rendering.Visible = !_godot3Version.ButtonPressed;

			var defEng = Database.Settings.DefaultEngine3;
			var selected = 0;

			_godotVersionSelection.Clear();
			foreach (var version in Database.AllVersion3())
			{
				_godotVersionSelection.AddItem(version.GetHumanReadableVersion());
				_godotVersionSelection.SetItemMetadata(_godotVersionSelection.ItemCount - 1, version.Id);
				selected = version.Id == defEng.Id ? _godotVersionSelection.ItemCount - 1 : 0;
			}

			_godotVersionSelection.Selected = selected;
		};

		_godot4Version.Pressed += () =>
		{
			_godot4Rendering.Visible = _godot4Version.ButtonPressed;
			_godot3Rendering.Visible = !_godot4Version.ButtonPressed;

			var defEng = Database.Settings.DefaultEngine4;
			var selected = 0;
			
			_godotVersionSelection.Clear();
			foreach (var version in Database.AllVersion4())
			{
				_godotVersionSelection.AddItem(version.GetHumanReadableVersion());
				_godotVersionSelection.SetItemMetadata(_godotVersionSelection.ItemCount - 1, version.Id);
				selected = version.Id == defEng.Id ? _godotVersionSelection.ItemCount - 1 : 0;
			}

			_godotVersionSelection.Selected = selected;
		};

		_godot3Version.EmitSignal(CheckBox.SignalName.Pressed);

		AddCancelButton("Cancel");
		// Rest of Initialization Functions
		_forwardPlus.Pressed += () =>
		{
			_renderingInfo.Text = ForwardPlusDesc;
		};
		_mobile.Pressed += () =>
		{
			_renderingInfo.Text = MobileDesc;
		};
		_compatability.Pressed += () =>
		{
			_renderingInfo.Text = CompatabilityDesc;
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