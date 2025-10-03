using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class GodotConfigParser
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections = new();
    private readonly Dictionary<string, string> _global = new();

    public IReadOnlyDictionary<string, Dictionary<string, string>> Sections => _sections;
    public IReadOnlyDictionary<string, string> Global => _global;

    public void Parse(string filePath)
    {
        using var reader = new StreamReader(filePath);
        string? line;
        string? currentSection = null;
        string? lastKey = null;
        Dictionary<string, string>? currentDict = _global;

        var sectionRegex = new Regex(@"^\[(.+)\]");
        var paramRegex = new Regex(@"^\s*([^;=\s]+)\s*=\s*(.+)\s*$");

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();

            // Skip comments and blank lines
            if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                continue;

            // Section header
            var sectionMatch = sectionRegex.Match(line);
            if (sectionMatch.Success)
            {
                currentSection = sectionMatch.Groups[1].Value;
                if (!_sections.ContainsKey(currentSection))
                    _sections[currentSection] = new Dictionary<string, string>();
                currentDict = _sections[currentSection];
                lastKey = null;
                continue;
            }

            // Parameter line
            var paramMatch = paramRegex.Match(line);
            if (paramMatch.Success)
            {
                var key = paramMatch.Groups[1].Value;
                var value = paramMatch.Groups[2].Value;
                currentDict![key] = value;
                lastKey = key;
            }
            else if (lastKey != null && currentDict != null)
            {
                // Multiline value: append to the last key's value
                // Separate by newline for clarity, but adjust as needed
                currentDict[lastKey] += "\n" + line;
            }
        }
    }

    public string? GetValue(string key, string? section = null)
    {
        if (section == null)
            return _global.TryGetValue(key, out var value) ? value : null;
        return _sections.TryGetValue(section, out var dict) && dict.TryGetValue(key, out var value2) ? value2 : null;
    }

    public void SetValue(string key, string value, string? section = null)
    {
        if (section == null)
        {
            _global[key] = value;
        }
        else
        {
            if (!_sections.ContainsKey(section))
                _sections[section] = new Dictionary<string, string>();
            _sections[section][key] = value;
        }
    }

    /// <summary>
    /// Saves the configuration back to a file, preserving basic format.
    /// </summary>
    public void Save(string filePath)
    {
        using var writer = new StreamWriter(filePath);

        // Write global (no section) parameters
        foreach (var kvp in _global)
        {
            WriteKeyValue(writer, kvp.Key, kvp.Value);
        }

        // Write sections
        foreach (var section in _sections)
        {
            writer.WriteLine();
            writer.WriteLine($"[{section.Key}]");
            foreach (var kvp in section.Value)
            {
                WriteKeyValue(writer, kvp.Key, kvp.Value);
            }
        }
    }

    private static void WriteKeyValue(StreamWriter writer, string key, string value)
    {
        if (value.Contains('\n'))
        {
            var lines = value.Split('\n');
            writer.WriteLine($"{key}={lines[0]}");
            for (int i = 1; i < lines.Length; i++)
            {
                writer.WriteLine(lines[i]);
            }
        }
        else
        {
            writer.WriteLine($"{key}={value}");
        }
    }
}