using GodotManager.Library.Util;

namespace GodotManager.Library.Models;

public class EngineVersion
{
    public int Id { get; set; }
    public EngineRelease Release { get; set; }
    public string InstallationPath { get; set; }
    public string ExecutablePath { get; set; }
    public string Arguments { get; set; }
    public string TemplatesDir { get; set; }
    public string SettingsDir { get; set; }
}