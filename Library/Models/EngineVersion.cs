using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Godot;
using GodotManager.Library.Util;

namespace GodotManager.Library.Models;

public class EngineVersion
{
    public int Id { get; set; }
    public EngineRelease Release { get; set; }
    public string StandardEditor { get; set; }
    public string DotnetEditor { get; set; }
    public string Arguments { get; set; }

    [NotMapped] public string StandardInstallPath => StandardEditor.GetBaseDir();
    [NotMapped] public string DotnetInstallPath => DotnetEditor.GetBaseDir();
    
    [NotMapped] public bool IsSelfContained => File.Exists(Path.Combine(StandardInstallPath, "._sc_"));
    [NotMapped] public bool IsDotnetSelfContained => File.Exists(Path.Combine(DotnetInstallPath, "._sc_"));

    [NotMapped] public string TemplatesDir => Path.Combine(EditorData, "export_templates");
    [NotMapped] public string DotnetTemplatesDir => Path.Combine(DotnetEditorData, "export_templates");

    [NotMapped]
    public string EditorData => IsSelfContained
        ? Path.Combine(StandardInstallPath, "editor_data")
        :
#if GODOT_LINUXBSD
        Path.Combine(OS.GetEnvironment("HOME"), ".local", "share", "godot")
#elif GODOT_WINDOWS
        Path.Combine(OS.GetEnvironment("APPDATA"), "Godot")
#elif GODOT_MACOS
        Path.Combine(OS.GetEnvironment("HOME"), "Library", "Application Support", "Godot")
#endif
    ;

    [NotMapped]
    public string DotnetEditorData => IsDotnetSelfContained
        ? Path.Combine(DotnetInstallPath, "editor_data")
        :
#if GODOT_LINUXBSD
        Path.Combine(OS.GetEnvironment("HOME"), ".local", "share", "godot")
#elif GODOT_WINDOWS
        Path.Combine(OS.GetEnvironment("APPDATA"), "Godot")
#elif GODOT_MACOS
        Path.Combine(OS.GetEnvironment("HOME"), "Library", "Application Support", "Godot")
#endif
    ;

    [NotMapped]
    public string EditorSettings => IsSelfContained
        ? Path.Combine(StandardInstallPath, "editor_data")
        :
#if GODOT_LINUXBSD
        Path.Combine(OS.GetEnvironment("HOME"), ".config", "godot")
#elif GODOT_WINDOWS
        Path.Combine(OS.GetEnvironment("APPDATA"), "Godot")
#elif GODOT_MACOS
        Path.Combine(OS.GetEnvironment("HOME"), "Library", "Application Support", "Godot")
#endif
    ;
    
    [NotMapped]
    public string DotnetEditorSettings => IsDotnetSelfContained
        ? Path.Combine(DotnetInstallPath, "editor_data")
        :
#if GODOT_LINUXBSD
        Path.Combine(OS.GetEnvironment("HOME"), ".config", "godot")
#elif GODOT_WINDOWS
        Path.Combine(OS.GetEnvironment("APPDATA"), "Godot")
#elif GODOT_MACOS
        Path.Combine(OS.GetEnvironment("HOME"), "Library", "Application Support", "Godot")
#endif
    ;

    [NotMapped]
    public string EditorCache => IsSelfContained
        ? Path.Combine(StandardInstallPath, "editor_data")
        :
#if GODOT_LINUXBSD
        Path.Combine(OS.GetEnvironment("HOME"), ".cache", "godot")
#elif GODOT_WINDOWS
        Path.Combine(OS.GetEnvironment("TEMP"), "Godot")
#elif GODOT_MACOS
        Path.Combine(OS.GetEnvironment("HOME"), "Library", "Caches", "Godot")
#endif
    ;
    
    [NotMapped]
    public string DotnetEditorCache => IsDotnetSelfContained
        ? Path.Combine(DotnetInstallPath, "editor_data")
        :
#if GODOT_LINUXBSD
        Path.Combine(OS.GetEnvironment("HOME"), ".cache", "godot")
#elif GODOT_WINDOWS
        Path.Combine(OS.GetEnvironment("TEMP"), "Godot")
#elif GODOT_MACOS
        Path.Combine(OS.GetEnvironment("HOME"), "Library", "Caches", "Godot")
#endif
    ;

    [NotMapped]
    public string EditorSettingsFile => Path.Combine(EditorSettings,
        Release.IsGodot3() ? "editor_settings-3.tres" : 
        Release.IsGodot4() && Release.Version.Version.Minor < 3 ? "editor_settings-4.tres" :
        $"editor_settings-{Release.Version.Version.Major}.{Release.Version.Version.Minor}.tres");

    [NotMapped]
    public string DotnetEditorSettingsFile => Path.Combine(DotnetEditorSettings,
        Release.IsGodot3() ? "editor_settings-3.tres" :
        Release.IsGodot4() && Release.Version.Version.Minor < 3 ? "editor_settings-4.tres" :
        $"editor_settings-{Release.Version.Version.Major}.{Release.Version.Version.Minor}.tres");
}