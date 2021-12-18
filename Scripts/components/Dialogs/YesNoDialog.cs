using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotSharpExtras;

public class YesNoDialog : ReferenceRect
{
    
    [NodePath("PC/CC/P/VB/MCContent/VC/Title")]
    Label Title = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/MessageText")]
    Label Message = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Confirm")]
    Button Confirm = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Cancel")]
    Button Cancel = null;

    private bool result = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();

        Confirm.Connect("pressed", this, "OnConfirmPressed");
        Cancel.Connect("pressed", this, "OnCancelPressed");
    }

    public async Task<bool> ShowDialog(string title, string message, string confirmText = "Yes", string cancelText = "No") {
        Title.Text = title;
        Message.Text = message;
        Confirm.Text = confirmText;
        Cancel.Text = cancelText;
        Visible = true;
        result = false;
        while (Visible) {
            await this.IdleFrame();
        }
        return result;
    }

    public void OnConfirmPressed() {
        result = true;
        Visible = false;
    }

    public void OnCancelPressed() {
        result = false;
        Visible = false;
    }
}
