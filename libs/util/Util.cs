using Godot;
using Godot.Collections;
using Path = System.IO.Path;
using DirectoryInfo = System.IO.DirectoryInfo;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;
using FileNotFoundException = System.IO.FileNotFoundException;
using FileInfo = System.IO.FileInfo;
using Dir = System.IO.Directory;
using SFile = System.IO.File;

public static class Util {
	public static string GetResourceBase(this string path, string file) {
		return Path.Combine(path.GetBaseDir(), file.Replace("res://", "")).Replace(@"\","/");
	}

	public static string GetProjectRoot(this string path, string target) {
		return target.Replace(path,"res:/").Replace(@"\", "/");
	}

	public static string GetOSDir(this string path) {
		return ProjectSettings.GlobalizePath(path);
	}

	public static string GetExtension(this string path) {
		return Path.GetExtension(path);
	}

	public static ImageTexture LoadImage(this string path, int width = 64, int height = 64, Image.Interpolation interpolate = Image.Interpolation.Cubic) {
		Image img = new Image();
		ImageTexture texture = null;
		if (img.Load(path) == Error.Ok) {
			img.Resize(width,height,interpolate);
			texture = new ImageTexture();
			texture.CreateFromImage(img);
		}
		return texture;
	}

	static string[] ByteSizes = new string[5] { "B", "KB", "MB", "GB", "TB"};


	public static string FormatSize(double bytes) {
		double len = bytes;
		int order = 0;
		while (len >= 1024 && order < ByteSizes.Length - 1) {
			order++;
			len = len / 1024;
		}
		return string.Format("{0:0.##} {1}", len, ByteSizes[order]);
	}

	public static string NormalizePath(this string path) {
		if (path.StartsWith("res://") || path.StartsWith("user://"))
			return Path.GetFullPath(ProjectSettings.GlobalizePath(path)); //path.Replace(@"\", "/");
		else
			return Path.GetFullPath(path); //path.Replace("/",@"\");
	}

	public static string Join(this string path, params string[] addTo) {
		foreach(string part in addTo) {
			path += "/" + part;
		}
		return path.NormalizePath();
	}

	public static string GetParentFolder(this string path) {
		return path.GetBaseDir().GetBaseDir();
	}

	public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = false) {
		var dir = new DirectoryInfo(sourceDir);

		if (!dir.Exists)
			throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
		
		DirectoryInfo[] dirs = dir.GetDirectories();

		Dir.CreateDirectory(destinationDir);

		foreach(FileInfo file in dir.GetFiles()) {
			string targetFilePath = Path.Combine(destinationDir, file.Name);
			file.CopyTo(targetFilePath);
		}

		if (recursive) {
			foreach (DirectoryInfo subDir in dirs) {
				string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
				CopyDirectory(subDir.FullName, newDestinationDir, true);
			}
		}
	}

	public static void CopyTo(string srcFile, string destFile) {
		FileInfo file = new FileInfo(srcFile);
		
		if (!file.Exists)
			throw new FileNotFoundException($"Source file not found: {file.FullName}");
		
		file.CopyTo(destFile);
	}

	public static SignalAwaiter IdleFrame(this Godot.Object obj) {
		return obj.ToSignal(Engine.GetMainLoop(), "idle_frame");
	}

	public static string EngineVersion {
		get {
			Dictionary vers = Engine.GetVersionInfo();
			return $"{vers["major"]}.{vers["minor"]}.{vers["patch"]}";
		}
	}

	public static ImageTexture LoadImage(string path) {
		var image = new Image();
		Error err = image.Load(path);
		if (err != Error.Ok)
			return null;
		var texture = new ImageTexture();
		texture.CreateFromImage(image);
		return texture;
	}
	
	public static string FindChmod() {
		Array output = new Array();
		int exit_code = OS.Execute("which", new string[] { "chmod" }, true, output);
		if (exit_code != 0)
			return "";
		return (output[0] as string).StripEdges();
	}

	public static bool Chmod(string path, int perms) {
		string chmod_cmd = FindChmod();
		if (chmod_cmd == "")
			return false;
		
		int exit_code = OS.Execute(chmod_cmd, new string[] { perms.ToString(), path.GetOSDir() }, true);
		if (exit_code != 0) 
			return false;
		
		return true;
	}

	public static string GetUpdateFolder() {
		string path = OS.GetExecutablePath();
		string base_path = "";
#if GODOT_WINDOWS || GODOT_UWP || GODOT_LINUXBSD || GODOT_X11
		base_path = path.GetBaseDir().NormalizePath();
#elif GODOT_MACOS || GOODT_OSX
		base_path = path.GetParentFolder().GetBaseDir().NormalizePath();
#endif
		return base_path.Join("update").NormalizePath();
	}
}