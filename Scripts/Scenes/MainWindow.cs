using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using Directory = System.IO.Directory;
using SFile = System.IO.File;
using System;
using System.Linq;

public class MainWindow : Control
{
	[NodePath("bg/Shell/Sidebar")]
	ColorRect _sidebar = null;
	Array<PageButton> _buttons;
	[NodePath("bg/Shell/VC/TabContainer")] TabContainer _notebook = null;

	public MainWindow() {
		if (!CentralStore.Instance.LoadDatabase()) {
			CentralStore.Settings.SetupDefaultValues();
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
		Texture appTex = GD.Load<Texture>("res://icon.png");
		Image appIcon = (Image)appTex.GetData();
		OS.SetIcon(appIcon);
		AppDialogs dlgs = AppDialogs.Instance;
		dlgs.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
		dlgs.Name = "AppDialogs";
		AddChild(dlgs);

		var res = CleanupCarriageReturns();

		if (CentralStore.Settings.UseSystemTitlebar) {
			OS.WindowBorderless = false;
			GetTree().Root.GetNode<Titlebar>("SceneManager/MainWindow/bg/Shell/VC/TitleBar").Visible = false;
			GetTree().Root.GetNode<Control>("SceneManager/MainWindow/bg/Shell/VC/VisibleSpacer").Visible = true;
		} else {
			OS.WindowBorderless = true;
			GetTree().Root.GetNode<Titlebar>("SceneManager/MainWindow/bg/Shell/VC/TitleBar").Visible = true;
			GetTree().Root.GetNode<Control>("SceneManager/MainWindow/bg/Shell/VC/VisibleSpacer").Visible = false;
		}

		if (CentralStore.Settings.FirstTimeRun) {
			AppDialogs.FirstTimeInstall.Visible = true;
		}
	}

	private Array<int> CleanupCarriageReturns(Node node = null)
	{
		Array<int> count = new Array<int>() { 0, 0 };
		if (node == null) {
			node = GetTree().Root;
		}

		foreach(Node cnode in node.GetChildren()) {
			if (cnode.GetChildCount() > 0) {
				var res = CleanupCarriageReturns(cnode);
				count[0] += res[0];
				count[1] += res[1];
			}
			if (cnode is Label || cnode is TextEdit || cnode is LineEdit) {
				var data = (string)cnode.Get("text");
				if (data.Contains("\r")) {
					cnode.Set("text", data.Replace("\r",""));
					count[0] += 1;
				}
			}
			if (cnode is RichTextLabel) {
				var data = (string)cnode.Get("bbcode_text");
				if (data.Contains("\r")) {
					cnode.Set("bbcode_text", data.Replace("\r", ""));
					count[0] += 1;
				}
			}
			count[1] = count[1] + 1;
		}
		return count;
	}

	void EnsureDirStructure() {
		
		if (!Directory.Exists(CentralStore.Settings.CachePath))
			Directory.CreateDirectory(CentralStore.Settings.CachePath);
		if (!Directory.Exists(CentralStore.Settings.CachePath.Join("Godot")))
			Directory.CreateDirectory(CentralStore.Settings.CachePath.Join("Godot"));
		if (!Directory.Exists(CentralStore.Settings.CachePath.Join("AssetLib")))
			Directory.CreateDirectory(CentralStore.Settings.CachePath.Join("AssetLib"));
		if (!Directory.Exists(CentralStore.Settings.CachePath.Join("images")))
			Directory.CreateDirectory(CentralStore.Settings.CachePath.Join("images"));
		if (!Directory.Exists(CentralStore.Settings.CachePath.Join("images","news")))
			Directory.CreateDirectory(CentralStore.Settings.CachePath.Join("images","news"));
		if (!Directory.Exists(CentralStore.Settings.EnginePath))
			Directory.CreateDirectory(CentralStore.Settings.EnginePath);
	}
	
	void OnPageButton_Clicked(PageButton pb) {
		_notebook.CurrentTab = _buttons.IndexOf(pb);
	}

	[SignalHandler("tree_exiting")]
	void OnExitingTree() {
		CentralStore.Instance.SaveDatabase();
	}
}
