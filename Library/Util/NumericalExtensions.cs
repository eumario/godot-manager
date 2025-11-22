using System;

namespace GodotManager.Library.Util;

public static class NumericalExtensions
{
    static readonly string[] ByteWords = ["B", "KB", "MB", "GB", "TB", "PB", "ZB"];

    static (decimal size, string bytes) CalcSize(object size)
    {
        var i = 0;
        var newSize = Convert.ToDecimal(size);
        while (newSize > 1024)
        {
            newSize /= 1024;
            i++;
        }

        return (newSize, ByteWords[i]);
    }

    public static string FormatSize(this int size, string format = "{0:0.##}{1}")
    {
        var (newSize, bytes) = CalcSize(size);
        return string.Format(format, newSize, bytes);
    }

    public static string FormatSize(this float size, string format = "{0:0.##}{1}")
    {
        var (newSize, bytes) = CalcSize(size);
        return string.Format(format, newSize, bytes);
    }

    public static string FormatSize(this double size, string format = "{0:0.##}{1}")
    {
        var (newSize, bytes) = CalcSize(size);
        return string.Format(format, newSize, bytes);
    }

    public static string FormatSize(this decimal size, string format = "{0:0.##}{1}")
    {
        var (newSize, bytes) = CalcSize(size);
        return string.Format(format, newSize, bytes);
    }

    public static string FormatSize(this long size, string format = "{0:0.##}{1}")
    {
        var (newSize, bytes) = CalcSize(size);
        return string.Format(format, newSize, bytes);
    }
}