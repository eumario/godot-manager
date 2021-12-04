using Godot;
using GodotSharpExtras;
using System;

public class BusyDialog : ReferenceRect
{
    [NodePath("PC/CC/P/VB/MCContent/VBoxContainer/Header")]
    public Label Header = null;

    [NodePath("PC/CC/P/VB/MCContent/VBoxContainer/Byline")]
    public Label Byline = null;

    [NodePath("PC/CC/P/VB/MCContent/Spinner")]
    public AnimatedSprite Spinner = null;
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
    }

    public void UpdateHeader(string text) {
        Header.Text = text;
    }

    public void UpdateByline(string text) {
        Byline.Text = text;
    }

    public void ShowDialog() {
        Visible = true;
        Spinner.Frame = 0;
        Spinner.Play();
    }

    public void HideDialog() {
        Visible = false;
        Spinner.Frame = 0;
        Spinner.Stop();
    }
}
