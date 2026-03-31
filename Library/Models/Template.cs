using System;

namespace GodotManager.Library.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ExampleShot { get; set; }
    public string ZipFile { get; set; }
    public string Version { get; set; }
    public string EngineVersion { get; set; }
    public bool IsCSharp { get; set; }
    public string Icon { get; set; }
    public string Generator { get; set; }
}