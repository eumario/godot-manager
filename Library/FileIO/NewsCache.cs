#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using GodotManager.Library.Models.General;
using GodotManager.Library.Util;

namespace GodotManager.Library.FileIO;

public class NewsCache
{
    public DateTime LastUpdated { get; set; }
    public List<NewsItem> Items { get; set; }

    public NewsCache()
    {
        LastUpdated = DateTime.MinValue;
        Items = [];
    }

    public static NewsCache? Load()
    {
        if (!File.Exists(GlobalSettings.NewsCachePath))
            return null;
        var data = File.ReadAllText(GlobalSettings.NewsCachePath);
        var nc = JsonSerializer.Deserialize<NewsCache>(data);
        return nc;
    }

    public void Save()
    {
        var data = JsonSerializer.Serialize(this);
        File.WriteAllText(GlobalSettings.NewsCachePath, data);
    }
}