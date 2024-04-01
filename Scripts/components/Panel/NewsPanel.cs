using System;
using Uri = System.Uri;
using SFile = System.IO.File;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using Array = Godot.Collections.Array;

public class NewsPanel : Panel
{
    [NodePath] private VBoxContainer NewsList = null;
    [NodePath] private TextureRect RefreshIcon = null;

    [Resource("res://components/NewsItem.tscn")] private PackedScene NewsItem = null;

    private readonly Uri NEWS_URI = new Uri("https://godotengine.org/blog/");
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
        {
            foreach (Node item in NewsList.GetChildren())
            {
                item.QueueFree();
            }
        }
        AppDialogs.BusyDialog.UpdateHeader(Tr("Loading News..."));
        AppDialogs.BusyDialog.UpdateByline(Tr("Fetching news from GodotEngine.org..."));
        AppDialogs.BusyDialog.ShowDialog();
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
        var feed = ParseNews(result.Body);
        foreach (Dictionary<string, string> item in feed)
        {
            var newsItem = NewsItem.Instance<NewsItem>();
            newsItem.Headline = "    " + item["title"];
            newsItem.Byline = $"    {item["author"]}{item["date"].Replace("&nbsp;", " ")}";
            newsItem.Url = new Uri(NEWS_URI, item["link"]).ToString();
            newsItem.Blerb = item["contents"];

            //newsItem.Image = item["image"];
            Uri uri = new Uri(item["image"]);
            string imgPath = $"{CentralStore.Settings.CachePath}/images/news/{uri.AbsolutePath.GetFile()}";
            if (!SFile.Exists(imgPath.GetOSDir().NormalizePath()))
            {
                ImageDownloader dld = new ImageDownloader(item["image"], imgPath);
                _queue.Push(dld);
                newsItem.SetMeta("imgPath", imgPath);
                newsItem.SetMeta("dld", dld);
            }
            else
            {
                newsItem.Image = imgPath.GetOSDir().NormalizePath();
            }

            uri = new Uri(item["avatar"]);
            imgPath = $"{CentralStore.Settings.CachePath}/images/news/{uri.AbsolutePath.GetFile()}";
            if (!SFile.Exists(imgPath.GetOSDir().NormalizePath()))
            {
                ImageDownloader dld = new ImageDownloader(item["avatar"], imgPath);
                _queue.Push(dld);
                newsItem.SetMeta("avatarPath", imgPath);
                newsItem.SetMeta("avatarDld", dld);
            }
            else
            {
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

                    break;
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

    private Array<Dictionary<string, string>> ParseNews(string buffer)
    {
        var parsed_news = new Array<Dictionary<string, string>>();

        var xml = new XMLParser();
        var error = xml.OpenBuffer(buffer.ToUTF8());
        if (error != Error.Ok) return parsed_news;
        while (true)
        {
            var err = xml.Read();
            if (err != Error.Ok)
            {
                if (err != Error.FileEof)
                {
                    GD.Print($"Error {err} reading XML");
                }

                break;
            }

            if (xml.GetNodeType() != XMLParser.NodeType.Element || xml.GetNodeName() != "article") continue;
            var tag_open_offset = xml.GetNodeOffset();
            xml.SkipSection();
            xml.Read();
            var tag_close_offset = xml.GetNodeOffset();
            parsed_news.Add(ParseNewsItem(buffer, tag_open_offset, tag_close_offset));
        }

        return parsed_news;
    }

    private Dictionary<string, string> ParseNewsItem(string buffer, ulong begin_ofs, ulong end_ofs)
    {
        var parsed_item = new Dictionary<string, string>();
        var xml = new XMLParser();
        var error = xml.OpenBuffer(buffer.ToUTF8());
        if (error != Error.Ok)
        {
            GD.PrintErr($"Error parsing news item.  Error Code: {error}");
            return null;
        }

        xml.Seek(begin_ofs);

        while (xml.GetNodeOffset() != end_ofs)
        {
            if (xml.GetNodeType() == XMLParser.NodeType.Element)
            {
                switch (xml.GetNodeName())
                {
                    case "div":
                        // <div class="thumbnail" style="background-image: url('https://godotengine.org/storage/app/uploads/....');" href="https://godotengine.org/article/....."> // Old Link
                        // <div class="thumbnail" style="background-image: url('/storage/blog/covers/dev/....');" href="/articles/......"> // New Link
                        if (xml.GetNamedAttributeValueSafe("class").Contains("thumbnail"))
                        {
                            var image_style = xml.GetNamedAttributeValueSafe("style");
                            var url_start = image_style.Find("'") + 1;
                            var url_end = image_style.FindLast("'");
                            var image_url = BASE_URI.AbsoluteUri + image_style.Substr(url_start + 1, url_end - url_start - 1);

                            parsed_item["image"] = image_url;
                            parsed_item["link"] = xml.GetNamedAttributeValueSafe("href");
                        }

                        break;

                    case "h3":
                        // <h3>Article Title</h3>
                        xml.Read();
                        parsed_item["title"] = xml.GetNodeType() == XMLParser.NodeType.Text
                            ? xml.GetNodeData().StripEdges()
                            : "";

                        break;
                    case "span":
                        // <span class="date">&nbsp;-&nbsp;dd Month year</span>
                        if (xml.GetNamedAttributeValueSafe("class").Contains("date"))
                        {
                            xml.Read();
                            parsed_item["date"] = xml.GetNodeType() == XMLParser.NodeType.Text ? xml.GetNodeData() : "";
                        }
                        // <span class="by">Author Name</span>
                        if (xml.GetNamedAttributeValue("class").Contains("by"))
                        {
                            xml.Read();
                            parsed_item["author"] = xml.GetNodeType() == XMLParser.NodeType.Text
                                ? xml.GetNodeData().StripEdges()
                                : "";
                        }

                        break;
                    case "p":
                        // <p class="excerpt">An excerpt of the blog entry to be read in.</p>
                        if (xml.GetNamedAttributeValue("class").Contains("excerpt"))
                        {
                            xml.Read();
                            parsed_item["contents"] =
                                xml.GetNodeType() == XMLParser.NodeType.Text ? xml.GetNodeData() : "";
                        }

                        break;
                    case "img":
                        // <img class="avatar" width="25" height="25" src="https://godotengine.org/storage/app/uploads/public/....." alt="">
                        if (xml.GetNamedAttributeValue("class").Contains("avatar"))
                        {
                            var part = xml.GetNamedAttributeValue("src");
                            parsed_item["avatar"] = BASE_URI.AbsoluteUri + part.Substr(1, part.Length - 1);
                        }
                        break;
                }
            }

            xml.Read();
        }

        return parsed_item;
    }
}
