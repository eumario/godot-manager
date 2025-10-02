namespace GodotManager.Library.Models;

public class ProjectFile
{
    public int Id { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }
    public string ProjectFilePath { get; set; }
    public EngineVersion EngineVersion { get; set; }
}