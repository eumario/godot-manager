using System.IO;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Controls;

public partial class ProjectLineItem : Control
{
	#region Signals
	[Signal] public delegate void FavoriteClickedEventHandler(ProjectLineItem pli, bool value);
	[Signal] public delegate void ClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void RightClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DoubleClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void RightDoubleClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DragStartedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DragEndedEventHandler(ProjectLineItem pli);
	#endregion
	
	#region Quick Create
	private static readonly PackedScene Packed = GD.Load<PackedScene>("res://Library/Components/Controls/ProjectLineItem.tscn");
	public static ProjectLineItem CreateControl() => Packed.Instantiate<ProjectLineItem>();
	#endregion
	
	#region Node Paths
	[NodePath] private TextureRect Icon = null;
	[NodePath] private RichTextLabel ProjectName = null;
	[NodePath] private Label ProjectDesc = null;
	[NodePath] private Label ProjectLoc = null;
	[NodePath] private Label GodotVersionDisplay = null;
	[NodePath] private Button Heart = null;
	#endregion
	
	#region Resources
	[Resource("res://Assets/Icons/svg/missing_icon.svg")] private Texture2D _missingIcon = null;
	[Resource("res://Assets/Icons/png/default_project_icon.png")] private Texture2D _defaultProjectIcon = null;
	#endregion
	
	#region Private Variables
	private GodotVersion _godotVersion;
	private ProjectFile _projectFile;
	#endregion
	
	#region Public Properties
	public bool MissingProject { get; set; } = false;

	public GodotVersion GodotVersion
	{
		get => _godotVersion;
		set
		{
			_godotVersion = value;
			if (GodotVersionDisplay != null)
			{
				GodotVersionDisplay.Text = $"Godot {_godotVersion.Tag}";
			}
		}
	}

	public ProjectFile ProjectFile
	{
		get => _projectFile;
		set
		{
			_projectFile = value;
			MissingProject = !File.Exists(value.Location);
			ProjectName.Text = value.Name;
			ProjectDesc.Text = value.Description;
			ProjectLoc.Text = MissingProject ? "Unknown Location" : value.Location.GetBaseDir();
			Heart.ButtonPressed = value.Favorite;

			if (MissingProject)
				Icon.Texture = _missingIcon;
			else
			{
				var file = value.Location.GetResourceBase(value.Icon);
				Icon.Texture = !File.Exists(file) ? _defaultProjectIcon : Util.LoadImage(file);
			}
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		GodotVersion = _godotVersion;
		ProjectFile = _projectFile;
		Heart.Pressed += () => EmitSignal(nameof(FavoriteClicked), this);
		GuiInput += HandleGuiInput;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return GetParent().GetParent<CategoryList>()._CanDropData(atPosition, data);
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		GetParent().GetParent<CategoryList>()._DropData(atPosition, data);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		Dictionary<string, Node> data = new Dictionary<string, Node>();
		
		if (GetParent().GetParent() is not CategoryList)
			return data;

		data["source"] = this;
		data["parent"] = GetParent().GetParent();
		var preview = CreateControl();
		preview.ProjectFile = ProjectFile;
		preview.GodotVersion = GodotVersion;
		var notifier = new VisibleOnScreenNotifier2D();
		preview.AddChild(notifier);
		notifier.ScreenEntered += () => EmitSignal(nameof(DragStarted), this);
		notifier.ScreenExited += () => EmitSignal(nameof(DragEnded), this);
		SetDragPreview(preview);
		data["preview"] = preview;
		return data;
	}


	#endregion
	
	#region Godot Event Handlers

	void HandleGuiInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton inputEventMouseButton) return;

		switch (inputEventMouseButton.ButtonIndex)
		{
			case MouseButton.Left when inputEventMouseButton.DoubleClick:
				EmitSignal(nameof(DoubleClicked), this);
				break;
			case MouseButton.Left:
				SelfModulate = Colors.White;
				EmitSignal(nameof(Clicked), this);
				break;
			case MouseButton.Right when inputEventMouseButton.DoubleClick:
				EmitSignal(nameof(RightDoubleClicked), this);
				break;
			case MouseButton.Right:
				SelfModulate = Colors.White;
				EmitSignal(nameof(RightClicked), this);
				break;
			default:
				return;
		}
	}
	#endregion
}