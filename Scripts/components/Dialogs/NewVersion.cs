using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System;

public class NewVersion : ReferenceRect
{
    [Signal]
    public delegate void download_update(Github.Release release, bool use_mono);

    [NodePath("PC/CC/P/VB/MCContent/VC/ReleaseInfo")]
    Label ReleaseInfo = null;
    [NodePath("PC/CC/P/VB/MCContent/VC/UseMono")]
    CheckBox UseMono = null;
    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Download")]
    Button Download = null;
    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Cancel")]
    Button Cancel = null;

    private Github.Release _release;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
    }

    [SignalHandler("pressed", nameof(Cancel))]
    void OnCancelClicked() {
        this.Visible = false;
    }

    [SignalHandler("pressed", nameof(Download))]
    void OnDownloadClicked() {
        Visible = false;
        EmitSignal("download_update", _release, UseMono.Pressed);
    }

    public void ShowDialog(Github.Release release) {
        _release = release;
        ReleaseInfo.Text = $"There is a new version of Godot available.\nVersion: {release.Name}\nReleased: {release.PublishedAt}\nReleased by: {release.Author.Login}";
        Visible = true;
    }

    public void HideDialog() {
        Visible = false;
    }
}
