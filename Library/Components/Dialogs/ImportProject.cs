using System;
using System.IO;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Dialogs;

public partial class ImportProject : ConfirmationDialog
{
	[Signal]
	public delegate void ImportCompletedEventHandler();
	[NodePath] private BrowseLine _location;

	[NodePath] private OptionButton _godotVersion;

	public static ImportProject FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/ImportProject.tscn");
		return scene.Instantiate<ImportProject>();
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();
		PopulateGodotVersions();
		_location.DefaultValue = Database.Settings.ProjectPath;
		_location.UseDefault = true;
		Canceled += QueueFree;
		Confirmed += OnConfirmed_ImportProject;
	}

	private void PopulateGodotVersions()
	{
		// foreach (var version in Database.AllVersions())
		// {
		// 	_godotVersion.AddItem(version.Tag);
		// 	_godotVersion.SetItemMetadata(_godotVersion.ItemCount - 1, version.Id.ToString());
		// }
	}

	private void OnConfirmed_ImportProject()
	{
		var loc = _location.GetFolder();
		if (!loc.EndsWith("project.godot"))
			loc += loc.EndsWith("/") || loc.EndsWith("\\") ? "project.godot" : "/project.godot";
		if (loc.StartsWith("~"))
			loc = Path.GetFullPath(loc.Replace("~",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)));
		if (loc.StartsWith("/~"))
			loc = Path.GetFullPath(loc.Replace("/~",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)));
		
		loc = loc.NormalizePath();
		
		if (!File.Exists(loc))
		{
			UI.MessageBox("Invalid Project", "The directory provided does not contain a valid Godot Project file!");
			return;
		}
		
		try
		{
			var projectFile = ProjectFile.ReadFromFile(loc);
			Database.AddProject(projectFile);
		}
		catch (Exception ex)
		{
			// Display Dialog saying the file is not correct.
			if (ex is FileLoadException)
			{
				UI.MessageBox("Invalid Project",
					"The directory contains an invalid Godot Project file, please check the format, and verify it can be opened with Godot, before trying again.");
			}
			else
			{
				UI.MessageBox("Failed to Import",
					"Unable to import Project due to some error.  Please check the Godot Project file to ensure it is valid, before trying again.");
				GD.PrintErr($"Failed to load project {loc}, Exception Occurred: {ex.GetType().Name}: {ex.Message}\nTraceback: {ex.StackTrace}");
			}
		
			return;
		}
		
		EmitSignal(SignalName.ImportCompleted);
	}
}