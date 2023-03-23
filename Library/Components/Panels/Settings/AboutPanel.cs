using System;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Utility;

// namespace

namespace GodotManager.Library.Components.Panels.Settings;

public partial class AboutPanel : MarginContainer
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private Label _versionLabel;
	[NodePath] private RichTextLabel _createdBy;
	[NodePath] private Button _checkUpdates;

	[NodePath] private RichTextLabel _versionInformation;
	[NodePath] private Button _buyMeCoffee;
	[NodePath] private Button _itchIO;
	[NodePath] private Button _gitHub;
	[NodePath] private Button _discord;
	#endregion
	
	#region Private Variables
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		// Rest of Initialization Functions
		PopulateVersionInformation();
		SetupEventHandlers();
	}
	#endregion
	
	#region Event Handlers

	private void SetupEventHandlers()
	{
		_checkUpdates.Pressed += () =>
		{

		};

		_buyMeCoffee.Pressed += () => Util.LaunchWeb("https://www.buymeacoffee.com/eumario");
		_itchIO.Pressed += () => Util.LaunchWeb("https://eumario.itch.io/godot-manager");
		_gitHub.Pressed += () => Util.LaunchWeb("https://github.com/eumario/godot-manager");
		_discord.Pressed += () => Util.LaunchWeb("https://discord.gg/ESkwAMN2Tt");

		_createdBy.MetaClicked += (meta) => Util.LaunchWeb(meta.AsString());
		_versionInformation.MetaClicked += (meta) => Util.LaunchWeb(meta.AsString());
	}
	#endregion
	
	#region Private Support Functions

	private void PopulateVersionInformation()
	{
		var version = "" +
		$"[table=3][cell][color=green]Project Name[/color][/cell][cell][color=green]Version[/color]     [/cell][cell][color=green]Website[/color][/cell]" +
		$"[cell][color=aqua]Godot Engine (Mono Edition)      [/color][/cell][cell][color=white]{Versions.GodotVersion.ToString()}[/color][/cell][cell][color=yellow][url]https://godotengine.org[/url][/color][/cell]" +
		$"[cell][color=aqua]GodotSharpExtras[/color][/cell][cell][color=white]{Versions.GodotSharp.ToString()}[/color][/cell][cell][color=yellow][url]https://github.com/eumario/GodotSharpExtras[/url][/color][/cell]" +
		$"[cell][color=aqua]NewtonSoft JSON[/color][/cell][cell][color=white]{Versions.Newtonsoft.ToString()}[/color][/cell][cell][color=yellow][url]https://www.newtonsoft.com/json[/url][/color][/cell]" +
		$"[cell][color=aqua]SixLabors ImageSharp[/color][/cell][cell][color=white]{Versions.ImageSharp.ToString()}[/color][/cell][cell][color=yellow][url]https://sixlabors.com/products/imagesharp/[/url][/color][/cell]" +
		$"[cell][color=aqua]Octokit[/color][/cell][cell][color=white]{Versions.Octokit.ToString()}[/color][/cell][cell][color=yellow][url]https://github.com/octokit/octokit.net[/url][/color][/cell]" +
		$"[cell][color=aqua]LiteDB[/color][/cell][cell][color=white]{Versions.LiteDb.ToString()}[/color][/cell][cell][color=yellow][url]https://litedb.org[/url][/color][/cell]" +
		$"[cell][color=aqua]SharpZipLib[/color][/cell][cell][color=white]{Versions.SharpZipLib.ToString()}[/color][/cell][cell][color=yellow][url]https://github.com/icsharpcode/SharpZipLib[/url][/color][/cell][/table]"
		;
		_versionInformation.Text = version;
		_versionLabel.Text = $"Version: {Versions.GodotManager.ToString()}";
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}