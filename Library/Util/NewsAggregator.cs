using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Models.General;

namespace GodotManager.Library.Util;

public class NewsAggregator
{
    public delegate void NewsArticleFetchedEventHandler(NewsItem item);
    public event NewsArticleFetchedEventHandler NewsArticleFetched;
    private readonly Uri _newsUri = new Uri("https://godotengine.org/rss.json");

    private readonly Uri _authorUri =
        new Uri("https://raw.githubusercontent.com/godotengine/godot-website/master/_data/authors.yml");

    private readonly Uri _baseUri = new Uri("https://godotengine.org/");
    private List<ImageDownloader> _downloads;

    public async Task FetchNews()
    {
        var news = new DownloadInstance(_newsUri);
        news.Failed += () =>
        {
            // TODO: Handle Failed
        };
        news.Cancelled += () =>
        {
            // TODO: Handle Cancelled
        };
        news.ProgressChanged += (size, total) =>
        {
            // TODO: Handle Progress
        };
        news.Completed += async (bytes) =>
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
                    Headline = nitem["title"].AsString(),
                    AuthorName = nitem["dc:creator"].AsString(),
                    Date = nitem["pubDate"].AsString(),
                    Url = nitem["guid"].AsString(),
                    Blerb = nitem["description"].AsString()
                };

                var uri = new Uri(nitem["image"].AsString());
                var imgPath = GlobalSettings.NewsImagePath.PathJoin(uri.AbsolutePath.GetFile());
                newsItem.ImagePath = !File.Exists(imgPath) ? uri.ToString() : imgPath;
                NewsArticleFetched?.Invoke(newsItem);
            }
        };
        news.StartDownload();
    }
}