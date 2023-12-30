using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godot;
using GodotManager.Library.Utility;

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
    private MemoryStream _fh;
    private Dictionary<string, bool> _stringValues;

    public ProjectConfig(string fileName = "")
    {
        _fileName = fileName;
        _buffer = "";
        _sections = new Dictionary<string, Dictionary<string, string>>();
        _stringValues = new Dictionary<string, bool>();
    }

    public string this[string section, string key]
    {
        get => _sections[section][key];
        set => _sections[section][key] = value;
    }

    public string this[string path]
    {
        get
        {
            var sectionAndKey = path.Split('/', 2);
            return _sections[sectionAndKey[0]][sectionAndKey[1]];
        }
        set
        {
            var sectionAndKey = path.Split("/", 2);
            _sections[sectionAndKey[0]][sectionAndKey[1]] = value;
        }
    }

    public string GetValue(string path, string defaultValue = "")
    {
        var ret = defaultValue;
        var sectionAndKey = path.Split("/", 2);
        if (HasSection(sectionAndKey[0]) && HasSectionKey(sectionAndKey[0], sectionAndKey[1]))
            ret = this[sectionAndKey[0], sectionAndKey[1]];

        return _stringValues.ContainsKey(path) ? ret.Replace("\"", "") : ret;
    }

    public void SetValue(string path, string value, bool isStringValue = false)
    {
        var sectionAndKey = path.Split("/", 2);
        if (!HasSection(sectionAndKey[0]))
            _sections[sectionAndKey[0]] = new Dictionary<string, string>();
        if (isStringValue) _stringValues[path] = true;
        this[sectionAndKey[0], sectionAndKey[1]] = isStringValue ? $"\"{value}\"" : value;
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

        _fh = new MemoryStream(_buffer.Length);
        _fh.Write(_buffer.ToAsciiBuffer());
        _fh.Seek(0, SeekOrigin.Begin);
        
        _sections = new Dictionary<string, Dictionary<string, string>>
        {
            ["header"] = new Dictionary<string, string>()
        };
        
        ParseBuffer();
    }

    private void ParseBuffer()
    {
        var currentSection = "header";
        var lastKey = "";
        var inQuote = false;
        var inParen = false;

        var token = new StringBuilder();

        while (_fh.Position != _fh.Length)
        {
            var ch = _fh.ReadChar();

            switch (ch)
            {
                case '\n':
                    if (inQuote)
                    {
                        token.Append(ch);
                    }

                    if (lastKey != "")
                    {
                        _sections[currentSection][lastKey] = token.ToString();
                        token.Clear();
                        lastKey = "";
                    }
                    continue;
                case '(':
                    token.Append(ch);
                    inParen = true;
                    break;
                case ')':
                    token.Append(ch);
                    inParen = false;
                    if (lastKey != "")
                    {
                        _sections[currentSection][lastKey] = token.ToString();
                        token.Clear();
                        lastKey = "";
                    }
                    break;
                case '"':
                    token.Append(ch);
                    if (inParen) continue;
                    inQuote = !inQuote;
                    if (lastKey != "" && !inQuote)
                    {
                        _sections[currentSection][lastKey] = token.ToString();
                        _stringValues[$"{currentSection}/{lastKey}"] = true;
                        token.Clear();
                        lastKey = "";
                    }
                    break;
                case ';':
                    while (true)
                    {
                        ch = _fh.ReadChar();
                        if (ch != '\n') continue;
                        //ch = _fh.ReadChar();
                        break;
                    }

                    break;
                case '[':
                    token.Clear();
                    ch = _fh.ReadChar();
                    while (ch != ']')
                    {
                        token.Append(ch);
                        ch = _fh.ReadChar();
                    }

                    currentSection = token.ToString();
                    _sections[currentSection] = new Dictionary<string, string>();
                    token.Clear();
                    break;
                default:
                    if (ch == '=')
                    {
                        lastKey = token.ToString();
                        token.Clear();
                    }
                    else
                        token.Append(ch);
                    break;
            }
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

        using var writer = new StreamWriter(_fileName);
        writer.WriteLine(Header);
        foreach (var key in _sections["header"].Keys)
        {
            writer.WriteLine($"{key}={_sections["header"][key]}");
        }

        writer.WriteLine("");

        foreach (var section in _sections.Keys.Where(section => section != "header"))
        {
            writer.WriteLine($"[{section}]");
            writer.WriteLine("");
                
            foreach(var key in _sections[section].Keys)
                writer.WriteLine($"{key}={_sections[section][key]}");
                
            writer.WriteLine("");
        }
    }
    
}