using Godot;
using System.Linq;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Data.UI;
using GodotManager.Library.Utility;

public partial class EditProjectDialog : Window
{
	#region Signals
	[Signal] public delegate void SaveProjectEventHandler(ProjectNodeCache cache);
	#endregion
	
	#region NodePaths
	// General Tab
	[NodePath] private TextureRect _projectIcon;
	[NodePath] private LineEdit _projectName;
	[NodePath] private OptionButton _engineVersion;
	[NodePath] private OptionButton _renderer;
	[NodePath] private TextEdit _projectDescription;
	
	// Addons Tab
	[NodePath] private VBoxContainer _pluginList;
	
	// Control Buttons
	[NodePath] private Button _saveProject;
	[NodePath] private Button _cancelDialog;
	#endregion
	
	#region Private Variables
	private ProjectNodeCache _projectCache;
	#endregion
	
	#region Public Properties
	public ProjectNodeCache ProjectCache
	{
		get => _projectCache;
		set
		{
			_projectCache = value;
			UpdateInfo();
		}
	}

	public ProjectFile ProjectFile => _projectCache.ProjectFile;
	#endregion
	
	#region FromScene()
	public static EditProjectDialog FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/EditProjectDialog.tscn");
		return scene.Instantiate<EditProjectDialog>();
	}
	#endregion
	
	#region Godot Overrides
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();
		UpdateInfo();
		_saveProject.Pressed += () =>
		{
			EmitSignal(SignalName.SaveProject, _projectCache);
			QueueFree();
		};
		_cancelDialog.Pressed += QueueFree;
		CloseRequested += QueueFree;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	#endregion
	
	#region Public Functions
	#endregion
	
	#region Private Functions

	private void UpdateInfo()
	{
		if (_projectIcon == null) return;
		ProjectFile.UpdateData();
		Database.UpdateProject(ProjectFile);
		var iconPath = FileUtil.GetResourceBase(ProjectFile.Location, ProjectFile.Icon).GetOsDir().NormalizePath();
		GD.Print($"ProjectFile Icon: {ProjectFile.Icon}");
		GD.Print($"Resource File: {iconPath}");
		_projectIcon.Texture = Util.LoadImage(iconPath);
		_projectName.Text = ProjectFile.Name;
		var versions = ProjectFile.IsGodot4
			? Database.AllVersions().Where(x => x.SemVersion.Version.Major == 4)
			: Database.AllVersions().Where(x => x.SemVersion.Version.Major == 3);
		_engineVersion.Clear();
		foreach (var version in versions)
		{
			_engineVersion.AddItem(version.GetHumanReadableVersion(), version.Id);
		}

		_engineVersion.Selected = _engineVersion.GetItemIndex(ProjectFile.GodotVersion.Id);

		_renderer.Clear();
		if (ProjectFile.IsGodot4)
		{
			_renderer.AddItem("Forward+");			// forward_plus
			_renderer.AddItem("Mobile");			// mobile
			_renderer.AddItem("Compatibility");	// gl_compatibility
		}
		else
		{
			_renderer.AddItem("OpenGL ES 3");		// gles3
			_renderer.AddItem("OpenGL ES 2");		// gles2
		}

		switch (ProjectFile.Renderer)
		{
			case "forward_plus":
			case "gles3":
				_renderer.Selected = 0;
				break;
			case "mobile":
			case "gles2":
				_renderer.Selected = 1;
				break;
			case "gl_compatibility":
				_renderer.Selected = 2;
				break;
			default:
				_renderer.Selected = 0;
				break;
		}

		_projectDescription.Text = ProjectFile.Description;
	}
	#endregion
}
