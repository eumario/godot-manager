using Godot;
using Godot.Collections;
using GodotSharpExtras;
using System;
using System.Linq;
using System.Threading.Tasks;

public class GodotPanel : Panel
{
#region Nodes
    [NodePath("VB/SC/GodotList")]
    VBoxContainer GodotList = null;

    [NodePath("VB/SC/GodotList/Install")]
    CategoryList Installed = null;

    [NodePath("VB/SC/GodotList/Download")]
    CategoryList Downloading = null;

    [NodePath("VB/SC/GodotList/Available")]
    CategoryList Available = null;

    [NodePath("/root/MainWindow/BusyDialog")]
    BusyDialog BusyDialog = null;
#endregion

#region Templates
    PackedScene GodotLE = GD.Load<PackedScene>("res://components/GodotLineEntry.tscn");
#endregion

    // Array<GithubRelease> Releases;
    // RateLimit ApiCalls;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        
        // Releases = Github.Instance.GetReleaseInfo();
        // ApiCalls = Github.Instance.GetLastApiInfo();

        // GD.Print($"Latest: {Releases[0].Name}  ({Releases[0].TagName})");
        // GD.Print($"API Rate Limit Information:\n Usage: {ApiCalls.Remaining} / {ApiCalls.Limit}\nResets in: {ApiCalls.Reset.ToString()}");
    }

    Array<Github.Release> Releases = new Array<Github.Release>();

    public async Task GetReleases() {
        Mutex mutex = new Mutex();
        bool stop = false;
        int page = 1;
        while (stop == false) {
            var tres = Github.Github.Instance.GetReleases(30,page);
            while (!tres.IsCompleted) {
                await ToSignal(Engine.GetMainLoop(), "idle_frame");
            }

            mutex.Lock();
            if (tres.Result == null) {
                OS.Alert("Failed to get Release information from Github", "Github Connection Error");
                stop = true;
            }
            else {
                Array<Github.Release> releases = tres.Result;
                if (releases.Count > 0) {
                    foreach(Github.Release release in releases) {
                        Releases.Add(release);
                    }
                }
                else
                    stop = true;
            }
            page++;
            mutex.Unlock();
        }
    }

    private int downloadedBytes = 0;
    public void OnChunkReceived(int bytes) {
        downloadedBytes += bytes;
        BusyDialog.UpdateByline($"Downloaded {Util.FormatSize(downloadedBytes)}...");
    }

    public async void OnButton_Pressed() {
        if (Github.Github.Instance.GetParent() == null)
            GetTree().Root.AddChild(Github.Github.Instance);
        BusyDialog.UpdateHeader("Fetching Releases from Github...");
        BusyDialog.UpdateByline("Connecting...");
        BusyDialog.ShowDialog();
        Github.Github.Instance.Connect("chunk_received", this, "OnChunkReceived");
        var task = GetReleases();
        while(!task.IsCompleted) {
            await ToSignal(Engine.GetMainLoop(), "idle_frame");
        }
        Github.Github.Instance.Disconnect("chunk_received", this, "OnChunkReceived");
        BusyDialog.HideDialog();

        foreach (Node child in Available.List.GetChildren())
            child.QueueFree();

        foreach(Github.Release release in Releases.OrderByDescending(rf => rf.PublishedAt)) {
            GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
            gle.Label = release.Name;
            gle.Source = release.HtmlUrl;
            gle.Filesize = "Unknown";
            Available.AddChild(gle);
            // GD.Print($"Url: {release.Url}");
            // GD.Print($"HtmlUrl: {release.HtmlUrl}");
            // GD.Print($"AssetsUrl: {release.AssetsUrl}");
            // GD.Print($"UploadUrl: {release.UploadUrl}");
            // GD.Print($"TarballUrl: {release.TarballUrl}");
            // GD.Print($"ZipballUrl: {release.ZipballUrl}");
            // GD.Print($"Id: {release.Id}");
            // GD.Print($"NodeId: {release.NodeId}");
            // GD.Print($"TagName: {release.TagName}");
            // GD.Print($"TargetCommitish: {release.TargetCommitish}");
            // GD.Print($"Name: {release.Name}");
            // GD.Print($"Body: {release.Body}");
            // GD.Print($"Draft: {release.Draft}");
            // GD.Print($"PreRelease: {release.PreRelease}");
            // GD.Print($"CreatedAt: {release.CreatedAt}");
            // GD.Print($"PublishedAt: {release.PublishedAt}");
            // GD.Print($"Author: {release.Author}");
            // GD.Print($"Assets[]: {release.Assets.Count}");
            // GD.Print("-------------------------------------------------------------------------");
            // GD.Print("");
        }
        foreach(Github.Asset asset in Releases[0].Assets) {
            GD.Print($"Url: {asset.Url}");
            GD.Print($"BrowserDownloadUrl: {asset.BrowserDownloadUrl}");
            GD.Print($"Id: {asset.Id}");
            GD.Print($"NodeId: {asset.NodeId}");
            GD.Print($"Name: {asset.Name}");
            GD.Print($"Label: {asset.Label}");
            GD.Print($"State: {asset.State}");
            GD.Print($"ContentType: {asset.ContentType}");
            GD.Print($"Size: {asset.Size}");
            GD.Print($"DownloadCount: {asset.DownloadCount}");
            GD.Print($"CreatedAt: {asset.CreatedAt}");
            GD.Print($"Uploader: {asset.Uploader.Login}");
            GD.Print("-------------------------------------------------------------------------");
            GD.Print("");
        }
    }
}