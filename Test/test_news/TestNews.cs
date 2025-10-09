using Godot;
using System.Linq;
using GodotManager.Library.Network;
using GodotManager.Library.UI;
using AppContext = GodotManager.Library.Database.AppContext;
using Logger = GodotManager.Library.Util.Logger;

namespace GodotManager.Test.TestNews;

[Tool, GlobalClass, SceneTree(root: "Nodes")]
public partial class TestNews : Control
{
    private AppContext _appContext = null;
    [OnInstantiate]
    public void OnCreate()
    {
        GD.Print("Test news ready.");
    }
    
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        OS.AddLogger(new Logger());
        FetchNews.Disabled = true;
        
        _appContext = AppContext.InitDatabase(ProjectSettings.GlobalizePath("res://Test/test_db.sqlite3"));
        var count = _appContext.AuthorInfo.Count();
        if (count == 0)
        {
            GD.Print("Database does not have Author Info, updating...");
            var authors = new GithubAuthors();
            authors.AuthorFetched += ai =>
            {
                GD.Print($"Author Fetched: {ai.Name}");
                _appContext.AuthorInfo.Add(ai);
            };
            authors.AuthorFetchCompleted += (_, _) =>
            {
                GD.Print($"Fetch and Parsing complete, saving database.");
                _appContext.SaveChanges();
                Callable.From(() => FetchNews.Disabled = false).CallDeferred();
            };
            authors.RefreshAuthors();
        }
        else
        {
            FetchNews.Disabled = false;
        }
        
        FetchNews.Pressed += () =>
        {
            foreach (var child in NewsItems.GetChildren())
                child.QueueFree();

            var news = new NewsAggregator();
            news.NewsArticleFetched += newsArticle =>
            {
                var ai = _appContext.AuthorInfo.Where(x => x.Name == newsArticle.AuthorName).FirstOrDefault();
                if (ai != null)
                {
                    newsArticle.AuthorImagePath = "https://godotengine.org".PathJoin(ai.AvatarUrl);
                }
                else
                {
                    ai = _appContext.AuthorInfo.Where(x => x.Name == "default").FirstOrDefault();
                    if (ai != null)
                        newsArticle.AuthorImagePath = "https://godotengine.org".PathJoin(ai.AvatarUrl);
                }
                var item = ArticleCard.Instantiate(newsArticle);
                Callable.From(() => NewsItems.AddChild(item)).CallDeferred();
            };
            news.NewsFetchCompleted += (_, _) => Callable.From(() => FetchNews.Disabled = false).CallDeferred();
            FetchNews.Disabled = true;
            news.FetchNews();
        };
    }
}
