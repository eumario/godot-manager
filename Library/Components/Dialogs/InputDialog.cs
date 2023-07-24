using Godot;
using System;
using NativeFileDialogSharp;

public partial class InputDialog : AcceptDialog
{
    [Signal]
    public delegate void ClosedEventHandler();

    public bool BoolResult = false;
    public string InputResult = "";
    public int IntResult = -1;

    private enum DialogStyle
    {
        BoolInput,
        StringInput,
        SelectionInput,
        SpinInput
    }

    private DialogStyle _style;

    public void SetupYesNo(string title, string message, string confirmName, string cancelName)
    {
        Title = title;
        DialogText = message;
        DialogAutowrap = true;
        OkButtonText = confirmName;
        AddCancelButton(cancelName);
        DialogHideOnOk = true;
        Confirmed += HandleConfirmed;
        Canceled += HandleCancelled;
        _style = DialogStyle.BoolInput;
    }

    public void HandleConfirmed()
    {
        switch (_style)
        {
            case DialogStyle.BoolInput:
                BoolResult = true;
                break;
            case DialogStyle.SelectionInput:
                break;
            case DialogStyle.SpinInput:
                break;
            case DialogStyle.StringInput:
                break;
        }

        EmitSignal(SignalName.Closed);
    }

    public void HandleCancelled()
    {
        EmitSignal(SignalName.Closed);
    }
}
