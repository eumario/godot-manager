using Godot;
using Godot.Collections;
using System.IO.Compression;
using Directory = System.IO.Directory;
using SFile = System.IO.File;
using StreamWriter = System.IO.StreamWriter;
using BinaryWriter = System.IO.BinaryWriter;

public class NewProject : Object
{
	public string ProjectName;
	public string ProjectLocation;
	public AssetProject Template;
	public string GodotVersion;
	public bool Gles3 = true;
	public bool Godot4 = false;
	public bool IsCSharp = false;
	public int GodotMajor = 0;
	public int GodotMinor = 0;
	public Array<AssetPlugin> Plugins;

	public bool CreateProject()
	{
		if (Template == null)
		{
			// Need to create the Project File ourselves.
			CreateProjectFile();
			CreateDefaultEnvironment();
			CopyIcon();
			ExtractPlugins();
		}
		else
		{
			// Project file should be provided in the Template.
			ExtractTemplate();
			ProjectConfig pf = new ProjectConfig();
			pf.Load(ProjectLocation.PlusFile("project.godot").NormalizePath());
			pf.SetValue("application", "config/name", $"\"{ProjectName}\"");

			// Need way to compile Assets before Enabling Plugins
			// if (Plugins.Count > 0)
			// 	SetupPlugins(pf);

			pf.Save(ProjectLocation.PlusFile("project.godot"));
			ExtractPlugins();
		}

		return true;
	}

	private void ExtractTemplate()
	{
		using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(Template.Location)))
		{
			foreach (ZipArchiveEntry zae in za.Entries)
			{
				int pp = zae.FullName.Find("/") + 1;
				string path = zae.FullName.Substr(pp, zae.FullName.Length);
				if (zae.FullName.EndsWith("/"))
				{
					// Is folder, we need to ensure to make the folder in the Project Location.
					Directory.CreateDirectory(ProjectLocation.PlusFile(path));
				}
				else
				{
					zae.ExtractToFile(ProjectLocation.PlusFile(path));
				}
			}
		}
	}

	private void ExtractPlugins()
	{
		if (!Directory.Exists(ProjectLocation.PlusFile("addons").NormalizePath()))
			Directory.CreateDirectory(ProjectLocation.PlusFile("addons"));

		foreach (AssetPlugin plgn in Plugins)
		{
			PluginInstaller installer = new PluginInstaller(plgn);
			installer.Install(ProjectLocation);
		}
	}

	private void CopyIcon()
	{
		var image = GD.Load<Texture>("res://Assets/Icons/default_project_icon.png");
		image.GetData().SavePng(ProjectLocation.PlusFile("icon.png").NormalizePath());
	}

	private void CreateDefaultEnvironment()
	{
		using (StreamWriter writer = new StreamWriter(ProjectLocation.PlusFile("default_env.tres").NormalizePath()))
		{
			writer.WriteLine("[gd_resource type=\"Environment\" load_steps=2 format=2]");
			writer.WriteLine("");
			writer.WriteLine("[sub_resource type=\"ProceduralSky\" id=1]");
			writer.WriteLine("");
			writer.WriteLine("[resource]");
			writer.WriteLine("background_mode = 2");
			writer.WriteLine("background_sky = SubResource(1)");
		}
	}

	private void CreateProjectFile()
	{
		ProjectConfig pf = new ProjectConfig();
		pf.SetValue("header", "config_version", "4");
		pf.SetValue("application", "config/name", $"\"{ProjectName}\"");
		pf.SetValue("application", "config/description", "\"Enter an interesting project description here!\"");
		pf.SetValue("application", "config/icon", "\"res://icon.png\"");

		if (Godot4)
		{
			var features = new Array<string>();
			features.Add($"\"{GodotMajor}.{GodotMinor}\"");
			if (IsCSharp) features.Add("\"C#\"");
			var strFeatures = "PackedStringArray(" + string.Join(", ", features) + ")";
			pf.SetValue("header", "config_version", "5");
			pf.SetValue("application", "config/features", strFeatures);
			if (Gles3)
				pf.SetValue("rendering", "renderer/rendering_method", "forward_plus");
			else
				pf.SetValue("rendering", "renderer/rendering_method", "gl_compatability");
		}
		else
		{
			if (IsCSharp)
			{
				pf.SetValue("mono", "debugger_agent/wait_timeout", "7000");
				pf.SetValue("rendering", "environment/default_environment", "\"res://default_env.tres\"");
			}
			pf.SetValue("rendering", "quality/driver/driver_name", Gles3 ? "GLES3" : "GLES2");
		}

		pf.Save(ProjectLocation.PlusFile("project.godot").NormalizePath());
	}
}