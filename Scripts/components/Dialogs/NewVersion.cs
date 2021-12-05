using Godot;
using Godot.Collections;
using GodotSharpExtras;
using System;

public class NewVersion : ReferenceRect
{
    [NodePath("PC/CC/P/VB/MCContent/VC/ReleaseInfo")]
    Label ReleaseInfo = null;
    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Download")]
    Button Download = null;
    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Cancel")]
    Button Cancel = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        Cancel.Connect("pressed", this, "OnCancelClicked");
        Download.Connect("pressed", this, "OnDownloadClicked");
    }

    public void OnCancelClicked() {
        this.Visible = false;
    }

    public void OnDownloadClicked() {
        // Handle Downloading new version of Godot.
    }

    public void UpdateReleaseInfo(Github.Release release) {
        ReleaseInfo.Text = $"There is a new version of Godot available.\nVersion: {release.Name}\nReleased: {release.PublishedAt}\nReleased by: {release.Author.Login}";
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
