using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Models.General;
using GodotManager.Library.Util;

namespace GodotManager.Library.Network;

public class NewsAggregator
{
    public delegate void NewsArticleFetchedEventHandler(NewsItem item);
    public event NewsArticleFetchedEventHandler NewsArticleFetched;
    public event EventHandler NewsFetchCompleted;
    private readonly Uri _newsUri = new("https://godotengine.org/rss.json");

    private readonly Uri _baseUri = new Uri("https://godotengine.org/");

    public void FetchNews()
    {
        var news = new DownloadInstance(_newsUri);
        news.Failed += (_, _) =>
        {
            // TODO: Handle Failed
        };
        news.Cancelled += (_, _) =>
        {
            // TODO: Handle Cancelled
        };
        news.ProgressChanged += (sender, progress) =>
        {
            // TODO: Handle Progress
        };
        news.Completed += (sender, bytes) =>
        {
            // TODO: Parse Data
            var data = Json.ParseString(bytes.GetStringFromUtf8()).AsGodotDictionary();
            if (!data.ContainsKey("title"))
            {
                GD.Print("Failed to parse RSS stream...");
                return;
            }

            foreach (var item in data["items"].AsGodotArray())
            {
                var nitem = item.AsGodotDictionary();
                var newsItem = new NewsItem()
                {
                    Headline = nitem["title"].AsString().Replace("&#39;", "'").Replace("&amp;", "&"),
                    AuthorName = nitem["dc:creator"].AsString(),
                    Date = nitem["pubDate"].AsString(),
                    Url = nitem["guid"].AsString(),
                    Blerb = nitem["description"].AsString().Replace("&#39;", "'").Replace("&amp;", "&")
                };

                var uri = new Uri(nitem["image"].AsString());
                var imgPath = GlobalSettings.NewsImagePath.PathJoin(uri.AbsolutePath.GetFile());
                newsItem.ImagePath = !File.Exists(imgPath) ? uri.ToString() : imgPath;
                NewsArticleFetched?.Invoke(newsItem);
            }
            NewsFetchCompleted?.Invoke(this, EventArgs.Empty);
        };
        news.StartDownload();
    }
}
