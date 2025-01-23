using System.IO;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Enumerations;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Controls;

[SceneNode("res://Library/Components/Controls/ProjectLineItem.tscn")]
public partial class ProjectLineItem : Control, IProjectIcon
{
	#region Signals
	[Signal] public delegate void FavoriteClickedEventHandler(ProjectLineItem pli, bool value);
	[Signal] public delegate void ClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void RightClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DoubleClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void RightDoubleClickedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DragStartedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void DragEndedEventHandler(ProjectLineItem pli);
	[Signal] public delegate void ContextMenuClickEventHandler(ProjectLineItem pli, ContextMenuItem id);
	#endregion
	
	#region Node Paths
	[NodePath] private ColorRect _hover;
	[NodePath] private TextureRect _projectIcon;
	[NodePath] private RichTextLabel _projectName;
	[NodePath] private Label _projectDesc;
	[NodePath] private Label _projectLoc;
	[NodePath] private Label _godotVersionDisplay;
	[NodePath] private Button _heart;
	[NodePath] private PopupMenu _contextMenu;
	#endregion
	
	#region Resources
	[Resource("res://Assets/Icons/svg/missing_icon.svg")] private Texture2D _missingIcon = null;
	[Resource("res://Assets/Icons/png/default_project_icon.png")] private Texture2D _defaultProjectIcon = null;
	#endregion
	
	#region Private Variables
	private GodotVersion _godotVersion;
	private ProjectFile _projectFile;
	private ShaderMaterial _shader;
	private bool _selected = false;
	#endregion
	
	#region Public Properties
	public bool MissingProject { get; set; } = false;

	public bool IsPreview { get; set; } = false;

	public bool Selected
	{
		get => _selected;
		set
		{
			_selected = value;
			if (_hover != null)
				_hover.Visible = _selected;
		}
	}

	public GodotVersion GodotVersion
	{
		get => _godotVersion;
		set
		{
			_godotVersion = value;
			if (_godotVersionDisplay == null) return;
			_godotVersionDisplay.Text = value is null ? "Unknown" : $"{_godotVersion.GetHumanReadableVersion()}";
		}
	}

	public ProjectFile ProjectFile
	{
		get => _projectFile;
		set
		{
			if (_projectFile != null)
				_projectFile.ProjectChanged -= UpdateUI;
			
			_projectFile = value;

			GodotVersion = _projectFile.GodotVersion;

			if (_projectName is null) return;

			value.ProjectChanged += UpdateUI;
			
			UpdateUI();
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		GodotVersion = _godotVersion;
		ProjectFile = _projectFile;
		_shader = (ShaderMaterial)_heart.Material.Duplicate();
		_heart.Material = _shader;
		_shader.SetShaderParameter("s", _projectFile.Favorite ? 1.0f : 0.0f);
		_shader.SetShaderParameter("v", _projectFile.Favorite ? 1.0f : 0.5f);
		_heart.Toggled += (toggle) =>
		{
			_shader.SetShaderParameter("s", toggle ? 1.0f : 0.0f);
			_shader.SetShaderParameter("v", toggle ? 1.0f : 0.5f);
			EmitSignal(SignalName.FavoriteClicked, this, toggle);
		};
		MouseEntered += () =>
		{
			if (Selected) return;
			_hover.Visible = true;
		};
		MouseExited += () =>
		{
			if (Selected) return;
			_hover.Visible = false;
		};
		GuiInput += HandleGuiInput;
		_contextMenu.IdPressed += id =>
		{
			EmitSignal(SignalName.ContextMenuClick, this, id);
		};
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (GetParent().Name != "List" && GetParent().Name == "ListView") return false;
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
		data["parent"] = GetParent().GetParent<CategoryList>();
		var preview = SceneNode<ProjectLineItem>.FromScene();
		preview.ProjectFile = ProjectFile;
		preview.GodotVersion = GodotVersion;
		var notifier = new VisibleOnScreenNotifier2D();
		preview.AddChild(notifier);
		notifier.ScreenEntered += () => EmitSignal(SignalName.DragStarted, this);
		notifier.ScreenExited += () => EmitSignal(SignalName.DragEnded, this);
		SetDragPreview(preview);
		preview.IsPreview = true;
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
				EmitSignal(SignalName.DoubleClicked, this);
				break;
			case MouseButton.Left:
				Selected = true;
				EmitSignal(SignalName.Clicked, this);
				break;
			case MouseButton.Right when inputEventMouseButton.DoubleClick:
				EmitSignal(SignalName.RightDoubleClicked, this);
				break;
			case MouseButton.Right:
				EmitSignal(SignalName.RightClicked, this);
				break;
			default:
				return;
		}
	}
	#endregion
	
	#region Public Functions

	public void ShowContextMenu() => _contextMenu.Popup(new Rect2I((GetScreenPosition() + GetLocalMousePosition()).ToVector2I(), Vector2I.Zero));
	#endregion
	
	#region Private Functions

	private async void UpdateUI()
	{
		if (IsPreview) return;
		MissingProject = !File.Exists(ProjectFile.Location);
		_projectName.Text = ProjectFile.Name;
		_projectDesc.Text = ProjectFile.Description;
		_projectLoc.Text = MissingProject ? "Unknown Location" : ProjectFile.Location.GetBaseDir();
		_heart.ButtonPressed = ProjectFile.Favorite;

		if (MissingProject)
			_projectIcon.Texture = _missingIcon;
		else
		{
			var file = ProjectFile.Location.GetResourceBase(ProjectFile.Icon);
			_projectIcon.Texture = !File.Exists(file) ? _defaultProjectIcon : await Util.LoadImage(file);
		}
	}
	#endregion
}