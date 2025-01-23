using System;
using System.Collections.Generic;
using Godot;
using GodotManager.Library.Components.Controls;

namespace GodotManager.Library;

public partial class Globals : Node
{
    public static Globals? Instance { get; private set; }
    public Vector2I Location;
    public Vector2I Size;

    private List<Action> _mainThreadActions = [];
    
    public override void _Ready()
    {
        Instance = this;
        TreeExiting += () =>
        {
            Location = DisplayServer.WindowGetPosition();
            Size = DisplayServer.WindowGetSize();
        };
    }
}