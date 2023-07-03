using Godot;
using System;
using Godot.Sharp.Extras;
using GodotManager.Library;

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
		if (Versions.GodotManager.SpecialVersion == "dev")
			ProjectSettings.SetSetting("application/config/custom_user_dir_name", "Godot-Manager-dev");
		this.OnReady();
		string[] args = OS.GetCmdlineArgs();
		if (args.Length > 0 && (args[0] == "--update" || args[0] == "--update-complete"))
		{
			// Handle Update Window
		}
		else
		{
			// Start Godot Manager normally
			var splash = _splashScreen.Instantiate<SplashScreen>();
			AddChild(splash);
			GetTree().CreateTimer(1.5f).Timeout += OnTimeout_Splash;
		}
	}

	void OnTimeout_Splash()
	{
		var main = _mainWindow.Instantiate<MainWindow>();
		this.QueueFreeChildren();
		AddChild(main);
	}
}
