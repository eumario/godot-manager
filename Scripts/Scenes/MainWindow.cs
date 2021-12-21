using Godot;
using GodotSharpExtras;
using Godot.Collections;
using System.IO;

public class MainWindow : Control
{
	[NodePath("bg/Shell/Sidebar")]
	ColorRect _sidebar = null;
	Array<PageButton> _buttons;
	[NodePath("bg/Shell/VC/TabContainer")]
	TabContainer _notebook = null;

	public static void FirstTimeRun() {
		// This is the first time running the program, we need to initialize the default settings.
		CentralStore.Instance.Settings["ProjectPath"] = OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects");
		CentralStore.Instance.Settings["DefaultEngine"] = System.Guid.Empty.ToString();
		CentralStore.Instance.Settings["EnginePath"] = "user://versions";
		CentralStore.Instance.Settings["CachePath"] = "user://cache";
		CentralStore.Instance.Settings["LastView"] = "ListView";
		CentralStore.Instance.Settings["LastCheck"] = System.DateTime.UtcNow.AddDays(-1);
		CentralStore.Instance.Settings["CheckInterval"] = System.TimeSpan.FromDays(1);
		CentralStore.Instance.Settings["ScanDirs"] = new Array<string> { OS.GetSystemDir(OS.SystemDir.Documents).Join("Projects") };
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();

		EnsureDirStructure();

		if (!CentralStore.Instance.LoadDatabase()) {
			FirstTimeRun();
			CentralStore.Instance.SaveDatabase();
		}

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
		if (System.IO.Directory.Exists(ProjectSettings.GlobalizePath("user://cache")))
			return;
		System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://cache"));
		System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://cache/Godot"));
		System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://cache/AssetLib"));
		System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("user://versions"));
	}
	
	public void OnPageButton_Clicked(PageButton pb) {
		_notebook.CurrentTab = _buttons.IndexOf(pb);
	}
}
