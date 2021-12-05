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
    
    [NodePath("/root/MainWindow/NewVersion")]
    NewVersion NewVersion = null;
#endregion

#region Templates
    PackedScene GodotLE = GD.Load<PackedScene>("res://components/GodotLineEntry.tscn");
#endregion

    Array<Github.Release> Releases = new Array<Github.Release>();
    // RateLimit ApiCalls;

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        this.OnReady();
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
    }

    async void OnPageChanged(int page) {
        if (GetParent<TabContainer>().GetCurrentTabControl() == this) {
            if (CentralStore.Instance.GHVersions.Count == 0) {
                var t = GatherReleases();
                while(!t.IsCompleted) {
                    await this.IdleFrame();
                }
            } else {
                var tres = Github.Github.Instance.GetLatestRelease();
                while (!tres.IsCompleted) {
                    await this.IdleFrame();
                }
                var gv = GithubVersion.FromAPI(tres.Result);                
                var l = from version in CentralStore.Instance.GHVersions
                        where version.Name == gv.Name
                        select gv;
                var c = l.FirstOrDefault<GithubVersion>();
                if (c == null) {
                    CentralStore.Instance.GHVersions.Clear();
                    var t = GatherReleases();
                    while (!t.IsCompleted) {
                        await this.IdleFrame();
                    }
                    NewVersion.UpdateReleaseInfo(tres.Result);
                    NewVersion.Visible = true;
                }
            }
            PopulateList();
        }
    }

    public void PopulateList() {
        foreach (Node child in Available.List.GetChildren())
            child.QueueFree();
        
        foreach(GithubVersion gv in CentralStore.Instance.GHVersions) {
            GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
            gle.GithubVersion = gv;
            Available.AddChild(gle);
        }
    }

    public async Task GetReleases() {
        Mutex mutex = new Mutex();
        bool stop = false;
        int page = 1;
        while (stop == false) {
            var tres = Github.Github.Instance.GetReleases(30,page);
            while (!tres.IsCompleted) {
                await this.IdleFrame();
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

    public async Task GatherReleases() {
        BusyDialog.UpdateHeader("Fetching Releases from Github...");
        BusyDialog.UpdateByline("Connecting...");
        BusyDialog.ShowDialog();
        downloadedBytes = 0;
        Github.Github.Instance.Connect("chunk_received", this, "OnChunkReceived");
        var task = GetReleases();
        while(!task.IsCompleted) {
            await this.IdleFrame();
        }

        Github.Github.Instance.Disconnect("chunk_received", this, "OnChunkReceived");
        
        BusyDialog.UpdateHeader("Processing Release Information from Github...");
        BusyDialog.UpdateByline($"Processing 0/{Releases.Count}");
        int i = 0;
        foreach(Github.Release release in Releases) {
            i++;
            BusyDialog.UpdateByline($"Processing {i}/{Releases.Count}");
            GithubVersion gv = GithubVersion.FromAPI(release);
            CentralStore.Instance.GHVersions.Add(gv);
            await this.IdleFrame();
        }
        CentralStore.Instance.SaveDatabase();

        BusyDialog.HideDialog();
    }
}