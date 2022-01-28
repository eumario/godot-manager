using Godot;
using System;

public class ProjectPopup : PopupMenu
{
    public ProjectLineEntry ProjectLineEntry = null;
    public ProjectIconEntry ProjectIconEntry = null;

    public override void _Ready()
    {
        Connect("id_pressed", this, "OnIdPressed");
    }

    void OnIdPressed(int id) {
        ((ProjectsPanel)GetParent())._IdPressed(id);
    }
}
