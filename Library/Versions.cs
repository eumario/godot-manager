using Godot;
using System;
using System.Reflection;
using Godot.Collections;
using GodotManager.Library.Utility;
using Semver;

namespace GodotManager.Library;
public static class Versions
{
    private static readonly Dictionary _godotVersion = Engine.GetVersionInfo();
    public static readonly SemVersion GodotManager = SemVersion.ParsedFrom(0,3,0,"dev");

    public static readonly SemVersion GodotVersion = SemVersion.ParsedFrom(_godotVersion["major"].AsInt32(),
        _godotVersion["minor"].AsInt32(), _godotVersion["patch"].AsInt32(), _godotVersion["status"].AsString());
    public static readonly SemVersion GodotSharp = Util.GetVersion(typeof(Godot.Variant));
    public static readonly SemVersion ImageSharp = Util.GetVersion(typeof(SixLabors.ImageSharp.Color));
    public static readonly SemVersion Octokit = Util.GetVersion(typeof(Octokit.Account));
    public static readonly SemVersion GodotSharpExtras = Util.GetVersion(typeof(Godot.Sharp.Extras.Tools));
    public static readonly SemVersion LiteDb = Util.GetVersion(typeof(LiteDB.LiteDatabase));
    public static readonly SemVersion Newtonsoft = Util.GetVersion(typeof(Newtonsoft.Json.Formatting));
    public static readonly SemVersion SharpZipLib = Util.GetVersion(typeof(ICSharpCode.SharpZipLib.Zip.FastZip));
}
