using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library;

namespace GodotManager.Scenes;

public partial class SplashScreen : Control
{
	[NodePath] private Label _versionLabel = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();
		_versionLabel.Text = $"Version: {Versions.GodotManager}";
	}
}