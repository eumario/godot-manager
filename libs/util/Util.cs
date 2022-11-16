using Godot;
using Godot.Collections;
using Path = System.IO.Path;
using DirectoryInfo = System.IO.DirectoryInfo;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;
using FileNotFoundException = System.IO.FileNotFoundException;
using FileInfo = System.IO.FileInfo;
using Dir = System.IO.Directory;
using SFile = System.IO.File;
using System.Linq;
using System.IO.Compression;

public static class Util
{

	private static Godot.Object dummy = new Godot.Object();
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
		if (path.EndsWith("/"))
			path = path.Substr(0,path.Length-1);
		
		foreach(string part in addTo) {
			path += "/" + part;
		}
		return path;
	}

	public static string Join(this string[] parts, string separator) {
		return string.Join(separator, parts);
	}

	public static string GetParentFolder(this string path) {
		return path.GetBaseDir().GetBaseDir();
	}

	public static bool IsDirEmpty(this string path) => !Dir.Exists(path) || !Dir.EnumerateFileSystemEntries(path).Any();

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

	public static SignalAwaiter WaitTimer(this Godot.Node obj, int milliseconds) {
		return obj.ToSignal(obj.GetTree().CreateTimer(milliseconds / 1000.0f), "timeout");
	}

	public static SignalAwaiter WaitTimer(this Godot.Node obj, float seconds) {
		return obj.ToSignal(obj.GetTree().CreateTimer(seconds), "timeout");
	}

	public static string EngineVersion {
		get {
			Dictionary vers = Engine.GetVersionInfo();
			return $"{vers["major"]}.{vers["minor"]}.{vers["patch"]}";
		}
	}

	public static ImageTexture LoadImage(string path) {
		var image = new Image();
		
		if (path.StartsWith("res://"))
		{
			StreamTexture tex = GD.Load<StreamTexture>(path);
			image = tex.GetData();
		} else {
			if (!SFile.Exists(path.GetOSDir().NormalizePath()))
				return null;

			if (SixLabors.ImageSharp.Image.DetectFormat(path.GetOSDir().NormalizePath()) == null)
				return null;

			Error err = image.Load(path);
			if (err != Error.Ok)
				return null;
		}
		var texture = new ImageTexture();
		texture.CreateFromImage(image);
		return texture;
	}

	public static string Which(string cmd)
	{
		Array output = new Array();
		#if GODOT_X11 || GODOT_LINUXBSD || GODOT_OSX || GODOT_MACOS
		string which = "which";
		#elif GODOT_WINDOWS || GODOT_UWP
		string which = "where";
		#endif
		int exit_code = OS.Execute(which, new string[] { cmd }, true, output);
		if (exit_code != 0)
			return null;
		else
			return (output[0] as string).StripEdges();
	}
	
	public static string FindChmod()
	{
		return Which("chmod");
	}

	public static string FindXAttr()
	{
		return Which("xattr");
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

	public static bool XAttr(string path, string flags) {
		string xattr_cmd = FindXAttr();
		if (xattr_cmd == "")
			return false;
		
		int exit_code = OS.Execute(xattr_cmd, new string[] { flags, path.GetOSDir() }, true);
		if (exit_code != 0)
			return false;
		
		return true;
	}

	public static string GetUpdateFolder() {
		string path = OS.GetExecutablePath();
		string base_path = "";
#if GODOT_WINDOWS || GODOT_UWP || GODOT_LINUXBSD || GODOT_X11
		base_path = path.GetBaseDir().NormalizePath();
#elif GODOT_MACOS || GODOT_OSX
		base_path = path.GetParentFolder().GetBaseDir().NormalizePath();
#endif
		return base_path.Join("update").NormalizePath();
	}

	public static string ReadFile(this ZipArchiveEntry zae) {
		byte[] buffer = new byte[zae.Length];
		using (var fh = zae.Open()) {
			fh.Read(buffer, 0, (int)zae.Length);
		}
		return buffer.GetStringFromUTF8();
	}

	public static byte[] ReadBuffer(this ZipArchiveEntry zae) {
		byte[] buffer = new byte[zae.Length];
		using (var fh = zae.Open()) {
			fh.Read(buffer, 0, (int)zae.Length);
		}
		return buffer;
	}

	public static void UpdateTr(this PopupMenu self, int indx, string text) {
		self.SetItemText(indx, text);
	}

	public static void UpdateTr(this OptionButton self, int indx, string text) {
		self.GetPopup().SetItemText(indx, text);
	}

	public static void UpdateTr(this MenuButton self, int indx, string text) {
		self.GetPopup().SetItemText(indx, text);
	}
	
	#if GODOT_X11 || GODOT_LINUXBSD
	private static string FindPkExec()
	{
		if (Which("pkexec") != null)
			return Which("pkexec");
		if (Which("gksu") != null)
			return Which("gksu");
		if (Which("gksudo") != null)
			return Which("gksudo");
		if (Which("kdesudo") != null)
			return Which("kdesudo");
		return Which("sudo");
	}
	public static int PkExec(string command, string shortDesc, string longDesc)
	{
		string pkexec = FindPkExec();
		if (pkexec == null)
		{
			GD.Print("Failed to find suitable Set-User command!");
			return 127;
		}

		Array<string> args = new Array<string>();
		
		if (pkexec.Contains("pkexec"))
		{
			args.Add("--disable-internal-agent");
		}

		if (pkexec.Contains("gksu") || pkexec.Contains("gksudo"))
		{
			args.Add("--preserve-env");
			args.Add("--sudo-mode");
			args.Add($"--description '{shortDesc}'");
		}

		if (pkexec.Contains("kdesudo"))
		{
			args.Add($"--comment '{longDesc}'");
		}

		if (pkexec.Contains("sudo") && !(pkexec.Contains("gksudo") || pkexec.Contains("kdesudo")))
		{
			return OS.Execute("bash", new string[] { pkexec, "-e", command }, true, null, false, true);
		}

		args.Add(command);
		return OS.Execute(pkexec, args.ToArray(), true);
	}

	public static int XdgDesktopInstall(string desktopFile)
	{
		string xdg_desktop_menu = Which("xdg-desktop-menu");
		if (xdg_desktop_menu == null)
		{
			GD.Print("Failed to find XDG Desktop Menu command, unable to install Desktop entry.");
			return -127;
		}

		return OS.Execute(xdg_desktop_menu,
			new string[] { "install", "--mode", "user", desktopFile }, true);
	}

	public static int XdgDesktopUninstall(string desktopFile)
	{
		string xdg_desktop_menu = Which("xdg-desktop-menu");
		if (xdg_desktop_menu == null)
		{
			GD.Print("Failed to find XDG Desktop Menu Command, unable to uninstall Desktop entry.");
			return -127;
		}

		return OS.Execute(xdg_desktop_menu,
			new string[] { "uninstall", "--mode", "user", desktopFile }, true);
	}

	public static int XdgDesktopUpdate()
	{
		string xdg_desktop_menu = Which("xdg-desktop-menu");
		if (xdg_desktop_menu == null)
		{
			GD.Print("Failed to find XDG Desktop Menu command, unable to install Desktop entry.");
			return -127;
		}

		return OS.Execute(xdg_desktop_menu,
			new string[] { "forceupdate", "--mode", "user" });
	}
#endif
}
