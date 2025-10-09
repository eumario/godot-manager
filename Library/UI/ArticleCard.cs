using Godot;
using System;
using System.Threading;
using GodotManager.Library.FileIO;
using GodotManager.Library.Models.General;
using GodotManager.Library.Network;
using GodotManager.Library.Util;

[Tool, GlobalClass, SceneTree(root: "Nodes")]
public partial class ArticleCard : PanelContainer
{
    private static Thread _mainThread;
    [Notify] public partial NewsItem NewsItem { get; set; }
    
    [OnInstantiate]
    public void OnInitialized(NewsItem newsItem)
    {
        NewsItem = newsItem;
    }
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        _mainThread ??= Thread.CurrentThread;
        NewsItemChanged += UpdateNewsItem;
        UpdateNewsItem();
    }

    private async void UpdateNewsItem()
    {
        if (NewsItem == null) return;
        Header.Text = NewsItem.Headline;
        Description.Text = NewsItem.Blerb.URIDecode();
        Url.Uri = NewsItem.Url;
        Url.Text = NewsItem.Url.Replace("https://", "      ");
        var imgPath = NewsItem.ImagePath;
        if (imgPath.StartsWith("http"))
        {
            var uri = new Uri(imgPath);
            var fileName = GlobalSettings.NewsImagePath.PathJoin(uri.AbsolutePath.GetFile());
            // Remote Resource
            var dld = new ImageDownloader(uri, "news", fileName);
            dld.DownloadCompleted += (_, filePath) => LoadImage(filePath);
            dld.DownloadImage();
        }
        else
        {
            LoadImage(imgPath);
        }

        var aiuri = new Uri(NewsItem.AuthorImagePath);
        var aiFileName = GlobalSettings.NewsAvatarImagePath.PathJoin(aiuri.AbsolutePath.GetFile());
        var aidld = new ImageDownloader(aiuri, "avatar", aiFileName);
        aidld.DownloadCompleted += (_, filePath) => LoadAvatarImage(filePath);
        aidld.DownloadImage();
    }

    private async void LoadImage(string path)
    {
        if (_mainThread != Thread.CurrentThread)
        {
            Callable.From(() => LoadImage(path)).CallDeferred();
            return;
        }

        if (Icon == null)
        {
            Callable.From(() => LoadImage(path)).CallDeferred();
            return;
        }

        var img = await ImageUtils.LoadImage(path);
        Icon.Texture = img;
    }

    private async void LoadAvatarImage(string path)
    {
        if (_mainThread != Thread.CurrentThread)
        {
            Callable.From(() => LoadAvatarImage(path)).CallDeferred();
            return;
        }

        if (AuthorIcon == null)
        {
            Callable.From(() => LoadAvatarImage(path)).CallDeferred();
            return;
        }

        var img = await ImageUtils.LoadImage(path);
        AuthorIcon.Texture = img;
    }
}
