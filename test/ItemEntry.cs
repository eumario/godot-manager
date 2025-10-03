using Godot;
using System;

[Tool, GlobalClass, SceneTree(root: "Nodes")]
public partial class ItemEntry : PanelContainer
{
    [Signal]
    public delegate void KeySizeChangedEventHandler(float width);
    [Notify, Export] public string KeyName { get; set; }
    [Notify, Export] public string ValueText { get; set; }

    [OnInstantiate]
    public void Initialize(string key, string value)
    {
        KeyName = key;
        ValueText = value;
        Key.Resized += () =>
        {
            EmitSignalKeySizeChanged(Key.Size.X);
        };
    }

    [GodotOverride]
    public void OnReady()
    {
        Key.Text = KeyName;
        Value.Text = ValueText;
        KeyNameChanged += () => Key.Text = KeyName;
        ValueTextChanged += () =>
        {
            Value.Text = ValueText;
            Value.TooltipText = ValueText;
        };
    }

    public override partial void _Ready();
}
