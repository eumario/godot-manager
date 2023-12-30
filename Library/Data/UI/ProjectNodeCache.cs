using Godot;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Data.POCO.Internal;

namespace GodotManager.Library.Data.UI;

public partial class ProjectNodeCache : RefCounted
{
    public ProjectFile ProjectFile { get; set; }
    public ProjectLineItem ListView { get; set; }
    public ProjectIconItem GridView { get; set; }
    public ProjectLineItem CategoryView { get; set; }

    public void QueueFree()
    {
        ListView.QueueFree();
        GridView.QueueFree();
        CategoryView.QueueFree();
    }
}