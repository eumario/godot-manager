using Godot;
using System;
using Godot.Sharp.Extras;

public class SplashScreen : Control
{
	[NodePath] private Label VersionInfo = null;
	private Thread _thread;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();
		VersionInfo.Text = $"Version {VERSION.GodotManager}";
		var timer = GetTree().CreateTimer(0.4f);
		timer.Connect("timeout", this, "OnTimeout_LoadResources");
	}

	void OnTimeout_LoadResources()
	{
		_thread = new Thread();
		_thread.Start(this, "GDThread_Loader");
	}

	void GDThread_Loader()
	{
		var loader = ResourceLoader.LoadInteractive("res://Scenes/SceneManager.tscn");
		GdAssert.Assert(loader != null, "Loader failed to start loading SceneManager");
		if (loader is null) return;

		do
		{
			OS.DelayMsec(100);
			var err = loader.Poll();
			if (err == Error.FileEof)
			{
				var res = (PackedScene)loader.GetResource();
				CallDeferred("ThreadDone", res);
				break;
			} else if (err != Error.Ok)
			{
				GD.PrintErr("There was an error loading.");
				break;
			}
		} while (true);
	}

	void ThreadDone(PackedScene res)
	{
		_thread.WaitToFinish();

		var inst = res.Instance<SceneManager>();
		GetTree().CurrentScene.QueueFree();
		GetTree().CurrentScene = null;
		GetTree().Root.AddChild(inst);
		GetTree().CurrentScene = inst;
	}
}
