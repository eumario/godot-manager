using Godot;
using GodotManager.Library.Data.Godot;

namespace GodotManager.Tests;

public class TestMain
{
    private ProjectConfig _test3;
    private ProjectConfig _test4;

    public void RunTest()
    {
        RunTest3();
        RunTest4();
        
        GD.Print("\n");
        GD.Print("Done");
    }

    private void RunTest3()
    {
        _test3 = new ProjectConfig("./Tests/data/godot_3_project.godot");
        _test3.Load();
        GD.Print("---------------[Config 3.x (V4)]---------------");
        _test3.DebugPrint(GD.Print);

        _test3["application", "config/name"] = "Godot Manager";
        _test3["application", "config/use_custom_user_dir"] = "false";
        
        _test3.Save("./Tests/out/godot_3_project.godot");
    }

    private void RunTest4()
    {
        _test4 = new ProjectConfig("./Tests/data/godot_4_project.godot");
        _test4.Load();
        GD.Print("\n");
        GD.Print("---------------[Config 4.x (V5)]---------------");
        _test4.DebugPrint(GD.Print);
        
        _test4["application", "config/name"] = "Godot Manager";
        _test4["application", "config/use_custom_user_dir"] = "false";
        
        _test4.Save("./Tests/out/godot_4_project.godot");
    }
}