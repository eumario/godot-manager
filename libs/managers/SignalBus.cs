using Godot;

public class SignalBus : Node
{
    [Signal] public delegate void update_projects();
}