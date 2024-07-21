namespace GodotManager.Library.Data.POCO.AssetLib;

public class AssetResult
{
    public string AssetId { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string AuthorId { get; set; }
    public string Category { get; set; }
    public string GodotVersion { get; set; }
    public string Rating { get; set; }
    public string Cost { get; set; }
    public string SupportLevel { get; set; }
    public string IconUrl { get; set; }
    public string Version { get; set; }
    public string VersionString { get; set; }
    public string ModifyDate { get; set; }

    public override string ToString()
    {
        return $"Asset {AssetId} - {Title} - {Category} - {Cost} - {IconUrl} - {Version} - {ModifyDate}";
    }
}