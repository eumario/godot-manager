using System.Threading.Tasks;
using Godot;
using Godot.Sharp.Extras;

public class YesNoCancelDialog : ReferenceRect
{
    
    [NodePath("PC/CC/P/VB/MCContent/VC/Title")]
    Label Title = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/MessageText")]
    Label Message = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/FirstAction")]
    Button FirstAction = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/SecondAction")]
    Button SecondAction = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/CancelAction")]
    Button CancelAction = null;

    public enum ActionResult {
        FirstAction,
        SecondAction,
        CancelAction
    }

    private ActionResult result = ActionResult.CancelAction;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
    }

    public async Task<ActionResult> ShowDialog(string title, string message, string firstActionText = "Yes", string secondActionText = "No", string cancelActionText = "Cancel") {
        Title.Text = title;
        Message.Text = message;
        FirstAction.Text = firstActionText;
        SecondAction.Text = secondActionText;
        CancelAction.Text = cancelActionText;
        Visible = true;
        result = ActionResult.CancelAction;
        while (Visible) {
            await this.IdleFrame();
        }
        return result;
    }

    [SignalHandler("pressed", nameof(FirstAction))]
    void OnFirstActionPressed() {
        result = ActionResult.FirstAction;
        Visible = false;
    }

    [SignalHandler("pressed", nameof(SecondAction))]
    void OnSecondActionPressed() {
        result = ActionResult.SecondAction;
        Visible = false;
    }

    [SignalHandler("pressed", nameof(CancelAction))]
    void OnCancelActionPressed() {
        result = ActionResult.CancelAction;
        Visible = false;
    }
}
