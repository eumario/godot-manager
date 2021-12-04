using Godot;
using GodotSharpExtras;
using Godot.Collections;
using System;

public class MainWindow : Control
{
	[NodePath("bg/Shell/Sidebar")]
	ColorRect _sidebar = null;
	Array<PageButton> _buttons;
	[NodePath("bg/Shell/VC/TabContainer")]
	TabContainer _notebook = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();

		_buttons = new Array<PageButton>();
		foreach(var pb in GetTree().GetNodesInGroup("page_buttons")) {
			if (pb is PageButton) {
				_buttons.Add(pb as PageButton);
			}
		}
		foreach(var pb in _buttons) {
			int i = _buttons.IndexOf(pb);
			if (i == _notebook.CurrentTab)
				pb.Activate();
			else
				pb.Deactivate();
			pb.Connect("Clicked", this, "OnPageButton_Clicked");
		}
		Image appIcon = new Image();
		appIcon.Load("res://icon.png");
		OS.SetIcon(appIcon);
	}
	
	public void OnPageButton_Clicked(PageButton pb) {
		_notebook.CurrentTab = _buttons.IndexOf(pb);
	}
}
