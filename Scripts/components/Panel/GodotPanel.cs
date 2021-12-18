using Godot;
using Godot.Collections;
using GodotSharpExtras;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Compression;

public class GodotPanel : Panel
{
#region Nodes
    [NodePath("VB/MC/HC/UseMono")]
    CheckBox UseMono = null;

    [NodePath("VB/MC/HC/DownloadSource")]
    OptionButton DownloadSource = null;

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

    [NodePath("/root/MainWindow/YesNoDialog")]
    YesNoDialog YesNoDialog = null;
#endregion

#region Templates
    PackedScene GodotLE = GD.Load<PackedScene>("res://components/GodotLineEntry.tscn");
#endregion

    Array<Github.Release> Releases = new Array<Github.Release>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
        UseMono.Connect("toggled", this, "OnToggledUseMono");
    }

    public void OnToggledUseMono(bool value) {
        foreach(GodotLineEntry gle in Available.List.GetChildren())
            gle.Mono = value;
        UpdateVisibility();
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

    public async void OnInstallClicked(GodotLineEntry gle) {
        Available.List.RemoveChild(gle);
        Downloading.List.AddChild(gle);
        Downloading.Visible = true;
        gle.ToggleDownloadProgress(true);
        Task blah = gle.StartDownload();
        while (!blah.IsCompleted)
            await this.IdleFrame();
        Downloading.List.RemoveChild(gle);
        
        if (Downloading.List.GetChildCount() == 0)
            Downloading.Visible = false;
        
        CentralStore.Instance.Versions.Add(gle.CreateGodotVersion());
        CentralStore.Instance.SaveDatabase();
        PopulateList();
    }

    public Array<string> RecursiveListDir(string path) {
        Array<string> list = new Array<string>();
        foreach(string dir in System.IO.Directory.EnumerateDirectories(path)) {
            foreach(string file in RecursiveListDir(System.IO.Path.Combine(path,dir).NormalizePath())) {
                list.Add(file);
            }
            list.Add(System.IO.Path.Combine(path,dir).NormalizePath());
        }

        foreach(string file in System.IO.Directory.EnumerateFiles(path)) {
            list.Add(file.NormalizePath());
        }
        
        list.Add(path.NormalizePath());
        
        return list;
    }


    public async void OnUninstallClicked(GodotLineEntry gle) {
        Task<bool> result = YesNoDialog.ShowDialog("Remove Godot Install",$"You are about to uninstall {gle.GodotVersion.Tag}, are you sure you want to continue?");
        while (!result.IsCompleted)
            await this.IdleFrame();

        if (result.Result) {
            Directory dir = new Directory();
            var install = ProjectSettings.GlobalizePath(gle.GodotVersion.Location);
            var cache = ProjectSettings.GlobalizePath(gle.GodotVersion.CacheLocation);
            var files = RecursiveListDir(install);

            foreach (string file in files) {
                dir.Remove(file);
            }
            dir.Remove(cache);
            CentralStore.Instance.Versions.Remove(gle.GodotVersion);
            CentralStore.Instance.SaveDatabase();
            PopulateList();
        }
    }

    public void PopulateList() {
        foreach (Node child in Installed.List.GetChildren())
            child.QueueFree();
        foreach (Node child in Available.List.GetChildren())
            child.QueueFree();
        
        foreach(GithubVersion gv in CentralStore.Instance.GHVersions) {
            GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
            gle.GithubVersion = gv;
            gle.Mono = UseMono.Pressed;
            Available.List.AddChild(gle);
            gle.Connect("install_clicked", this, "OnInstallClicked");
        }

        foreach(GodotVersion gdv in CentralStore.Instance.Versions) {
            GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
            gle.GodotVersion = gdv;
            gle.GithubVersion = gdv.GithubVersion;
            gle.Mono = gdv.IsMono;
            gle.Downloaded = true;
            Installed.List.AddChild(gle);
            gle.Connect("uninstall_clicked", this, "OnUninstallClicked");
        }

        UpdateVisibility();
    }

	private void UpdateVisibility()
	{
        Array<string> gdName = new Array<string>();
        Array<GodotVersion> gdVersion = new Array<GodotVersion>();
        Array<GithubVersion> ghVersion = new Array<GithubVersion>();
		foreach(GodotLineEntry igle in Installed.List.GetChildren()) {
            gdName.Add(igle.GithubVersion.Name);
            gdVersion.Add(igle.GodotVersion);
            ghVersion.Add(igle.GithubVersion);
        }

        foreach(GodotLineEntry agle in Available.List.GetChildren()) {
            foreach(GodotVersion version in gdVersion) {
                if (agle.GithubVersion.Name == version.GithubVersion.Name) {
                    if (UseMono.Pressed) {
                        if (version.IsMono)
                            agle.Visible = false;
                        else
                            agle.Visible = true;
                    } else {
                        if (version.IsMono)
                            agle.Visible = true;
                        else
                            agle.Visible = false;
                    }
                }
            }
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