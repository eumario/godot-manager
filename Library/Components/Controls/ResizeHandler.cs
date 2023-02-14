using Godot;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Controls;

public partial class ResizeHandler : Control
{
	[Export] public bool Left = false;
	[Export] public bool Top = false;
	[Export] public bool Vertical = true;
	[Export] public Vector2I MinimumSize = Vector2I.Zero;

	private bool _following = false;
	private Vector2I _mouseOffset;
	private Vector2I _windowMouseOffset;
	private Vector2I _windowPosition;
	private Vector2I _windowSize;
	private int _distanceToEdge;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Vertical)
			if (Top)
				_distanceToEdge = (int)GlobalPosition.Y;
			else
				_distanceToEdge = (int)(GetTree().Root.Size.Y - GlobalPosition.Y);
		else
		{
			if (Left)
				_distanceToEdge = (int)GlobalPosition.X;
			else
				_distanceToEdge = (int)(GlobalPosition.X - GetTree().Root.Size.X);
		}

		GuiInput += OnGuiInput_Handler;
		GetTree().Root.SizeChanged += () =>
		{
			var sizeChanged = GetTree().Root.Size;
			Size = Size.ToVector2I().X == 5 ? new Vector2(Size.X, sizeChanged.Y) : new Vector2(sizeChanged.X, Size.Y);
			if (!Vertical && !Left) Position = new Vector2(sizeChanged.X - 5, 0);
			if (Vertical && !Top) Position = new Vector2(0, sizeChanged.Y - 5);
		};
	}

	void OnGuiInput_Handler(InputEvent inputEvent)
	{
		if (inputEvent is not InputEventMouseButton { ButtonIndex: MouseButton.Left }) return;
		_mouseOffset = GetLocalMousePosition().ToVector2I();
		_windowMouseOffset = GetTree().Root.GetMousePosition().ToVector2I();
		_windowPosition = GetTree().Root.Position;
		_windowSize = GetTree().Root.Size;
		_following = !_following;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!_following) return;
		var winSize = GetTree().Root.Size;
		var winPos = GetTree().Root.Position;
		var globalMousePos = GetGlobalMousePosition().ToVector2I();
		var globalPosition = GlobalPosition.ToVector2I();
		var controlPosition = Position.ToVector2I();

		if (Vertical)
		{
			if (Top)
			{
				var oldPos = winPos;
				winPos.Y = (DisplayServer.MouseGetPosition() - _windowMouseOffset).Y;
				var sizeDiff = oldPos.Y - winPos.Y;
				GetTree().Root.Position = winPos;
				winSize.Y = Mathf.Clamp(winSize.Y + sizeDiff, MinimumSize.Y, DisplayServer.ScreenGetSize().Y);
				GetTree().Root.Size = winSize;
			}
			else
			{
				winSize.Y = Mathf.Clamp(globalMousePos.Y + _distanceToEdge - _mouseOffset.Y, MinimumSize.Y,
					DisplayServer.ScreenGetSize().Y);
				GetTree().Root.Size = winSize;
				controlPosition.Y = winSize.Y - _distanceToEdge;
				Position = controlPosition;
			}
		}
		else
		{
			if (Left)
			{
				var oldPos = winPos;
				winPos.X = (DisplayServer.MouseGetPosition() - _windowMouseOffset).X;
				var sizeDiff = oldPos.X - winPos.X;
				GetTree().Root.Position = winPos;
				winSize.X = Mathf.Clamp(winSize.X + sizeDiff, MinimumSize.X, DisplayServer.ScreenGetSize().X);
				GetTree().Root.Size = winSize;
			}
			else
			{
				winSize.X = Mathf.Clamp(globalMousePos.X - _distanceToEdge - _mouseOffset.X, MinimumSize.X, DisplayServer.ScreenGetSize().X);
				GetTree().Root.Size = winSize;
			}
		}

		if (!Vertical && !Left)
		{
			globalPosition.X = winSize.X + _distanceToEdge;
			GlobalPosition = globalPosition;
		}
	}
}