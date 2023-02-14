using Godot;

namespace GodotManager.Library;

public partial class Globals : Node
{
    public Vector2I Location;
    public Vector2I Size;
    
    public override void _Ready()
    {
        TreeExiting += () =>
        {
            Location = DisplayServer.WindowGetPosition();
            Size = DisplayServer.WindowGetSize();
        };
    }
}