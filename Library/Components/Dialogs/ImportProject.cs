using System;
using System.IO;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Dialogs;

public partial class ImportProject : ConfirmationDialog
{
	[Signal]
	public delegate void ImportCompletedEventHandler();
	[NodePath] private LineEdit _location;

	[NodePath] private Button _browse;

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
		_browse.Pressed += OnPressed_Browse;
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

	private void OnPressed_Browse()
	{
		var dlg = new FileDialog();
		dlg.Access = FileDialog.AccessEnum.Filesystem;
		dlg.CurrentDir = Database.Settings.ProjectPath;
		dlg.CurrentPath = Database.Settings.ProjectPath;
		dlg.FileMode = FileDialog.FileModeEnum.OpenFile;
		dlg.AddFilter(".godot","Godot Project File");
		dlg.FileSelected += (file) =>
		{
			dlg.QueueFree();
			OnFileDialog_FileSelected(file);
		};
		AddChild(dlg);
		dlg.PopupCentered(new Vector2I(600,300));
	}

	private void OnFileDialog_FileSelected(string file)
	{
		_location.Text = file.GetOsDir().GetBaseDir();
	}

	private void OnConfirmed_ImportProject()
	{
		var loc = _location.Text;
		if (!loc.EndsWith("project.godot"))
			loc += loc.EndsWith("/") || loc.EndsWith("\\") ? "project.godot" : "/project.godot";
		if (loc.StartsWith("~"))
			loc = Path.GetFullPath(loc.Replace("~",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)));
		if (loc.StartsWith("/~"))
			loc = Path.GetFullPath(loc.Replace("/~",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)));

		try
		{
			var projectFile = ProjectFile.ReadFromFile(loc);
		}
		catch (Exception)
		{
			// Display Dialog saying the file is not correct.
		}

		EmitSignal(nameof(ImportCompleted));
	}
}