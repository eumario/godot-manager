using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace GodotManager.Library.UI;

[SceneTree(root: "Nodes")]
public partial class Sidebar : PanelContainer
{
    [Export] public Array<PanelContainer> ButtonSections { get; set; }
    private List<SectionButton> _buttons = [];
    [GodotOverride]
    public void OnReady()
    {
        _buttons = Sections.GetChild<VBoxContainer>(0).GetChildren().OfType<SectionButton>().ToList();
        foreach (var section in _buttons)
            section.ButtonPressed += () => ToggleButtons(section);

        Settings.Pressed += () =>
        {
            // TODO: Implement Settings Dialog
            GD.Print("Show Settings...");
        };
        
        DownloadSection.ButtonPressed += () =>
        {
            DownloadSection.Deselect(true);
            // TODO: Handle Showing Downloads...
            GD.Print("Show Downloads...");
            var mainWin = MainWindow.GetInstance();
            if (mainWin!.Downloads.Visible)
                mainWin.HideDownloads();
            else
                mainWin.ShowDownloads();
        };
        ProjectSection.Select();
        foreach (var section in ButtonSections)
            section.Visible = false;
        ButtonSections[0].Visible = true;
    }

    private void ToggleButtons(SectionButton btn)
    {
        foreach(var section in _buttons.Where(sb => sb != btn))
            section.Deselect();
        foreach (var section in ButtonSections)
            section.Visible = false;
        ButtonSections[_buttons.IndexOf(btn)].Visible = true;
    }

    public override partial void _Ready();
}
