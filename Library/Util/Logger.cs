using Godot;
using Godot.Collections;

namespace GodotManager.Library.Util;

public partial class Logger : Godot.Logger
{
    public override partial void _LogMessage(string msg, bool error);
    
    [GodotOverride]
    public void OnLogMessage(string msg, bool error)
    {
	    GD.PrintRich(error ? $"[color=red][b]{msg}[/b][/color]" : $"[color=yellow][b]{msg}[/b][/color]");
    }
    
    public override partial void _LogError(string function, string file, int line, string code, string rationale, bool editorNotify, int errorType, Array<ScriptBacktrace> scriptBacktraces);
    
    [GodotOverride]
    public void OnLogError(string function, string file, int line, string code, string rationale, bool editorNotify, int errorType, Array<ScriptBacktrace> scriptBacktraces)
    {
	    var fmt = $"[{file}:{line}:{code}:{rationale}]";
	    GD.PrintRich($"[color=brickred][b]{fmt}[/b][/color]");
	    foreach (var bt in scriptBacktraces)
		    GD.PrintRich($"[color=yellow]{bt.Format(1,2)}[/color]");
    }
}