using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Godot;

namespace GodotManager.Library.Util;

public class GodotProjectFile
{
    private const string Header = """
                                  ; Engine configuration file.
                                  ; It's best edited using the editor UI and not directly,
                                  ; since the parameters that go here are not all obvious.
                                  ;
                                  ; Format:
                                  ;   [section] ; section goes between []
                                  ;   param=value ; assign values to parameters

                                  """;

    private string _buffer;
    private Dictionary<string, Dictionary<string, string>> _sections;
    private string _fileName;
    private MemoryStream _fh;
    private Dictionary<string, bool> _stringValues;

    public GodotProjectFile(string fileName = "")
    {
        _fileName = fileName;
        _buffer = "";
        _sections = new Dictionary<string, Dictionary<string, string>>();
        _stringValues = new Dictionary<string, bool>();
    }

    public IEnumerable<string> Sections => _sections.Keys;

    public IEnumerable<string> GetKeys(string section) => HasSection(section)
        ? _sections[section].Keys 
        : Enumerable.Empty<string>();

    public string this[string section, string key, bool isString = false]
    {
        get => _sections[section][key];
        set
        {
            _sections[section][key] = value;
            if (isString)
                _stringValues[$"{section}/{key}"] = isString;
        }
    }

    public string this[string path, bool isString = false]
    {
        get
        {
            var sectionAndKey = path.Split("/", 2);
            return _sections[sectionAndKey[0]][sectionAndKey[1]];
        }
        set
        {
            var sectionAndKey = path.Split("/", 2);
            _sections[sectionAndKey[0]][sectionAndKey[1]] = value;
            if (isString)
                _stringValues[path] = isString;
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
        this[sectionAndKey[0], sectionAndKey[1]] = value;
    }

    public bool HasSection(string section) => _sections.ContainsKey(section);
    public bool HasSectionKey(string section, string key) => HasSection(section) && _sections[section].ContainsKey(key);

    public void LoadBuffer(string buffer = "")
    {
        if (buffer != "")
            _buffer = buffer;

        _fh = new MemoryStream(buffer.Length);
        _fh.Write(_buffer.ToAsciiBuffer());
        _fh.Seek(0, SeekOrigin.Begin);

        _sections = new Dictionary<string, Dictionary<string, string>>
        {
            ["header"] = new()
        };
        ParseBuffer();
    }

    private void ParseBuffer()
    {
        var currentSection = "header";
        var lastKey = "";
        var inQuote = false;
        var inParen = 0;
        var inBracket = 0;
        var inBrace = 0;

        bool InDelimiter() => inQuote || inBracket > 0 || inParen > 0 || inBrace > 0;
        bool InSub() => inBracket > 0 || inParen > 0 || inBrace > 0;

        var token = new StringBuilder();
        var maybeSection = new StringBuilder();

        while (_fh.Position != _fh.Length)
        {
            var ch = (char)_fh.ReadByte();

            switch (ch)
            {
                case '\n':
                    if (InDelimiter())
                    {
                        token.Append(ch);
                    }

                    GD.Print("Are we in section? ", lastKey != "" && maybeSection.Length > 0);
                    GD.Print("We have key? ", lastKey != "");
                    GD.Print($"Last Key: {lastKey}");

                    if (lastKey == "" && maybeSection.Length > 0)
                    {
                        _sections[maybeSection.ToString()] = new Dictionary<string, string>();
                        maybeSection.Clear();
                    }

                    if (lastKey != "")
                    {
                        _sections[currentSection][lastKey] = token.ToString();
                        token.Clear();
                        lastKey = "";
                    }

                    break;
                case '{':
                    token.Append(ch);
                    inBracket++;
                    break;
                case '}':
                    token.Append(ch);
                    inBracket--;
                    break;
                case '(':
                    token.Append(ch);
                    inParen++;
                    break;
                case ')':
                    token.Append(ch);
                    inParen--;
                    break;
                case '"':
                    if (InSub())
                    {
                        token.Append(ch);
                        continue;
                    }
                    inQuote = !inQuote;
                    break;
                case ';':
                    while (true)
                    {
                        ch = (char)_fh.ReadByte();
                        if (ch != '\n') continue;
                        break;
                    }

                    break;
                case '[':
                    inBrace++;
                    break;
                case ']':
                    inBrace--;
                    break;
                default:
                    if (ch == '=')
                    {
                        lastKey = token.ToString();
                        token.Clear();
                    }
                    else
                    {
                        if (InDelimiter() && InSub())
                            maybeSection.Append(ch);
                        token.Append(ch);
                    }

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
        foreach(var key in _sections["header"].Keys)
            writer.WriteLine($"{key}={_sections["header"][key]}");

        writer.WriteLine("");

        foreach (var section in _sections.Keys.Where(s => s != "header"))
        {
            writer.WriteLine($"[{section}]");
            writer.WriteLine("");

            foreach (var key in _sections[section].Keys)
            {
                writer.Write($"{key}=");
                writer.WriteLine(
                    _stringValues.ContainsKey($"{section}/{key}")
                    ? $"\"{_sections[section][key]}\""
                    : _sections[section][key]
                );
            }
            writer.WriteLine("");
        }
    }
}