using System;
using System.Linq;
using Uri = System.Uri;
using SFile = System.IO.File;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using GodotManager.libs.data.Internal;
using GodotManager.libs.util;
using Array = Godot.Collections.Array;

public class NewsPanel : Panel
{
    [NodePath] private VBoxContainer NewsList = null;
    [NodePath] private TextureRect RefreshIcon = null;

    [Resource("res://components/NewsItem.tscn")] private PackedScene NewsItem = null;

    private readonly Uri NEWS_URI = new Uri("https://godotengine.org/rss.json");
    private readonly Uri AUTHOR_URI =
        new Uri("https://raw.githubusercontent.com/godotengine/godot-website/master/_data/authors.yml");
    private readonly Uri BASE_URI = new Uri("https://godotengine.org");
    private GDCSHTTPClient _client = null;
    private DownloadQueue _queue = null;

    public override void _Ready()
    {
        _queue = new DownloadQueue();
        _queue.Name = "DownloadQueue";
        AddChild(_queue);
        this.OnReady();
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
    }

    private void OnPageChanged(int page)
    {
        if (GetParent<TabContainer>().GetCurrentTabControl() == this && NewsList.GetChildCount() == 0)
        {
            RefreshNews();
        }
    }

    [SignalHandler("gui_input", nameof(RefreshIcon))]
    private void OnGuiInput_RefreshIcon(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton iemb))
            return;

        if (iemb.ButtonIndex == 1 && iemb.Pressed)
            RefreshNews();
    }

    private async void RefreshNews()
    {
        if (NewsList.GetChildCount() != 0)
            NewsList.QueueFreeChildren();
        
        AppDialogs.BusyDialog.UpdateHeader(Tr("Loading Authors..."));
        AppDialogs.BusyDialog.UpdateByline(Tr("Fetching authors from GodotEngine.org..."));
        AppDialogs.BusyDialog.ShowDialog();
        InitClient();
        if (CentralStore.Settings.UseProxy)
            _client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, true);
        else
            _client.ClearProxy();

        Task<HTTPClient.Status> cres = _client.StartClient(AUTHOR_URI.Host, AUTHOR_URI.Port, true);

        while (!cres.IsCompleted)
            await this.IdleFrame();

        if (!_client.SuccessConnect(cres.Result))
        {
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Connection Failed", "Unable to fetch news entries from website. Connection refused, or no results provided.");
            return;
        }

        var tresult = _client.MakeRequest(AUTHOR_URI.PathAndQuery);
        while (!tresult.IsCompleted)
            await this.IdleFrame();
        _client.Close();

        var result = tresult.Result;

        if (result.ResponseCode != 200)
        {
            CleanupClient();
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Fetch News Error", $"Failed to fetch news entries from website. (Error Code: {result.ResponseCode}");
            return;
        }
        
        AppDialogs.BusyDialog.UpdateByline(Tr("Parsing author information..."));
        var authors = result.Body.Split("\n");
        var currentAuthor = new AuthorEntry();

        foreach (var (author, index) in authors.WithIndex())
        {
            AppDialogs.BusyDialog.UpdateByline($"Parsing {index} of {authors.Length} ");
            if (author.BeginsWith("- name: "))
                currentAuthor.Name = author.Replace("- name: ", "");

            if (author.BeginsWith("  image: "))
                currentAuthor.AvatarUrl = author.Replace("  image: ", "");

            if (!currentAuthor.HasAll()) continue;
            currentAuthor.Name = currentAuthor.Name.Replace("\"", "").Replace("'", "").StripEdges();
            if (CentralStore.AuthorEntries.All(x => x.Name != currentAuthor.Name))
            {
                CentralStore.AuthorEntries.Add(currentAuthor);
            }
            currentAuthor = new AuthorEntry();
        }

        CentralStore.Instance.SaveDatabase();
        CleanupClient();
        FetchNews();
    }

    private async void FetchNews()
    {
        AppDialogs.BusyDialog.UpdateHeader(Tr("Loading News..."));
        AppDialogs.BusyDialog.UpdateByline(Tr("Fetching news from GodotEngine.org..."));
        InitClient();
        if (CentralStore.Settings.UseProxy)
            _client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, true);
        else
            _client.ClearProxy();

        Task<HTTPClient.Status> cres = _client.StartClient(NEWS_URI.Host, NEWS_URI.Port, true);
        
        while (!cres.IsCompleted)
            await this.IdleFrame();

        if (!_client.SuccessConnect(cres.Result))
        {
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Connection Failed", "Unable to fetch news entries from website.  Connection refused, or no results provided.");
            return;
        }
        
        var tresult = _client.MakeRequest(NEWS_URI.PathAndQuery);
        while (!tresult.IsCompleted)
            await this.IdleFrame();
        _client.Close();

        var result = tresult.Result;
        
        if (result.ResponseCode != 200)
        {
            CleanupClient();
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Fetch News Error", $"Failed to fetch news entries from website.  (Error Code: {result.ResponseCode}");
            return;
        }

        AppDialogs.BusyDialog.UpdateByline(Tr("Parsing news entries..."));

        var jsonResponse = JSON.Parse(result.Body);

        if (jsonResponse.Error != Error.Ok)
        {
            CleanupClient();
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Fetch News Error", $"Failed to parse news entries from website.  (Error Code: {jsonResponse.Error}");
            return;
        }

        var entries = jsonResponse.Result as Dictionary;
        if (entries is null || !entries.Contains("title"))
        {
            AppDialogs.BusyDialog.HideDialog();
            AppDialogs.MessageDialog.ShowMessage("Parse News Error", $"Invalid data returned when attempting to fetch RSS Feed.");
            return;
        }

        foreach (var item in (Array)entries["items"])
        {
            var nitem = (Dictionary)item;
            var newsItem = NewsItem.Instance<NewsItem>();

            newsItem.Headline = "    " + (string)nitem["title"];
            newsItem.Byline = $"    {(string)nitem["dc:creator"]} - {((string)nitem["pubDate"]).Replace("&nbsp;", " ")}";
            newsItem.Url = (string)nitem["guid"];
            newsItem.Blerb = (string)nitem["description"];

            Uri uri = new Uri((string)nitem["image"]);
            string imgPath = $"{CentralStore.Settings.CachePath}/images/news/{uri.AbsolutePath.GetFile()}";
            if (!SFile.Exists(imgPath.GetOSDir().NormalizePath()))
            {
                ImageDownloader dld = new ImageDownloader((string)nitem["image"], imgPath);
                _queue.Push(dld);
                newsItem.SetMeta("imgPath", imgPath);
                newsItem.SetMeta("dld", dld);
            }
            else
                newsItem.Image = imgPath.GetOSDir().NormalizePath();

            var avatar = CentralStore.AuthorEntries.FirstOrDefault(x => x.Name == (string)nitem["dc:creator"]);
            if (avatar is null) avatar = CentralStore.AuthorEntries.FirstOrDefault(x => x.Name == "default");
            if (avatar != null)
            {
                uri = new Uri(BASE_URI, avatar.AvatarUrl);
                imgPath = $"{CentralStore.Settings.CachePath}/images/news/{uri.AbsolutePath.GetFile()}";
                if (!SFile.Exists(imgPath.GetOSDir().NormalizePath()))
                {
                    if (_queue.Queued.All(x => x.Tag != (string)nitem["dc:creator"]))
                    {
                        ImageDownloader dld = new ImageDownloader(uri.ToString(), imgPath, (string)nitem["dc:creator"]);
                        _queue.Push(dld);
                        newsItem.SetMeta("avatarPath", imgPath);
                        newsItem.SetMeta("avatarDld", dld);
                    }
                    else
                    {
                        var dld = _queue.Queued.FirstOrDefault(x => x.Tag == (string)nitem["dc:creator"]);
                        newsItem.SetMeta("avatarPath", imgPath);
                        newsItem.SetMeta("avatarDld", dld);
                    }
                }
                else
                    newsItem.Avatar = imgPath.GetOSDir().NormalizePath();
            }

            NewsList.AddChild(newsItem);
        }
        _queue.StartDownload();
        AppDialogs.BusyDialog.HideDialog();
    }
    
    [SignalHandler("download_completed", nameof(_queue))]
    void OnImageDownloaded(ImageDownloader dld)
    {
        foreach (NewsItem item in NewsList.GetChildren())
        {
            if (item.HasMeta("dld"))
            {
                if ((item.GetMeta("dld") as ImageDownloader) == dld)
                {
                    item.RemoveMeta("dld");
                    string imgPath = item.GetMeta("imgPath") as string;
                    if (SFile.Exists(imgPath.GetOSDir().NormalizePath()))
                    {
                        item.Image = imgPath.GetOSDir().NormalizePath();
                    }
                    else
                    {
                        // Need Generic Image to use instead.
                    }

                    break;
                }
            }

            if (item.HasMeta("avatarDld"))
            {
                if ((item.GetMeta("avatarDld") as ImageDownloader) == dld)
                {
                    item.RemoveMeta("avatarDld");
                    var avatarPath = item.GetMeta("avatarPath") as string;
                    if (SFile.Exists(avatarPath.GetOSDir().NormalizePath()))
                    {
                        item.Avatar = avatarPath.GetOSDir().NormalizePath();
                    }
                    else
                    {
                        // Need Generic Image to use instead.
                    }
                }
            }
        }
    }

    private void InitClient()
    {
        if (_client != null)
            CleanupClient();
        _client = new GDCSHTTPClient();
        _client.Connect("chunk_received", this, "OnChunkReceived");
    }

    private void CleanupClient()
    {
        _client.Disconnect("chunk_received", this, "OnChunkReceived");
        _client.QueueFree();
        _client = null;
    }

    private void OnChunkReceived(int size)
    {
        GD.Print($"Downloaded {size} bytes");
    }
}
