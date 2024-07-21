using System.Collections.Generic;
namespace GodotManager.Library.Data.POCO.AssetLib;

public class QueryResult
{
    public List<AssetResult> Result { get; set; }
    public int Page { get; set; }
    public int Pages { get; set; }
    public int PageLength { get; set; }
    public int TotalItems { get; set; }
}