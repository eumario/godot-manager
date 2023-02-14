using System.Linq;
using Godot;
using Godot.Collections;

namespace GodotManager.Library.Utility;

public static class NixUtil
{
    public static string Which(string cmd)
    {
        var output = new Array();
        var exitCode = OS.Execute("which", new string[] { cmd }, output, false, false);
        return exitCode != 0 ? null : output[0].ToString().StripEdges();
    }

    public static string FindChmod() => Which("chmod");
    public static string FindXAttr() => Which("xattr");

    public static string FindPkExec() =>
        Which("pkexec") ?? Which("gksu") ?? Which("gksudo") ?? Which("kdesudo") ?? Which("sudo");

    public static string FindXdgDesktopMenu() => Which("xdg-desktop-menu");

    public static bool Chmod(this string path, int perms)
    {
        var chmodCmd = FindChmod();
        if (chmodCmd is null) return false;
        var exitCode = OS.Execute(chmodCmd, new string[] { perms.ToString(), path.GetOsDir() });
        return exitCode == 0;
    }

    public static bool Chmod(this string path, string perms)
    {
        var chmodCmd = FindChmod();
        if (chmodCmd is null) return false;
        var exitCode = OS.Execute(chmodCmd, new string[] { perms, path.GetOsDir() });
        return exitCode == 0;
    }

    public static bool XAttr(this string path, string flags)
    {
        var xAttrCmd = FindXAttr();
        if (xAttrCmd is null) return false;
        var exitCode = OS.Execute(xAttrCmd, new string[] { flags, path.GetOsDir() });
        return exitCode == 0;
    }

    public static int PkExec(string command, string shortDesc, string longDesc)
    {
        var pkExec = FindPkExec();
        if (pkExec is null) return -127;

        var args = new Array<string>();
        
        if (pkExec.Contains("pkexec"))
            args.Add("--disable-internal-agent");

        if (pkExec.Contains("gksu") || pkExec.Contains("gksudo"))
        {
            args.Add("--preserve-env");
            args.Add("--sudo-mode");
            args.Add($"--description '{shortDesc}'");
        }

        if (pkExec.Contains("kdesudo"))
            args.Add($"--comment '{longDesc}'");

        if (pkExec.Contains("sudo") && !(pkExec.Contains("gksudo") || pkExec.Contains("kdesudo")))
            return OS.Execute("bash", new string[] { pkExec, "-e", command }, null, false, true);
        
        args.Add(command);
        return OS.Execute(pkExec, args.ToArray());
    }

    public static int XdgDesktopInstall(this string desktopFile)
    {
        var xdgDesktopMenu = FindXdgDesktopMenu();
        if (xdgDesktopMenu is null) return -127;
        return OS.Execute(xdgDesktopMenu, new string[] { "install", "--mode", "user", desktopFile });
    }

    public static int XdgDesktopUninstall(this string desktopFile)
    {
        var xdgDesktopMenu = FindXdgDesktopMenu();
        if (xdgDesktopMenu is null) return -127;
        return OS.Execute(xdgDesktopMenu, new string[] { "uninstall", "--mode", "user", desktopFile });
    }

    public static int XdgDesktopUpdate()
    {
        var xdgDesktopMenu = FindXdgDesktopMenu();
        if (xdgDesktopMenu is null) return -127;
        return OS.Execute(xdgDesktopMenu, new string[] { "forceupdate", "--mode", "user" });
    }
}