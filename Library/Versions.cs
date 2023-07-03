using Godot;
using Godot.Collections;
using GodotManager.Library.Utility;

namespace GodotManager.Library;
public static class Versions
{
    private static readonly Dictionary GodotEngineVersion = Engine.GetVersionInfo();
    public static readonly SemanticVersion GodotManager = new SemanticVersion(0,3,0,"dev");

    public static readonly SemanticVersion GodotVersion = new SemanticVersion(GodotEngineVersion["major"].AsInt32(),
        GodotEngineVersion["minor"].AsInt32(), GodotEngineVersion["patch"].AsInt32(), GodotEngineVersion["status"].AsString());
    public static readonly SemanticVersion GodotSharp = Util.GetVersion(typeof(Godot.Variant));
    public static readonly SemanticVersion ImageSharp = Util.GetVersion(typeof(SixLabors.ImageSharp.Color));
    public static readonly SemanticVersion Octokit = Util.GetVersion(typeof(Octokit.Account));
    public static readonly SemanticVersion GodotSharpExtras = Util.GetVersion(typeof(Godot.Sharp.Extras.Tools));
    public static readonly SemanticVersion LiteDb = Util.GetVersion(typeof(LiteDB.LiteDatabase));
    public static readonly SemanticVersion Newtonsoft = Util.GetVersion(typeof(Newtonsoft.Json.Formatting));
    public static readonly SemanticVersion SharpZipLib = Util.GetVersion(typeof(ICSharpCode.SharpZipLib.Zip.FastZip));
}
