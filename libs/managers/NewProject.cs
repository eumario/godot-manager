using Godot;
using Godot.Collections;
using System.IO.Compression;
using Directory = System.IO.Directory;

public class NewProject : Object {
	public string ProjectName;
	public string ProjectLocation;
	public AssetProject Template;
	public string GodotVersion;
	public bool Gles3 = true;
	public Array<AssetPlugin> Plugins;

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
			pf.SetValue("application", "config/name", ProjectName);

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
							Directory.CreateDirectory(ProjectLocation.PlusFile(path));
						} else {
							zae.ExtractToFile(ProjectLocation.PlusFile(path));
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
			if (ret != Error.Ok)
				return;
			icon_buffer = fh.GetBuffer((long)fh.GetLen());
		}
		using (File icon = new File()) {
			var ret = icon.Open(ProjectLocation.PlusFile("icon.png"), File.ModeFlags.Write);
			if (ret != Error.Ok)
				return;
			icon.StoreBuffer(icon_buffer);
		}
	}

	private void CreateDefaultEnvironment()
	{
		ConfigFile tres = new ConfigFile();
		tres.SetValue("gd_resource type=\"Environment\" load_steps=2 format=2","","");
		tres.SetValue("sub_resource type=\"ProceduralSky\" id=1", "", "");
		tres.SetValue("resource", "background_mode", 2);
		tres.SetValue("resource", "background_sky", "SubResource(1)");
		tres.Save(ProjectLocation.PlusFile("default_env.tres"));
	}

	private void CreateProjectFile()
	{
		ConfigFile pf = new ConfigFile();
		pf.SetValue(null, "config_version", 4);
		pf.SetValue(null, "_global_script_classes", new Array());
		pf.SetValue(null, "_global_script_class_icons", new Dictionary());
		pf.SetValue("application", "config/name", ProjectName);
		pf.SetValue("application", "config/description", "Enter an interesting project description here!");
		pf.SetValue("application", "config/icon", "res://icon.png");

		// Need way to compile Assets before Enabling Plugins
		// if (Plugins.Count > 0)
		// {
		// 	SetupPlugins(pf);
		// }

		if (CentralStore.Instance.FindVersion(GodotVersion).IsMono)
		{
			pf.SetValue("mono", "debugger_agent/wait_timeout", 7000);
		}

		pf.SetValue("rendering", "environment/default_environment", "res://default_env.tres");
		pf.Save(ProjectLocation.PlusFile("project.godot"));
	}

	private void SetupPlugins(ConfigFile pf)
	{
		Array<string> plugins = null;
		if (pf.HasSectionKey("editor_plugins","enabled"))
			plugins = pf.GetValue("editor_plugins","enabled") as Array<string>;
		else	
			plugins = new Array<string>();
		foreach (AssetPlugin plgn in Plugins)
		{
			foreach (string file in plgn.InstallFiles)
			{
				if (file.EndsWith("plugin.cfg"))
				{
					var path = file;
					int pp = path.Find("/")+1;
					path = path.Substr(pp,path.Length);
					plugins.Add($"res://{path}");
				}
			}
		}
		pf.SetValue("editor_plugins", "enabled", plugins);
	}
}