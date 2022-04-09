using Godot;
using Godot.Sharp.Extras;
using System;
using System.Collections.Generic;

public class ErrorWindow : WindowDialog
{
	//[NodePath("VBoxContainer/TextEdit")] // <- removed because it does not do relative paths.
	TextEdit _textEdit = null;
	List<String> _messageQueue = new List<String>();

	public override void _Ready()
	{
		_textEdit = GetNode<TextEdit>("VBoxContainer/TextEdit");
		_textEdit.Text = "";
		Connect("popup_hide", this, "Closed");
	}
	
	public void ShowError(string text)
	{
		if (this.Visible)
		{
			_messageQueue.Add(text);
		}
		else
		{
			_textEdit.Text = text;
			PopupCentered();
		}
	}

	public void Closed()
	{
		if (_messageQueue.Count>0)
		{
			ShowError(_messageQueue[0]);
			_messageQueue.RemoveAt(0);
		}
	}
}
