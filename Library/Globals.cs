using System;
using System.Collections.Generic;
using Godot;

namespace GodotManager.Library;

public partial class Globals : Node
{
    public Vector2I Location;
    public Vector2I Size;

    private List<Action> _mainThreadActions = [];
    
    public override void _Ready()
    {
        TreeExiting += () =>
        {
            Location = DisplayServer.WindowGetPosition();
            Size = DisplayServer.WindowGetSize();
        };
    }
    
    private void ProcessOnMainThread()
    {
        if (_mainThreadActions.Count == 0) return;
        var finished = new List<Action>();
        var iterate = new List<Action>(_mainThreadActions);
        foreach (var action in iterate)
        {
            action.Invoke();
            finished.Add(action);
        }

        foreach (var action in finished) _mainThreadActions.Remove(action);
    }

    public void RunOnMain(Action action) => _mainThreadActions.Add(action);

    public override void _Process(double delta)
    {
        ProcessOnMainThread();
    }
}