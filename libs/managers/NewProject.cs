using Godot;
using Godot.Collections;
using GitTools;
using System.IO.Compression;
using Directory = System.IO.Directory;
using SFile = System.IO.File;
using StreamWriter = System.IO.StreamWriter;
using BinaryWriter = System.IO.BinaryWriter;

public class NewProject : Object {
	public string ProjectName;
	public string ProjectLocation;
	public AssetProject Template;
	public string GodotVersion;
	public bool Gles3 = true;
	public bool Godot4 = false;
	public Array<AssetPlugin> Plugins;
	public bool InitializeGit = true;
	public ErrorWindow ErrorDisplay = null;

	public bool CreateProject() {
		if (Template == null)
		{
			// Need to create the Project File ourselves.
			CreateProjectFile();
			CreateDefaultEnvironment();
			CopyIcon();
			ExtractPlugins();			
		} else {
			// Project file should be provided in the Template.
			ExtractTemplate();
			ConfigFile pf = new ConfigFile();
			pf.Load(ProjectLocation.PlusFile("project.godot").NormalizePath());
			pf.SetValue("application", "config/name", $"\"{ProjectName}\"");

			// Need way to compile Assets before Enabling Plugins
			// if (Plugins.Count > 0)
			// 	SetupPlugins(pf);

			pf.Save(ProjectLocation.PlusFile("project.godot"));
			ExtractPlugins();
		}

		if (InitializeGit)
		{
			var git = new Runner("git", ProjectLocation);
			GD.Print(git.Run("init"));
			CreateGitFiles();

		}
		
		return true;
	}

	private void ExtractTemplate()
	{
		using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(Template.Location))) {
			foreach(ZipArchiveEntry zae in za.Entries) {
				int pp = zae.FullName.Find("/") + 1;
				string path = zae.FullName.Substr(pp, zae.FullName.Length);
				if (zae.FullName.EndsWith("/")) {
					// Is folder, we need to ensure to make the folder in the Project Location.
					Directory.CreateDirectory(ProjectLocation.PlusFile(path));
				} else {
					zae.ExtractToFile(ProjectLocation.PlusFile(path));
				}
			}
		}
	}

	private void ExtractPlugins()
	{
		foreach(AssetPlugin plgn in Plugins) {
			Array<string> files = plgn.InstallFiles;
			using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(plgn.Location))) {
				foreach(ZipArchiveEntry zae in za.Entries) {
					int pp = zae.FullName.Find("/") + 1;
					string path = zae.FullName.Substr(pp,zae.FullName.Length);
					if (files.Contains(zae.FullName)) {
						// File we need to install
						if (zae.FullName.EndsWith("/")) {
							// Is folder, we need to ensure to make the folder in the Project Location.
							Directory.CreateDirectory(ProjectLocation.PlusFile(path).NormalizePath());
						} else {
							zae.ExtractToFile(ProjectLocation.PlusFile(path).NormalizePath());
						}
					}
				}
			}
		}
	}

	private void CopyIcon()
	{
		byte[] icon_buffer;
		using (File fh = new File()) {
			var ret = fh.Open("res://Assets/Icons/default_project_icon.png",File.ModeFlags.Read);
			if (ret != Godot.Error.Ok)
				return;
			icon_buffer = fh.GetBuffer((long)fh.GetLen());
		}

		using (var stream = SFile.OpenWrite(ProjectLocation.PlusFile("icon.png").NormalizePath())) {
			using (BinaryWriter writer = new BinaryWriter(stream)) {
				writer.Write(icon_buffer);
			}
		}
	}

	private void CreateDefaultEnvironment()
	{
		using (StreamWriter writer = new StreamWriter(ProjectLocation.PlusFile("default_env.tres").NormalizePath())) {
			writer.WriteLine("[gd_resource type=\"Environment\" load_steps=2 format=2]");
			writer.WriteLine("");
			writer.WriteLine("[sub_resource type=\"ProceduralSky\" id=1]");
			writer.WriteLine("");
			writer.WriteLine("[resource]");
			writer.WriteLine("background_mode = 2");
			writer.WriteLine("background_sky = SubResource(1)");
		}
	}

	private void CreateGitFiles()
	{
		File f = new File();
		if (f.FileExists("res://default_gitignore.txt"))
		{
			f.Open("res://default_gitignore.txt", File.ModeFlags.Read);
			var fileContent = f.GetAsText();
			using (StreamWriter writer = new StreamWriter(ProjectLocation.PlusFile(".gitignore").NormalizePath())){
				writer.Write(fileContent);
			}
			f.Close();
		}
		else
		{
			if (ErrorDisplay != null)
			{
				ErrorDisplay.ShowError("Could not find default_gitingore.txt file");
			}
		}

		if (f.FileExists("res://default_gitattributes.txt"))
		{
			f.Open("res://default_gitattributes.txt", File.ModeFlags.Read);
			var fileContent = f.GetAsText();
			using (StreamWriter writer = new StreamWriter(ProjectLocation.PlusFile(".gitattributes").NormalizePath())){
				writer.Write(fileContent);
			}
			f.Close();
		}
		else
		{
			if (ErrorDisplay != null)
			{
				ErrorDisplay.ShowError("Could not find default_gitattributes.txt file");
			}
		}
	}

	private void CreateProjectFile()
	{
		ProjectConfig pf = new ProjectConfig();
		pf.SetValue("header", "config_version", "4");
		pf.SetValue("application", "config/name", $"\"{ProjectName}\"");
		pf.SetValue("application", "config/description", "\"Enter an interesting project description here!\"");
		pf.SetValue("application", "config/icon", "\"res://icon.png\"");

		if (Godot4) {
			pf.SetValue("header", "config_version", "5");
			pf.SetValue("application", "config/features", "PackedStringArray(\"4.0\", \"Vulkan Clustered\")");
		} else {
			if (CentralStore.Instance.FindVersion(GodotVersion).IsMono)
			{
				pf.SetValue("mono", "debugger_agent/wait_timeout", "7000");
				pf.SetValue("rendering", "environment/default_environment", "\"res://default_env.tres\"");
			}
		}

		pf.Save(ProjectLocation.PlusFile("project.godot").NormalizePath());
	}
}
