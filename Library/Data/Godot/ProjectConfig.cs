using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GodotManager.Library.Data.Godot;

public class ProjectConfig
{
    private const string Header = @"; Engine Configuration file.
; It's best edited using the editor UI and not directly,
; since the parameters that go here are not all obvious.
;
; Format:
;   [section] ; section goes between []
;   param=value ; assign values to parameters
";

    private string _buffer;
    private Dictionary<string, Dictionary<string, string>> _sections;
    private string _fileName;

    public ProjectConfig(string fileName = "")
    {
        _fileName = fileName;
        _buffer = "";
        _sections = new();
    }

    public string this[string section, string key]
    {
        get => _sections[section][key];
        set => _sections[section][key] = value;
    }

    public string GetValue(string section, string key, string defaultValue = null)
    {
        string retrieval = defaultValue;
        if (HasSection(section) && HasSectionKey(section, key))
        {
            retrieval = this[section, key];
            if (retrieval.StartsWith("\"") && retrieval.EndsWith("\""))
                retrieval = retrieval.Substring(1, retrieval.Length - 2);
        }

        return retrieval;
    }

    public void SetValue(string section, string key, string value)
    {
        if (!HasSection(section))
            _sections[section] = new();
        this[section, key] = value;
    }

    public bool HasSection(string section) => _sections.Keys.Contains(section);
    public bool HasSectionKey(string section, string key) => HasSection(section) && _sections[section].Keys.Contains(key);

    public void DebugPrint(Action<string> print)
    {
        print("DEBUG> <<< Sections >>>");
        foreach (string section in _sections.Keys)
        {
            print($"DEBUG> [{section}]");
            foreach (string key in _sections[section].Keys)
                print($"DEBUG> >>{key}={_sections[section][key]}");
        }
    }

    public void LoadBuffer(string buffer = "")
    {
        if (buffer != "")
            _buffer = buffer;
        
        _sections = new();
        _sections["header"] = new();
        string currentSection = "header";
        string lastKey = "";

        foreach (var data in _buffer.Split("\n", true))
        {
            var line = data.StripEdges();

            if (line.StartsWith(";"))
                continue;

            if (line.StartsWith("["))
            {
                currentSection = line.Substring(1, line.Length - 2);
                _sections[currentSection] = new();
                continue;
            }

            if (line.IndexOf("=", StringComparison.Ordinal) != -1)
            {
                string[] parts = line.Split("=");
                string key = parts[0];
                lastKey = key;
                string value = parts[1];
                this[currentSection,key] = value;
                continue;
            }

            if (!string.IsNullOrEmpty(line))
                this[currentSection, lastKey] += $"\n{line}";
        }
    }

    public void Load(string fileName = "")
    {
        if (fileName != "")
            _fileName = fileName;
        
        _buffer = File.ReadAllText(_fileName);
        LoadBuffer();
    }

    public void Save(string fileName = "")
    {
        if (fileName != "")
            _fileName = fileName;

        using StreamWriter writer = new StreamWriter(_fileName);
        writer.WriteLine(Header);
        foreach (string key in _sections["header"].Keys)
        {
            writer.WriteLine($"{key}={_sections["header"][key]}");
        }

        writer.WriteLine("");

        foreach (var section in _sections.Keys)
        {
            if (section == "header") continue;
                
            writer.WriteLine($"[{section}]");
            writer.WriteLine("");
                
            foreach(string key in _sections[section].Keys)
                writer.WriteLine($"{key}={_sections[section][key]}");
                
            writer.WriteLine("");
        }
    }
    
}