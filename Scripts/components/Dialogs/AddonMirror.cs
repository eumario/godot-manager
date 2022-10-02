using Godot;
using Godot.Sharp.Extras;

public class AddonMirror : ReferenceRect
{
    #region Signals
    [Signal]
    public delegate void asset_add_mirror(string protocol, string domainName, string pathTo);
    #endregion

    #region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/VB/Protocol")]
    OptionButton _protocol = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/DomainName")]
    LineEdit _domainName = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/PathTo")]
    LineEdit _pathTo = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/AddBtn")]
    Button _addBtn = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")]
    Button _cancelBtn = null;
    #endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
    }

    public void ShowDialog(string protocol="", string domainName="", string pathTo="", bool editing = false) {
        if (editing == false)
            GetNode<Label>("PC/CC/P/VB/MC/TitleBarBG/HB/Title").Text = Tr("Add Addon Mirror");
        else
            GetNode<Label>("PC/CC/P/VB/MC/TitleBarBG/HB/Title").Text = Tr("Edit Addon Mirror");

        for(int i = 0; i < _protocol.GetItemCount(); i++) {
            if (_protocol.GetItemText(i) == protocol) {
                _protocol.Select(i);
                break;
            }
            _protocol.Select(0);
        }

        _domainName.Text = domainName;
        _pathTo.Text = pathTo;

        Visible = true;
    }

    [SignalHandler("pressed", nameof(_addBtn))]
    void OnPressedAddBtn() {
        string protocol, domainName, pathTo;

        if (_domainName.Text == "") {
            AppDialogs.MessageDialog.ShowMessage(Tr("Add Mirror Error"),
                Tr("You need to provide a Domain name in which to connect to."));
            return;
        }
        protocol = _protocol.GetItemText(_protocol.Selected);
        domainName = _domainName.Text;
        pathTo = _pathTo.Text == "" ? "/asset-library/api/" : _pathTo.Text;
        EmitSignal("asset_add_mirror", protocol, domainName, pathTo);
        Visible = false;
    }

    [SignalHandler("pressed", nameof(_cancelBtn))]
    void OnPressedCancelBtn() {
        Visible = false;
    }
}
