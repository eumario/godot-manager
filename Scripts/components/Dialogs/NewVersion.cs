using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System;

public class NewVersion : ReferenceRect
{
    [Signal]
    public delegate void download_update(Github.Release release, bool use_mono);
    
    [Signal]
    public delegate void download_manager_update(Github.Release release);

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
        if (UseMono.Visible)
            EmitSignal("download_update", _release, UseMono.Pressed);
        else
            EmitSignal("download_manager_update", _release);
        Visible = false;
    }

    public void ShowDialog(Github.Release release, bool isGodotManager = false) {
        _release = release;
        if (!isGodotManager) {
            ReleaseInfo.Text = string.Format(Tr("There is a new version of Godot available.\n" + 
                "Version: {0}\nReleased:{1}\nReleased by: {2}"),
                release.Name,release.PublishedAt.ToLongDateString(),release.Author.Login);
            UseMono.Visible = true;
            UseMono.Pressed = false;
        }
        else {
            ReleaseInfo.Text = string.Format(Tr("There is a new version of Godot Manager.\n" +
                "Version: {0}\nReleased: {1}\nReleased by: {2}"),
                release.Name, release.PublishedAt.ToLongDateString(), release.Author.Login);
            UseMono.Visible = false;
        }
        Visible = true;
    }

    public void HideDialog() {
        Visible = false;
    }
}
