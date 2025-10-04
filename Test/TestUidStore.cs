using Godot;
using System;
using System.IO;
using GodotManager.Library.FileIO;
using GodotManager.Library.Util;

namespace GodotManager.Test;

[Tool, GlobalClass]
public partial class TestUidStore : EditorScript
{
    [GodotOverride]
    public void OnRun()
    {
        var uidStore = new UidStore();
        uidStore.ScanFolder(Directory.GetCurrentDirectory());
        foreach (var kv in uidStore.Uids)
        {
            GD.Print($"{kv.Key}: {kv.Value}");
        }
    }

    public override partial void _Run();
}
