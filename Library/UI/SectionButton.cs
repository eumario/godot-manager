using Godot;
using System;

namespace GodotManager.Library.UI;

[GlobalClass, Tool, SceneTree(root: "Nodes")]
public partial class SectionButton : PanelContainer
{

    [Signal]
    public delegate void ButtonPressedEventHandler();
    
    [Notify, Export(PropertyHint.Enum, "solid,regular,brands")] public partial string IconStyle { get; set; }
    [Notify, Export] public partial string IconName { get; set; }
    [Notify, Export] public partial string Title { get; set; }
    [Notify, Export] public partial int IconSize { get; set; }

    private static readonly Color SelectedColor = new Color(0.41f, 0.41f, 0.41f, 0.8f);
    private static readonly Color HoverColor = new Color(0.41f, 0.41f, 0.41f, 0.6f);
    private static readonly Color NormalColor = new Color(0.41f, 0.41f, 0.41f, 0.0f);

    private StyleBoxFlat _background;

    private bool _selected = false;

    public SectionButton()
    {
        InitIconStyle("solid");
        InitIconName("circle-question");
        InitIconSize(16);
        InitTitle("");
    }
    
    [GodotOverride]
    public void OnReady()
    {
        _background = GetThemeStylebox("panel") as StyleBoxFlat;
        _background!.BgColor = NormalColor;
        IconNode.Set("icon_type", IconStyle);
        IconNode.Set("icon_name", IconName);
        IconNode.Set("icon_size", IconSize);
        TextNode.Text = Title;
        if (Engine.IsEditorHint())
        {
            TitleChanged += () => TextNode.Text = Title;
            IconStyleChanged += () => IconNode.Set("icon_style", IconStyle);
            IconNameChanged += () => IconNode.Set("icon_name", IconName);
            IconSizeChanged += () => IconNode.Set("icon_size", IconSize);
        }

        MouseEntered += () =>
        {
            if (_selected) return;
            _background!.BgColor = HoverColor;
        };
        MouseExited += () =>
        {
            if (_selected) return;
            _background!.BgColor = NormalColor;
        };
        GuiInput += (iev) =>
        {
            if (iev is not InputEventMouseButton button) return;
            if (!button.Pressed || button.ButtonIndex != MouseButton.Left) return;
            _selected = true;
            _background!.BgColor = SelectedColor;
            EmitSignalButtonPressed();
        };
    }

    public void Deselect(bool hover = false)
    {
        _selected = false;
        _background!.BgColor = hover ? HoverColor : NormalColor;
    }

    public void Select()
    {
        _selected = true;
        _background!.BgColor = SelectedColor;
    }

    public bool IsSelected() => _selected;


    public override partial void _Ready();
}
