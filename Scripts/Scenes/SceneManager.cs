using Godot;
using System;

public class SceneManager : Control
{
	static Vector2 DEFAULT_RESOLUTION = new Vector2(1024,700);
	static Vector2 UPDATE_RESOLUTION = new Vector2(600,50);

	PackedScene mainWindow = GD.Load<PackedScene>("res://Scenes/MainWindow.tscn");
	PackedScene updateWindow = GD.Load<PackedScene>("res://Scenes/UpdateWindow.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		string[] args = OS.GetCmdlineArgs();
		if (args.Length > 0 && (args[0] == "--update" || args[0] == "--update-complete")) {
			OS.MinWindowSize = UPDATE_RESOLUTION;
			OS.WindowSize = UPDATE_RESOLUTION;
			OS.CenterWindow();
			UpdateWindow win = updateWindow.Instance<UpdateWindow>();
			AddChild(win);
			win.Visible = true;
			win.StartUpdate(args);
			//win.CallDeferred("StartUpdate",args);
		} else {
			OS.MinWindowSize = DEFAULT_RESOLUTION;
			OS.WindowSize = DEFAULT_RESOLUTION;
			OS.CenterWindow();
			MainWindow win = mainWindow.Instance<MainWindow>();
			AddChild(win);
			
			if (args.Length > 0) {
				AppDialogs.ImportProject.ShowDialog(args[0]);
			}
		
			win.Visible = true;
		}
	}

	public void RunMainWindow() {
		UpdateWindow win = GetNode<UpdateWindow>("./UpdateWindow");
		RemoveChild(win);
		win.QueueFree();
		OS.MinWindowSize = DEFAULT_RESOLUTION;
		OS.WindowSize = DEFAULT_RESOLUTION;
		OS.CenterWindow();
		MainWindow mWin = mainWindow.Instance<MainWindow>();
		AddChild(mWin);
		mWin.Visible = true;
	}
}
