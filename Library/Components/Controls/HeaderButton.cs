using Godot;
using Godot.Sharp.Extras;

namespace GodotManager.Library.Components.Controls;

[Tool]
public partial class HeaderButton : PanelContainer
{
	#region Signals
	[Signal] public delegate void DirectionChangedEventHandler();
	#endregion

	#region Node Paths
	[NodePath] private Label Header = null;
	[NodePath] private TextureRect DirIcon = null;
	#endregion

	#region Resources
	[Resource("res://Assets/Icons/svg/drop_down1.svg")] private Texture2D Arrow = null;
	[Resource("res://Assets/Icons/svg/minus.svg")] private Texture2D Minus = null;
	#endregion

	#region Private Variables
	private string _title;
	private SortDirection _direction;
	#endregion

	#region Exports
	[Export]
	public string Title
	{
		get => _title;
		set
		{
			_title = value;
			if (Header is not null)
				Header.Text = value;
		}
	}

	[Export(PropertyHint.Enum, "Direction to Sort")]
	public SortDirection Direction
	{
		get => _direction;
		set
		{
			_direction = value;
			if (DirIcon is not null)
			{
				switch (_direction)
				{
					case SortDirection.Down:
						DirIcon.Texture = Arrow;
						DirIcon.FlipV = false;
						break;
					case SortDirection.Up:
						DirIcon.Texture = Arrow;
						DirIcon.FlipV = true;
						break;
					case SortDirection.Indeterminate:
						DirIcon.Texture = Minus;
						DirIcon.FlipV = false;
						break;
				}
			}
		}
	}
	#endregion
	
	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		Title = _title;
		Direction = _direction;

		GuiInput += (InputEvent inputEvent) =>
		{
			if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left, DoubleClick: true })
			{
				Direction = SortDirection.Indeterminate;
				EmitSignal(nameof(DirectionChanged));
			} else if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left })
			{
				Direction = Direction == SortDirection.Down ? SortDirection.Up : SortDirection.Down;
				EmitSignal(nameof(DirectionChanged));
			}
		};
	}
	#endregion

	#region Public Functions
	public void Indeterminate() => Direction = SortDirection.Indeterminate;
	public void Down() => Direction = SortDirection.Down;
	public void Up() => Direction = SortDirection.Up;

	public bool IsIndeterminate() => Direction == SortDirection.Indeterminate;
	public bool IsDown() => Direction == SortDirection.Down;
	public bool IsUp() => Direction == SortDirection.Up;
	#endregion
}

#region Enumerations
public enum SortDirection
{
	Indeterminate,
	Up,
	Down
}
#endregion