using System.Collections.Generic;
namespace GodotManager.Library.Data.POCO.AssetLib;

public class QueryResult
{
    public List<AssetResult> Result;
    public int Page;
    public int Pages;
    public int PageLength;
    public int TotalItems;
}