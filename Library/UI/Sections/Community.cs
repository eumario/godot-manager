#nullable enable
using Godot;
using System;
using System.IO;
using System.Linq;
using Godot.Collections;
using GodotManager;
using GodotManager.Library.FileIO;
using GodotManager.Library.Models.General;
using GodotManager.Library.Network;
using GodotManager.Library.UI;
using GodotManager.Library.Util;

[GlobalClass, SceneTree(root: "Nodes")]
public partial class Community : PanelContainer
{
    [Notify, Export] public partial Array<ScrollContainer> TabWindows { get; set; }
    public override partial void _Ready();

    public NewsCache? NewsCache { get; set; }
    
    [GodotOverride]
    public void OnReady()
    {
        NavBar.TabChanged += lindx =>
        {
            var indx = (int)lindx;
            if (TabWindows[indx] != null)
            {
                HideSections();
                TabWindows[indx].Show();
            }
        };
        HideSections();
        TabWindows[0].Show();

        Callable.From(async void () =>
        {
            while (MainWindow.GetInstance() == null)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            FetchAuthors();
        }).CallDeferred();
    }

    public void FetchNews()
    {
        var dbContext = MainWindow.GetInstance()?.Context;
        if (dbContext == null)
            return;
        
        NewsCache ??= File.Exists(GlobalSettings.NewsCachePath) ? NewsCache.Load() : new NewsCache();
        if ((DateTime.Now - NewsCache.LastUpdated) > TimeSpan.FromHours(24))
        {
            var news = new NewsAggregator();
            news.NewsArticleFetched += item =>
            {
                if (NewsCache.Items.Any(x => x.Url == item.Url))
                    return;
                var ai = dbContext.AuthorInfo.Where(x => x.Name == item.AuthorName).FirstOrDefault() ?? dbContext.AuthorInfo.Where(x => x.Name == "default").FirstOrDefault();
                if (ai != null)
                    item.AuthorImagePath = "https://godotengine.org".PathJoin(ai.AvatarUrl);
                NewsCache.Items.Add(item);
            };
            news.NewsFetchCompleted += (_, _) =>
            {
                NewsCache.LastUpdated = DateTime.Now;
                NewsCache.Save();
                Callable.From(() =>
                {
                    ClearNews();
                    foreach (var nitem in NewsCache.Items.Select(item => ArticleCard.Instantiate(item)))
                        NewsItemList.AddChild(nitem);
                }).CallDeferred();
            };
            news.FetchNews();
        }
        else
        {
            ClearNews();
            foreach (var nitem in NewsCache.Items.Select(item => ArticleCard.Instantiate(item)))
                NewsItemList.AddChild(nitem);
        }
    }

    public void FetchAuthors()
    {
        var dbContext = MainWindow.GetInstance()?.Context;
        if (dbContext == null)
            return;
        var count = dbContext.AuthorInfo.Count();
        if (count == 0)
        {
            var authors = new GithubAuthors();
            authors.AuthorFetched += ai =>
            {
                dbContext.AuthorInfo.Add(ai);
            };
            authors.AuthorFetchCompleted += (_, _) =>
            {
                dbContext.SaveChanges();
                FetchNews();
            };
            authors.RefreshAuthors();
        }
        else
            FetchNews();
    }

    private void HideSections()
    {
        foreach (var node in TabWindows)
            node?.Hide();
    }

    public void ClearNews()
    {
        foreach (var node in NewsItemList.GetChildren())
            node.QueueFree();
    }
}
