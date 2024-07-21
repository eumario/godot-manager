using System;
using System.Collections.Generic;
using LiteDB;

namespace GodotManager.Library.Data.POCO.AssetLib;

public class Asset
{
    public string AssetId { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string AuthorId { get; set; }
    public string Version { get; set; }
    public string VersionString { get; set; }
    public string Category { get; set; }
    public string CategoryId { get; set; }
    public string GodotVersion { get; set; }
    public string Rating { get; set; }
    public string Cost { get; set; }
    public string Description { get; set; }
    public string SupportLevel { get; set; }
    public string DownloadProvider { get; set; }
    public string DownloadCommit { get; set; }
    public string BrowseUrl { get; set; }
    public string IssuesUrl { get; set; }
    public string IconUrl { get; set; }
    public string Searchable { get; set; }
    public string ModifyDate { get; set; }
    public string DownloadUrl { get; set; }
    public List<Preview> Previews { get; set; }
    public string DownloadHash { get; set; }
}