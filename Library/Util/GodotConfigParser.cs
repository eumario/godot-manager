using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class GodotConfigParser
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections = new();
    private readonly Dictionary<string, string> _global = new();

    // Stores all lines (including comments and blank lines) as they appeared in the original file
    private readonly List<string> _originalLines = new();

    // Optionally, stores mapping from key to line indexes for more advanced editing
    // private readonly Dictionary<(string? Section, string Key), int> _keyLineIndexes = new();

    public IReadOnlyDictionary<string, Dictionary<string, string>> Sections => _sections;
    public IReadOnlyDictionary<string, string> Global => _global;

    public void Parse(string filePath)
    {
        _sections.Clear();
        _global.Clear();
        _originalLines.Clear();

        using var reader = new StreamReader(filePath);
        string? line;
        string? currentSection = null;
        string? lastKey = null;
        Dictionary<string, string>? currentDict = _global;

        var sectionRegex = new Regex(@"^\[(.+)\]");
        var paramRegex = new Regex(@"^\s*([^;=\s]+)\s*=\s*(.+)\s*$");

        while ((line = reader.ReadLine()) != null)
        {
            _originalLines.Add(line); // Save the raw line as-is

            string trimmedLine = line.Trim();

            // Skip blank lines for parsing, but keep all lines for saving
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                continue;

            // Section header
            var sectionMatch = sectionRegex.Match(trimmedLine);
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
            var paramMatch = paramRegex.Match(trimmedLine);
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
                currentDict[lastKey] += "\n" + trimmedLine;
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
    /// Saves the configuration back to a file, preserving comments and blank lines as in the original file.
    /// Modified key/values will be updated in their original place; new ones will be appended at the end of their section.
    /// </summary>
    public void Save(string filePath)
    {
        // 1. Build a lookup for current config so we can track what's been written
        var globalWritten = new HashSet<string>();
        var sectionsWritten = new Dictionary<string, HashSet<string>>();
        foreach (var section in _sections.Keys)
            sectionsWritten[section] = new HashSet<string>();

        string? currentSection = null;

        using var writer = new StreamWriter(filePath);
        foreach (var rawLine in _originalLines)
        {
            string trimmedLine = rawLine.Trim();

            // Section header?
            var sectionHeader = Regex.Match(trimmedLine, @"^\[(.+)\]");
            if (sectionHeader.Success)
            {
                currentSection = sectionHeader.Groups[1].Value;
                writer.WriteLine(rawLine);
                continue;
            }

            // Key-value line?
            var paramMatch = Regex.Match(trimmedLine, @"^\s*([^;=\s]+)\s*=\s*(.+)\s*$");
            if (paramMatch.Success)
            {
                var key = paramMatch.Groups[1].Value;
                string? value = null;
                if (currentSection == null)
                {
                    if (_global.TryGetValue(key, out var v) && !globalWritten.Contains(key))
                    {
                        value = v;
                        globalWritten.Add(key);
                    }
                }
                else
                {
                    if (_sections.TryGetValue(currentSection, out var dict) && dict.TryGetValue(key, out var v) && !sectionsWritten[currentSection].Contains(key))
                    {
                        value = v;
                        sectionsWritten[currentSection].Add(key);
                    }
                }
                if (value != null)
                {
                    WriteKeyValue(writer, key, value);
                    // If multiline, consume extra original lines until next section/key/comment/blank
                    var lines = value.Split('\n');
                    int extraLines = lines.Length - 1;
                    while (extraLines-- > 0)
                        writer.WriteLine(); // We will not duplicate old lines, but preserve count
                }
                else
                {
                    writer.WriteLine(rawLine); // key deleted in config, just keep original line
                }
            }
            else
            {
                writer.WriteLine(rawLine); // comment, blank, or anything else
            }
        }

        // 2. Append any new keys that were not present in the original file
        // Global
        foreach (var kvp in _global)
        {
            if (!globalWritten.Contains(kvp.Key))
                WriteKeyValue(writer, kvp.Key, kvp.Value);
        }
        // Sections
        foreach (var section in _sections)
        {
            var written = sectionsWritten[section.Key];
            foreach (var kvp in section.Value)
            {
                if (!written.Contains(kvp.Key))
                {
                    writer.WriteLine();
                    writer.WriteLine($"[{section.Key}]");
                    WriteKeyValue(writer, kvp.Key, kvp.Value);
                }
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