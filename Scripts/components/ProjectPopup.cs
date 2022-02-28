using Godot;
using Godot.Sharp.Extras;

public class ProjectPopup : PopupMenu
{
    public ProjectLineEntry ProjectLineEntry = null;
    public ProjectIconEntry ProjectIconEntry = null;

    public override void _Ready()
    {
        this.OnReady();
    }

    [SignalHandler("id_pressed")]
    void OnIdPressed(int id) {
        ((ProjectsPanel)GetParent())._IdPressed(id);
    }
}
