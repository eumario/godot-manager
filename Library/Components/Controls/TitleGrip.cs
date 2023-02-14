using Godot;
using GodotManager.Library.Utility;
using GodotManager.Scenes;

namespace GodotManager.Library.Components.Controls;

public partial class TitleGrip : Control
{
	private bool _following = false;
	private Vector2I _mouseOffset;
	public override void _Ready()
	{
		GuiInput += OnGuiInput_TitleGrip;
		GetTree().Root.SizeChanged += () =>
		{
			var sizeChanged = GetTree().Root.Size;
			Size = new Vector2(sizeChanged.X - 237, Size.Y);
		};
	}

	void OnGuiInput_TitleGrip(InputEvent inputEvent)
	{
		if (inputEvent is not InputEventMouseButton { ButtonIndex: MouseButton.Left }) return;
		_following = !_following;
		MouseDefaultCursorShape = _following ? CursorShape.Move : CursorShape.Arrow;
		_mouseOffset = GetTree().Root.GetMousePosition().ToVector2I();
	}
	
	public override void _Process(double delta)
	{
		if (!_following) return;
		
		GetTree().Root.Position = DisplayServer.MouseGetPosition() - _mouseOffset;
	}
}