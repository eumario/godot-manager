using Godot;
using System;
using Godot.Sharp.Extras;

namespace GodotManager.Scenes;

public partial class SceneManager : Control
{
	[Resource("res://Scenes/SplashScreen.tscn")]
	private PackedScene _splashScreen;

	[Resource("res://Scenes/MainWindow.tscn")]
	private PackedScene _mainWindow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Starting C# Code");
		this.OnReady();
		GD.Print("Initialized OnReady");
		string[] args = OS.GetCmdlineArgs();
		if (args.Length > 0 && (args[0] == "--update" || args[0] == "--update-complete"))
		{
			// Handle Update Window
		}
		else
		{
			GD.Print("Showing Splash Screen.");
			// Start Godot Manager normally
			var splash = _splashScreen.Instantiate<SplashScreen>();
			AddChild(splash);
			GetTree().CreateTimer(1.5f).Timeout += OnTimeout_Splash;
		}
	}

	void OnTimeout_Splash()
	{
		GD.Print("Showing Main Window...");
		var main = _mainWindow.Instantiate<MainWindow>();
		this.QueueFreeChildren();
		AddChild(main);
	}
}
