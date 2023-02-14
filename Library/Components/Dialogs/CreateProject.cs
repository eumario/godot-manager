using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Dialogs;

public partial class CreateProject : AcceptDialog
{
	#region Signals
	#endregion
	
	#region Quick Create
	public static CreateProject FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/CreateProject.tscn");
		return scene.Instantiate<CreateProject>();
	}
	#endregion
	
	#region Node Paths

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