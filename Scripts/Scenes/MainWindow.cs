using Godot;
using GodotSharpExtras;
using Godot.Collections;
using System.IO;

public class MainWindow : Control
{
	//[NodePath("bg/Shell/Sidebar")]
	//ColorRect _sidebar = null;
	Array<PageButton> _buttons;
	[NodePath("bg/Shell/VC/TabContainer")]
	TabContainer _notebook = null;

	public MainWindow() {
		if (!CentralStore.Instance.LoadDatabase()) {
			CentralStore.Instance.SaveDatabase();
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();

		EnsureDirStructure();

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
		AppDialogs dlgs = AppDialogs.Instance;
		dlgs.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
		dlgs.Name = "AppDialogs";
		AddChild(dlgs);
	}

	void EnsureDirStructure() {
		if (!System.IO.Directory.Exists(ProjectSettings.GlobalizePath("user://cache")))
			System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://cache"));
		if (!System.IO.Directory.Exists(ProjectSettings.GlobalizePath("user://cache/Godot")))
			System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://cache/Godot"));
		if (!System.IO.Directory.Exists(ProjectSettings.GlobalizePath("user://cache/AssetLib")))
			System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://cache/AssetLib"));
		if (!System.IO.Directory.Exists(ProjectSettings.GlobalizePath("user://cache/images")))
			System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://cache/images"));
		if (!System.IO.Directory.Exists(ProjectSettings.GlobalizePath("user://versions")))
			System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://versions"));
	}
	
	void OnPageButton_Clicked(PageButton pb) {
		_notebook.CurrentTab = _buttons.IndexOf(pb);
	}
}
