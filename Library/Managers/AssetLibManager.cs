using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Data.POCO.AssetLib;

namespace GodotManager.Library.Managers;

public class AssetLibManager
{
    private static readonly AssetLibManager InternalInstance;
    
    public static readonly AssetLibManager Instance = InternalInstance ??= new AssetLibManager();
    
    public static ApiQuery LastQuery;

    public delegate void ChunkReceivedEventHandler(int size);
    public static event ChunkReceivedEventHandler ChunkReceived;

    public delegate void RequestCompletedEventHandler();
    public static event RequestCompletedEventHandler RequestCompleted;

    private static readonly string[] SortKey =
    [
        "updated",
        "updated",
        "name",
        "name",
        "cost",
        "cost"
    ];

    private static readonly string[] SortText =
    [
        "Recently Updated",
        "Least Recently Updated",
        "Name (A-Z)",
        "Name (Z-A)",
        "License (A-Z)",
        "License (Z-A)"
    ];

    private static readonly string[] SupportKey =
    [
        "official",
        "community",
        "testing"
    ];

    public static async Task<ConfigureResult> Configure(string url, bool templatesOnly)
    {
        var uri = new Uri(url);
        using var api = new HttpApiRequest(uri);
        var result = await api.MakeRequest<ConfigureResult>(Path.Join(uri.AbsolutePath, "configure") +
                                           (templatesOnly ? "?type=project" : "?type=addon"));
        return result;
    }

    public static async Task<QueryResult> Search(string url, string godotVersion, bool templatesOnly, int sortBy,
        List<string> supportList, int category, string filter = "")
    {
        var query = new ApiQuery()
        {
            Url = url,
            GodotVersion = godotVersion,
            SearchTemplates = templatesOnly,
            SortBy = SortKey[sortBy],
            SupportList = supportList,
            Category = category,
            Filter = filter
        };
        LastQuery = query;
        using var api = new HttpApiRequest(query.BuildUri());
        var result = await api.MakeRequest<QueryResult>();
        query.LastResult = result;
        return result;
    }

    public static async Task<QueryResult> SearchPage(int index)
    {
        if (LastQuery == null) return null;
        if (index <= 0 && index > LastQuery.LastResult.Pages) return null;
        LastQuery.PageIndex = index;
        using var api = new HttpApiRequest(LastQuery.BuildUri());
        var result = await api.MakeRequest<QueryResult>();
        LastQuery.LastResult = result;
        return result;
    }
}