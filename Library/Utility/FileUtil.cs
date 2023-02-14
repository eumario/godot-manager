using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GodotManager.Library.Utility;

public static class FileUtil
{
    public static string GetResourceBase(this string path, string file) =>
        Path.Combine(path.GetBaseDir(), file.Replace("res://", ""));

    public static string GetProjectRoot(this string path, string target) =>
        target.Replace(path, "res:/");

    public static string GetOsDir(this string path) => ProjectSettings.GlobalizePath(path).NormalizePath();

    public static string GetExtension(this string path) => Path.GetExtension(path);
    public static string GetFilename(this string path) => Path.GetFileName(path);
    public static string GetBaseName(this string path) => Path.GetFileNameWithoutExtension(path);

    public static string Join(this string path, params string[] parts)
    {
        var paths = new List<string> { path };
        paths.AddRange(parts);
        return Path.Combine(paths.ToArray());
    }

    public static string NormalizePath(this string path) =>
        path.StartsWith("user://")
            ? Path.GetFullPath(ProjectSettings.GlobalizePath(path))
            : (path.StartsWith("res://") 
                ? string.Empty
                : Path.GetFullPath(path));

    public static string GetParentFolder(this string path) => path.GetBaseDir().GetBaseDir();

    public static bool IsDirEmpty(this string path) =>
        !Directory.Exists(path) || !Directory.EnumerateFileSystemEntries(path).Any();

    public static bool IsSelfContained() =>
        File.Exists(OS.GetExecutablePath().GetBaseDir().Join("._sc_"))
        || File.Exists(OS.GetExecutablePath().GetBaseDir().Join("_sc_"));

    public static string GetUserFolder(params string[] parts) =>
        IsSelfContained()
            ? OS.GetExecutablePath().GetBaseDir().Join("godot_data").Join(parts)
            : "user://".NormalizePath().Join(parts);

    public static string GetUpdateFolder()
    {
        var path = OS.GetExecutablePath();
        var basePath = "";
        #if GODOT_MACOS || GODOT_OSX
        basePath = path.GetParentFolder().GetBaseDir().NormalizePath();
        #else
        basePath = path.GetBaseDir().NormalizePath();
        #endif
        return basePath.Join("update").NormalizePath();
    }

    public static void CopyTo(this string srcFile, string destFile)
    {
        FileInfo file = new FileInfo(srcFile);
        if (!file.Exists)
            throw new FileNotFoundException($"Source file not found: {file.FullName}");

        file.CopyTo(destFile);
    }

    public static void CopyDirectory(this string sourceDir, string destinationDir, bool recursive = false)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source Directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        if (recursive)
        {
            foreach (var subDir in dirs)
            {
                var targetDirPath = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, targetDirPath, true);
            }
        }
    }
}