using System.Diagnostics;
using System.IO;
using System.Reflection;
using Godot;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Managers;

public static class GodotRunner
{
    public static async void EditProject(GodotVersion version, ProjectFile project)
    {
        if (version == null)
        {
            var res = await UI.YesNoBox("Missing Godot Version",
                "The Version of Godot that was associated with this project, is no longer available, change Versions?");
            if (!res) return;
            // Handle Missing Godot Version
        }

        if (!File.Exists(version.GetExecutablePath().GetOsDir()))
        {
            UI.MessageBox("Missing Executable",
                $"Unable to find Godot Executable in {version.Location} for {version.Tag}");
            return;
        }
        
        // Handle Shared Settings

        var psi = new ProcessStartInfo()
        {
            FileName = version.GetExecutablePath().GetOsDir(),
            Arguments = $"--path \"{project.Location.GetBaseDir()}\" -e",
            WorkingDirectory = project.Location.GetBaseDir().GetOsDir().NormalizePath(),
            UseShellExecute = !Database.Settings.NoConsole,
            CreateNoWindow = Database.Settings.NoConsole
        };

        var proc = Process.Start(psi);
        if (Database.Settings.CloseManagerOnEdit)
            ((SceneTree)Engine.GetMainLoop()).Quit();
    }

    public static async void RunEngine(GodotVersion version)
    {
        if (version == null)
        {
            var res = await UI.YesNoBox("Missing Godot Version",
                "The Version of Godot that was associated with this project, is no longer available, change Versions?");
            if (!res) return;
            // Handle Missing Godot Version
        }

        if (!File.Exists(version.GetExecutablePath().GetOsDir()))
        {
            UI.MessageBox("Missing Executable",
                $"Unable to find Godot Executable in {version.Location} for {version.Tag}");
            return;
        }
        
        // Handle Shared Settings
        var psi = new ProcessStartInfo()
        {
            FileName = version.GetExecutablePath().GetOsDir(),
            WorkingDirectory = version.GetExecutablePath().GetOsDir().GetBaseDir(),
            Arguments = "",
            UseShellExecute = !Database.Settings.NoConsole,
            CreateNoWindow = Database.Settings.NoConsole
        };

        var proc = Process.Start(psi);
    }
    
    public static async void RunProject(GodotVersion version, ProjectFile project)
    {
        if (version == null)
        {
            var res = await UI.YesNoBox("Missing Godot Version",
                "The Version of Godot that was associated with this project, is no longer available, change Versions?");
            if (!res) return;
            // Handle Missing Godot Version
        }

        if (!File.Exists(version.GetExecutablePath().GetOsDir()))
        {
            UI.MessageBox("Missing Executable",
                $"Unable to find Godot Executable in {version.Location} for {version.Tag}");
            return;
        }
        
        // Handle Shared Settings

        var psi = new ProcessStartInfo()
        {
            FileName = version.GetExecutablePath().GetOsDir(),
            Arguments = $"--path \"{project.Location.GetBaseDir()}\"",
            WorkingDirectory = project.Location.GetBaseDir().GetOsDir().NormalizePath(),
            UseShellExecute = !Database.Settings.NoConsole,
            CreateNoWindow = Database.Settings.NoConsole
        };

        var proc = Process.Start(psi);
        if (Database.Settings.CloseManagerOnEdit)
            ((SceneTree)Engine.GetMainLoop()).Quit();
    }
}