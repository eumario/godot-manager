using System;
using System.Collections.Generic;
using LiteDB;

namespace GodotManager.Library.Data.POCO.AssetLib;

public class Asset
{
    public string AssetId;
    public string Type;
    public string Title;
    public string Author;
    public string AuthorId;
    public string Version;
    public string VersionString;
    public string Category;
    public string CategoryId;
    public string GodotVersion;
    public string Rating;
    public string Cost;
    public string Description;
    public string SupportLevel;
    public string DownloadProvider;
    public string DownloadCommit;
    public string BrowseUrl;
    public string IssuesUrl;
    public string IconUrl;
    public string Searchable;
    public string ModifyDate;
    public string DownloadUrl;
    public List<Preview> Previews;
    public string DownloadHash;
}