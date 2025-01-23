using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace GodotManager.Library.Data.POCO.AssetLib;

public class ApiQuery
{
    public string Url { get; set; }
    public Uri Uri { get; set; }
    public string GodotVersion { get; set; }
    public bool SearchTemplates { get; set; }
    public string SortBy { get; set; }
    public List<string> SupportList { get; set; }
    public int Category { get; set; }
    public bool Reverse { get; set; }
    public string Filter { get; set; }
    public QueryResult LastResult { get; set; }
    public Asset LastAsset { get; set; }
    public int PageIndex { get; set; }

    public Uri BuildUri()
    {
        Uri ??= new Uri(Url);
        var query = new StringBuilder(Url);
        query.Append("/asset");
        query.Append(SearchTemplates ? "?type=project" : "?type=addon");
        query.Append($"&sort={SortBy}");
        query.Append($"&godot_version={GodotVersion}");
        if (SupportList.Count != 0) query.Append($"&support={string.Join("+", SupportList)}");
        if (Category > 0) query.Append($"&category={Category}");
        if (Reverse) query.Append($"&reverse=true");
        if (!string.IsNullOrEmpty(Filter)) query.Append($"&filter={Uri.EscapeDataString(Filter)}");
        if (PageIndex > 0) query.Append($"&page={PageIndex}");
        return new Uri(query.ToString());
    }

    public Uri BuildUri(string id)
    {
        Uri ??= new Uri(Url);
        var query = new StringBuilder(Url);
        query.Append("/asset");
        query.Append('/');
        query.Append(id);
        return new Uri(query.ToString());
    }
}