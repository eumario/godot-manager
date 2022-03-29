using Godot;
using Godot.Sharp.Extras;
using System;

public class ErrorWindow : WindowDialog
{
	//[NodePath("VBoxContainer/TextEdit")] // <- removed because it does not do relative paths.
	TextEdit _textEdit = null;

	public override void _Ready()
	{
        //_textEdit = GetNode<TextEdit>("VBoxContainer/TextEdit");
		_textEdit.Text = "";
        GD.Print(_textEdit,"test");
        GD.Print("WHO YOU NOT WORK FER");
	}
	public void ShowError(string text)
	{
		_textEdit.Text = text;
		PopupCentered();
	}
}
