using Godot;
using Godot.Sharp.Extras;
using System;

public class FirstTimeInstall : ReferenceRect
{
    [NodePath("PC/CC/P/VB/MCButtons/HB/AddGodot")]
    Button AddGodot = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/DownloadGodot")]
    Button DownloadGodot = null;

    [NodePath("/root/SceneManager/MainWindow/bg/Shell/Sidebar/VC/Godot")]
    PageButton GodotButton = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        CentralStore.Settings.CachePath = CentralStore.Settings.CachePath.GetOSDir().NormalizePath();
        CentralStore.Settings.EnginePath = CentralStore.Settings.EnginePath.GetOSDir().NormalizePath();
        CentralStore.Instance.SaveDatabase();
    }

    [SignalHandler("pressed", nameof(AddGodot))]
    void OnPressed_AddGodot() {
        Visible = false;
        AppDialogs.AddCustomGodot.Visible = true;
    }

    [SignalHandler("pressed", nameof(DownloadGodot))]
    void OnPressed_DownloadGodot() {
        GodotButton.Activate();
        GodotButton.EmitSignal("Clicked", GodotButton);
        Visible = false;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
