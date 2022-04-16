using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using System.Linq;
using Directory = System.IO.Directory;
using SFile = System.IO.File;
using System.IO.Compression;
using System.Text.RegularExpressions;

public class PluginInstaller : Object
{
	AssetPlugin _plugin;
	public AssetPlugin AssetPlugin => _plugin;

	string subFolder = "";

	Array<string> _zipContents;
	Array<string> _fileList;

	Regex resMatch = new Regex(pattern: @"res:\/\/addons\/([\w\d\s-_]+)\/");

	public PluginInstaller(AssetPlugin plugin) {
		_plugin = plugin;
		_zipContents = new Array<string>();
		_fileList = new Array<string>();
	}

	private void GetSubFolder(ZipArchive za) {
		foreach(ZipArchiveEntry zae in za.Entries) {
			if (zae.Name.EndsWith(".gd")) {
				byte[] buffer = new byte[zae.Length];
				using (var fh = zae.Open()) {
					fh.Read(buffer,0,(int)zae.Length);
				}
				string data = System.Text.Encoding.UTF8.GetString(buffer, 0, (int)zae.Length);
				var res = resMatch.Matches(data);
				if (res.Count > 0) {
					subFolder = res[0].Groups[1].ToString();
					break;
				}
			}
		}
	}

	public Array<string> GetZipContents() {
		if (_zipContents.Count != 0)
			return _zipContents;
		
		Array<string> files = new Array<string>();

		using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(_plugin.Location))) {
			foreach(ZipArchiveEntry zae in za.Entries) {
				files.Add(zae.FullName);
			}
		}
		return files;
	}

	public Array<string> GetFileList() {
		if (_fileList.Count != 0)
			return _fileList;
		
		Array<string> files = new Array<string>();
		Array<string> ret = new Array<string>();

		using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(_plugin.Location))) {
			bool needAddonsFolder = true;
			foreach(ZipArchiveEntry zae in za.Entries) {
				files.Add(zae.FullName);
			}

			foreach(string file in files) {
				if (file.IndexOf("addons") >= 0) {
					needAddonsFolder = false;
					break;
				}
			}

			if (needAddonsFolder)
				GetSubFolder(za);
			else
				return files;
			
			foreach(string file in files) {
				string path = file.Substr(file.IndexOf("/")+1,file.Length);
				if (subFolder != "")
					path = "addons/".Join(subFolder,path);
				else
					path = "addons/".Join(path);
				ret.Add(path);
			}
		}
		return ret;
	}

	public async void Install(string instLocation) {
		Array<string> files = _plugin.InstallFiles;
		bool needAddonsFolder = true;
		foreach(string file in files) {
			if (file.IndexOf("addons") >= 0) {
				needAddonsFolder = false;
				break;
			}
		}

		using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(_plugin.Location))) {
			if (needAddonsFolder)
				GetSubFolder(za);
			
			foreach(ZipArchiveEntry zae in za.Entries) {
				string path = zae.FullName;
				if (path.IndexOf("/") >= 0)
					path = path.Substr(path.IndexOf("/")+1,path.Length);
				
				if (path == "")
					continue;

				if (needAddonsFolder) {
					if (subFolder != "")
						path = "addons/".Join(subFolder,path);
					else
						path = "addons/".Join(path);
					
					if (!Directory.Exists(instLocation.Join(path).GetBaseDir().NormalizePath()))
						Directory.CreateDirectory(instLocation.Join(path).GetBaseDir().NormalizePath());
				}
				if (files.Contains(zae.FullName)) {
					if (zae.FullName.EndsWith("/")) {
						if (!Directory.Exists(instLocation.Join(path).NormalizePath()))
							Directory.CreateDirectory(instLocation.Join(path).NormalizePath());
					} else {
						if (SFile.Exists(instLocation.Join(path).NormalizePath())) {
							var ynres = await AppDialogs.YesNoDialog.ShowDialog("Install Addon",
											$"The file {zae.Name} already exists at {instLocation.Join(path).NormalizePath()}, do you wish to overwrite it?");
							try {
								zae.ExtractToFile(instLocation.Join(path).NormalizePath(), ynres);
							} catch (System.Exception ex) {
								GD.PrintErr($"Failed to write to file: {zae.Name} Reason: {ex.Message}");
							}
						} else {
							try {
								zae.ExtractToFile(instLocation.Join(path).NormalizePath());
							} catch(System.Exception ex) {
								GD.PrintErr($"Failed to write file: {zae.Name} Reason: {ex.Message}");
							}
						}
					}
				}

			}
		}
	}

	public void Uninstall(string instLocation) {
		Array<string> files = _plugin.InstallFiles;
		Array<string> dirs = new Array<string>();
		bool needAddonsFolder = true;
		bool dirtyUninstall = false;
		foreach(string file in files) {
			if (file.IndexOf("addons") >= 0) {
				needAddonsFolder = false;
				break;
			}
		}

		if (needAddonsFolder) {
			using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(_plugin.Location))) {
				GetSubFolder(za);
			}
		}

		foreach(string file in files) {
			if (file.EndsWith("/")) {
				dirs.Add(file);
			} else {
				string path = file.Substr(file.IndexOf("/")+1,file.Length);
				if (needAddonsFolder)
					if (subFolder != "")
						path = "addons/".Join(subFolder,path);
					else
						path = "addons/".Join(path);

				if (SFile.Exists(instLocation.Join(path).NormalizePath())) {
					SFile.Delete(instLocation.Join(path).NormalizePath());
				}
			}
		}

		foreach(string dir in dirs.Reverse()) {
			string path = dir.Substr(dir.IndexOf("/")+1,dir.Length);
			if (needAddonsFolder)
				if (subFolder != "")
					path = "addons/".Join(subFolder,path);
				else
					path = "addons/".Join(path);
			
			if (path == "addons/")
				continue;
			
			if (Directory.Exists(instLocation.Join(path).NormalizePath())) {
				if (Directory.EnumerateFileSystemEntries(instLocation.Join(path)).Count() == 0)
					Directory.Delete(instLocation.Join(path).NormalizePath());
				else
					dirtyUninstall = true;
			}
		}

		if (dirtyUninstall)
			AppDialogs.MessageDialog.ShowMessage($"Uninstall {_plugin.Asset.Title}", "Godot Manager has uninstalled the plugin, but files may still remain, please check your Project.");
	}
}