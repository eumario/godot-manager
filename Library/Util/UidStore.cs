using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

namespace GodotManager.Library.Util;

public class UidStore
{
    public Dictionary<string, string> Uids { get; set; } = new();

    public void ScanFolder(string path)
    {
        // Scan all tscn files
        foreach (var file in Directory.EnumerateFiles(path, "*.tscn", SearchOption.AllDirectories))
        {
            var data = File.ReadAllText(file);
            var hdr = data.Split("\n")[0];
            var pattern = @"uid=""(uid://[^""]+)""";
            var match = Regex.Match(hdr, pattern);
            if (match.Success)
            {
                var uid = match.Groups[1].Value;
                Uids[uid] = file;
            }
        }
        
        // Scan all tres files
        foreach (var file in Directory.EnumerateFiles(path, "*.tres", SearchOption.AllDirectories))
        {
            var data = File.ReadAllText(file);
            var hdr = data.Split("\n")[0];
            var pattern = @"uid=""(uid://[^""]+)""";
            var match = Regex.Match(hdr, pattern);
            if (match.Success)
            {
                var uid = match.Groups[1].Value;
                Uids[uid] = file;
            }
        }
        
        // Scan all import files
        foreach (var file in Directory.EnumerateFiles(path, "*.import", SearchOption.AllDirectories))
        {
            var data = File.ReadAllText(file);
            var pattern = @"uid=""(uid://[^""]+)""";
            var match = Regex.Match(data, pattern);
            if (match.Success)
            {
                var uid = match.Groups[1].Value;
                Uids[uid] = file.Replace(".import", "");
            }
        }
        
        // Scan all uid files
        foreach (var file in Directory.EnumerateFiles(path, "*.uid", SearchOption.AllDirectories))
        {
            var uid = File.ReadAllText(file).StripEdges();
            Uids[uid] = file.Replace(".uid", "");
        }
    }
}