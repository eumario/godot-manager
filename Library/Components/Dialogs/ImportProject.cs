using System;
using System.IO;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Dialogs;

[SceneNode("res://Library/Components/Dialogs/ImportProject.tscn")]
public partial class ImportProject : ConfirmationDialog
{
	[Signal]
	public delegate void ImportCompletedEventHandler();
	[NodePath] private BrowseLine _location;

	[NodePath] private OptionButton _godotVersion;
	
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

	public void SetLocation(string path)
	{
		_location.Text = path;
	}

	private void PopulateGodotVersions()
	{
		_godotVersion.Clear();
		foreach (var version in Database.AllVersions().OrderByDescending(x => x.SemVersion, SemVersionCompare.Instance))
		{
			_godotVersion.AddItem(version.GetHumanReadableVersion());
			_godotVersion.SetItemMetadata(_godotVersion.ItemCount - 1, version.Id);
		}
	}

	private async void OnConfirmed_ImportProject()
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
			var godot = Database.GetVersion(_godotVersion.GetItemMetadata(_godotVersion.Selected).AsInt32());
			if (godot is null)
			{
				UI.MessageBox("Godot Version Failure", "Failed to get the proper version of Godot Engine to use with this project.");
				return;
			}

			if (godot.SemVersion.Version.Major == 4 && !projectFile.IsGodot4)
			{
				var res = await UI.YesNoBox("Older Version Project",
					"This project file is Godot 3.x project, are you sure you want to use Godot 4 with this project?");
				if (!res)
					return;
			}

			projectFile.GodotVersion = godot;
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