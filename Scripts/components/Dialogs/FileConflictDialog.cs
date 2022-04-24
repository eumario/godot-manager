using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public class FileConflictDialog : ReferenceRect
{
    #region Enumerations
    public enum ConflictAction {
        Confirm,
        ConfirmAll,
        Cancel,
        Abort
    }
    #endregion

    #region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/VC/GridContainer/ArchiveFile")]
    Label _ArchiveFile = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/GridContainer/DestinationFile")]
    Label _DestinationFile = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Confirm")]
    Button _Confirm = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/ConfirmAll")]
    Button _ConfirmAll = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Cancel")]
    Button _Cancel = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Abort")]
    Button _Abort = null;
    #endregion

    #region Private Variables
    ConflictAction _result;
    #endregion

    #region Public Properties
    public string ArchiveFile {
        set => _ArchiveFile.Text = value;
    }

    public string DestinationFile {
        set => _DestinationFile.Text = value;
    }
    #endregion

    public override void _Ready()
    {
        this.OnReady();
    }

    public async Task<ConflictAction> ShowDialog() {
        _result = ConflictAction.Abort;
        Visible = true;

        while(Visible)
            await this.IdleFrame();
        
        return _result;
    }

    public void ResultHide(ConflictAction action) {
        _result = action;
        Visible = false;
    }

    [SignalHandler("pressed", nameof(_Confirm))] void Confirm() => ResultHide(ConflictAction.Confirm);
    [SignalHandler("pressed", nameof(_ConfirmAll))] void ConfirmAll() => ResultHide(ConflictAction.ConfirmAll);
    [SignalHandler("pressed", nameof(_Cancel))] void Cancel() => ResultHide(ConflictAction.Cancel);
    [SignalHandler("pressed", nameof(_Abort))] void Abort() => ResultHide(ConflictAction.Abort);

}
