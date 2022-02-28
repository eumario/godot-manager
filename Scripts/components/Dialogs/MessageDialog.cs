using Godot;
using Godot.Sharp.Extras;

public class MessageDialog : ReferenceRect
{
#region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/VC/Title")]
    Label _title = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/MessageText")]
    Label _message = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Confirm")]
    Button _confirm = null;
#endregion

#region Private Holder Variables
    string sTitle = "";
    string sMessage = "";
#endregion

    public string Title {
        get {
            if (_title != null)
                return _title.Text;
            return sTitle;
        }

        set {
            sTitle = value;
            if (_title != null)
                _title.Text = value;
        }
    }

    public string Message {
        get {
            if (_message != null)
                return _message.Text;
            return sMessage;
        }
        set {
            sMessage = value;
            if (_message != null)
                _message.Text = value;
        }
    }

    public override void _Ready()
    {
        this.OnReady();

        Title = sTitle;
        Message = sMessage;
    }

    public void ShowMessage(string title, string message) {
        Title = title;
        Message = message;
        Visible = true;
    }

    [SignalHandler("pressed", nameof(_confirm))]
    void OnConfirmPressed() {
        Visible = false;
    }
}
