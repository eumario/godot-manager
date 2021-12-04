using Godot;
using GodotSharpExtras;
using System;

public class FirstTimeInstall : ReferenceRect
{
    [NodePath("PC/CC/P/VB/MCButtons/HB/AddGodot")]
    Button AddGodot = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/DownloadGodot")]
    Button DownloadGodot = null;

    [NodePath("../bg/Shell/Sidebar/VC/Godot")]
    PageButton GodotButton = null;

    [NodePath("../AddCustomGodot")]
    AddCustomGodot addCustomGodot = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();

        AddGodot.Connect("pressed", this, "OnPressed_AddGodot");
        DownloadGodot.Connect("pressed", this, "OnPressed_DownloadGodot");
    }

    public void OnPressed_AddGodot() {
        Visible = false;
        addCustomGodot.Visible = true;
    }

    public void OnPressed_DownloadGodot() {
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
