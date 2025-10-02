namespace GodotManager.Library.Util;

public class VersionUrl
{
    public VersionUrl()
    {
    }

    public VersionUrl(string url, int size)
    {
        Url = url;
        Size = size;
    }

    public string Url { get; set; }
    public int Size { get; set; }
}