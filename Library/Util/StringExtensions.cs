using Godot;

namespace GodotManager.Library.Util;

public static class StringExtensions
{
    public static string GlobalizePath(this string path) => ProjectSettings.GlobalizePath(path);
}