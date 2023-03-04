using GodotManager.Library.Data.POCO.Internal;

namespace GodotManager.Library.Components.Controls;

public interface IProjectIcon
{
    public bool MissingProject { get; set; }
    public GodotVersion GodotVersion { get; set; }
    public ProjectFile ProjectFile { get; set; }
}