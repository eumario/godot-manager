using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotManager.Library.UI;

[SceneTree(root: "Nodes")]
public partial class Sidebar : PanelContainer
{
    private List<SectionButton> _buttons = [];
    [GodotOverride]
    public void OnReady()
    {
        _buttons = Sections.GetChild<VBoxContainer>(0).GetChildren().OfType<SectionButton>().ToList();
        foreach (var section in _buttons)
            section.ButtonPressed += () => ToggleButtons(section);

        DownloadSection.ButtonPressed += () =>
        {
            DownloadSection.Deselect(true);
            // TODO: Handle Showing Downloads...
            GD.Print("Show Downloads...");
        };
        ProjectSection.Select();
    }

    private void ToggleButtons(SectionButton btn)
    {
        foreach(var section in _buttons.Where(sb => sb != btn))
            section.Deselect();
    }

    public override partial void _Ready();
}
