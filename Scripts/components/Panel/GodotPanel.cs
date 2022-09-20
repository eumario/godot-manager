using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Compression;
using Uri = System.Uri;
using Directory = System.IO.Directory;
using Path = System.IO.Path;
using Guid = System.Guid;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using Mirrors;

public class GodotPanel : Panel
{
    #region Nodes
    // [NodePath("VB/MC/HC/UseMono")]
    // CheckBox UseMono = null;

    [NodePath("VB/MC/HC/PC/TagSelection")]
    MenuButton TagSelection = null;

    [NodePath("VB/MC/HC/ActionButtons")]
    ActionButtons ActionButtons = null;

    [NodePath("VB/MC/HC/DownloadSource")]
    OptionButton DownloadSource = null;

    [NodePath("VB/SC/GodotList/Install")]
    CategoryList Installed = null;

    [NodePath("VB/SC/GodotList/Download")]
    CategoryList Downloading = null;

    [NodePath("VB/SC/GodotList/Available")]
    CategoryList Available = null;
    #endregion

    #region Templates

    [Resource("res://components/GodotLineEntry.tscn")]
    private PackedScene GodotLE = null;

    [Resource("res://components/EnginePopup.tscn")]
    private PackedScene EnginePopup = null;

    #endregion

    private EnginePopup _enginePopup = null;

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        this.OnReady();
        _enginePopup = EnginePopup.Instance<EnginePopup>();
        _enginePopup.Name = "EngineContextMenu";
        AddChild(_enginePopup);
        
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
        AppDialogs.AddCustomGodot.Connect("added_custom_godot", this, "PopulateList");
        DownloadSource.Clear();
        DownloadSource.AddItem("Github");

        if (CentralStore.Mirrors.Count == 0 || CentralStore.Settings.LastMirrorCheck < (DateTime.UtcNow - CentralStore.Settings.CheckInterval)) {
            var res = MirrorManager.Instance.GetMirrors();
            while (!res.IsCompleted)
                await this.IdleFrame();
            
            foreach(MirrorSite site in res.Result) {
                var cres = from csite in CentralStore.Mirrors
                        where csite.Id == site.Id
                        select csite;
                if (cres.FirstOrDefault<MirrorSite>() == null) {
                    CentralStore.Mirrors.Add(site);
                    CentralStore.MRVersions[site.Id] = new Array<MirrorVersion>();
					CentralStore.Settings.LastUpdateMirrorCheck[site.Id] = new UpdateCheck() {
                        LastCheck = DateTime.UtcNow - TimeSpan.FromDays(1)
                    };
				}
            }
        }

        foreach(MirrorSite site in CentralStore.Mirrors)
            DownloadSource.AddItem(site.Name, site.Id);

        TagSelection.GetPopup().HideOnCheckableItemSelection = false;
        TagSelection.GetPopup().Connect("id_pressed", this, "OnIdPressed_TagSelection");

        // Translations for Menu Items
        TagSelection.UpdateTr(0, Tr("Mono / C#"));
        TagSelection.UpdateTr(1, Tr("Release Type"));
        TagSelection.UpdateTr(2, Tr("Stable"));
        TagSelection.UpdateTr(3, Tr("Alpha"));
        TagSelection.UpdateTr(4, Tr("Beta"));
        TagSelection.UpdateTr(5, Tr("Release Candidate"));

        OnlyMono();

        AppDialogs.ManageCustomDownloads.Connect("update_list", this, "OnUpdateList");
    }

    async void OnIdPressed_TagSelection(int id) {
        if (id == 0)
            TagSelection.GetPopup().SetItemChecked(id, !IsMono());
        else {
            for(int i = 2; i < TagSelection.GetPopup().GetItemCount(); i++) {
                TagSelection.GetPopup().SetItemChecked(i, (i == id));
            }
        }
        await PopulateList();
    }

    bool IsMono() {
        return TagSelection.GetPopup().IsItemChecked(0);
    }

    bool IsStable() {
        return TagSelection.GetPopup().IsItemChecked(2);
    }

    bool IsAlpha() {
        return TagSelection.GetPopup().IsItemChecked(3);
    }

    bool IsBeta() {
        return TagSelection.GetPopup().IsItemChecked(4);
    }

    bool IsRC() {
        return TagSelection.GetPopup().IsItemChecked(5);
    }

    void OnlyMono() {
        if (DownloadSource.Selected == 0)
        {
            for (int i = 2; i < TagSelection.GetPopup().GetItemCount(); i++) {
                TagSelection.GetPopup().SetItemDisabled(i, true);
            }
        }
    }

    void AllTags() {
        if (DownloadSource.Selected > 0)
        {
            for (int i = 2; i < TagSelection.GetPopup().GetItemCount(); i++) {
                TagSelection.GetPopup().SetItemDisabled(i, false);
            }
        }
    }

    [SignalHandler("item_selected", nameof(DownloadSource))]
    async void OnItemSelected_DownloadSource(int index)
    {
        if (index == 0) {
            OnlyMono();
            if (CentralStore.GHVersions.Count == 0) {
                var t = GatherReleases();
                while(!t.IsCompleted) {
                    await this.IdleFrame();
                }
            } else {
                if (CentralStore.Settings.CheckForUpdates &&
                    (DateTime.UtcNow - CentralStore.Settings.LastCheck) >= CentralStore.Settings.CheckInterval)
                {
                    await CheckForUpdates();
                }
            }
        } else {
            AllTags();
            int id = DownloadSource.GetSelectedId();
            if (CentralStore.MRVersions[id].Count == 0) {
                await GatherReleases();
            } else {
                if (CentralStore.Settings.CheckForUpdates &&
                    (DateTime.UtcNow - CentralStore.Settings.LastUpdateMirrorCheck[id].LastCheck) >= CentralStore.Settings.CheckInterval) {
					await CheckForUpdates();
				}
            }
        }

        if (CentralStore.Settings.UseLastMirror)
            CentralStore.Settings.LastEngineMirror = index;
        await PopulateList();
    }

    [SignalHandler("clicked", nameof(ActionButtons))]
    async void OnActionClicked(int index) {
        switch(index) {
            case 0:     // Add Custom Godot
                AppDialogs.AddCustomGodot.ShowDialog();
                break;
            case 1:     // Manage Custom Godot Downloads
                AppDialogs.ManageCustomDownloads.ShowDialog();
                break;
            case 2:     // Manually Check for Updates for Godot
                await CheckForUpdates();
                break;
        }
    }

    async void OnPageChanged(int page) {
        if (GetParent<TabContainer>().GetCurrentTabControl() != this) return;

        if (CentralStore.Settings.UseLastMirror)
        {
            DownloadSource.Selected = CentralStore.Settings.LastEngineMirror;
            if (CentralStore.Settings.LastEngineMirror == 0)
                OnlyMono();
            else
                AllTags();
        }

        if (DownloadSource.Selected == 0) {
            if (CentralStore.GHVersions.Count == 0) {
                var t = GatherReleases();
                while(!t.IsCompleted) {
                    await this.IdleFrame();
                }
            } else {
                if (CentralStore.Settings.CheckForUpdates &&
                    (DateTime.UtcNow - CentralStore.Settings.LastCheck) >= CentralStore.Settings.CheckInterval)
                {
                    await CheckForUpdates();
                }
            }
        } else {
            MirrorSite site = CentralStore.Mirrors[DownloadSource.Selected - 1];
            if (CentralStore.MRVersions[site.Id].Count == 0) {
                var t = GatherReleases();
                while (!t.IsCompleted)
                    await this.IdleFrame();
            } else {
                if (CentralStore.Settings.CheckForUpdates &&
                    (DateTime.UtcNow - CentralStore.Settings.LastUpdateMirrorCheck[site.Id].LastCheck) >= CentralStore.Settings.CheckInterval)
                {
                    await CheckForUpdates();
                }
            }
        }
        await PopulateList();
    }

	public async Task CheckForUpdates()
	{
		MirrorSite site = null;
		if (DownloadSource.Selected == 0)
		{
			AppDialogs.BusyDialog.UpdateHeader(Tr("Grabbing information from Github"));
			AppDialogs.BusyDialog.UpdateByline(Tr("Getting the latest version information from Github for Godot Engine..."));
		}
		else
		{
			site = CentralStore.Mirrors[DownloadSource.Selected - 1];
			AppDialogs.BusyDialog.UpdateHeader(string.Format(Tr("Grabbing information for mirror {0}"), site.Name));
			AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Grabbing the latest version information for mirror {0} for Godot Engine..."), site.Name));
		}
		AppDialogs.BusyDialog.ShowDialog();
		if (DownloadSource.Selected == 0)
		    await CheckForGithub();
		else
			await CheckForMirror();

        if (DownloadSource.Selected == 0)
    		CentralStore.Settings.LastCheck = DateTime.UtcNow;
        else
			CentralStore.Settings.LastUpdateMirrorCheck[site.Id].LastCheck = DateTime.UtcNow;

		AppDialogs.BusyDialog.HideDialog();
		CentralStore.Instance.SaveDatabase();
	}

    private async Task CheckForMirror() {
		int id = DownloadSource.GetSelectedId();
		CentralStore.MRVersions[id].Clear();
		var t = GatherMirrorReleases();
        while (!t.IsCompleted)
			await this.IdleFrame();
		await PopulateList();
	}

	private async Task CheckForGithub()
	{
        var tres = Github.Github.Instance.GetLatestRelease();
		while (!tres.IsCompleted)
		{
			await this.IdleFrame();
		}
		var gv = GithubVersion.FromAPI(tres.Result);
		var l = from version in CentralStore.GHVersions
				where version.Name == gv.Name
				select gv;
		var c = l.FirstOrDefault<GithubVersion>();
		if (c == null)
		{
			CentralStore.GHVersions.Clear();
			var t = GatherGithubReleases();
			while (!t.IsCompleted)
			{
				await this.IdleFrame();
			}
			//AppDialogs.NewVersion.UpdateReleaseInfo(tres.Result);
			//AppDialogs.NewVersion.Visible = true;
			await PopulateList();
			AppDialogs.NewVersion.Connect("download_update", this, "OnDownloadUpdate");
			AppDialogs.NewVersion.ShowDialog(tres.Result);
		}
	}

	async void OnDownloadUpdate(Github.Release release, bool useMono) {
        AppDialogs.NewVersion.Disconnect("download_update", this, "OnDownloadUpdate");
        foreach (GodotLineEntry gle in Available.List.GetChildren()) {
            if (gle.GithubVersion.Name == release.Name) {
                gle.Mono = useMono;
                await OnInstallClicked(gle);
                break;
            }
        }
    }

    async void OnDownloadCompleted(GodotInstaller installer, GodotLineEntry gle) {
        Downloading.List.RemoveChild(gle);
        if (Downloading.List.GetChildCount() == 0)
            Downloading.Visible = false;
        
        gle.StopDownloadStats();
        installer.Install();

        CentralStore.Versions.Add(installer.GodotVersion);
        
        if (CentralStore.Versions.Count == 0) {
            CentralStore.Settings.DefaultEngine = installer.GodotVersion.Id;
            gle.ToggleDefault(true);
        }

        CentralStore.Instance.SaveDatabase();
        gle.Downloaded = true;
        gle.ToggleDownloadProgress(false);

        await PopulateList();
    }

    void OnUpdateList()
    {
        PopulateList();
    }

    async void OnDownloadFailed(GodotInstaller installer, HTTPClient.Status status, GodotLineEntry gle) {
        Downloading.List.RemoveChild(gle);
        if (Downloading.List.GetChildCount() == 0)
            Downloading.Visible = false;
        
        Available.List.AddChild(gle);
        gle.ToggleDownloadProgress(false);
        gle.Downloaded = false;

        string errDesc = "";
        Uri dl = new Uri(installer.GodotVersion.Url);
        switch(status) {
            case HTTPClient.Status.CantConnect:
                errDesc = string.Format(Tr("Unable to connect to server {0}"), dl.Host);
                break;
            case HTTPClient.Status.CantResolve:
                errDesc = string.Format(Tr("Unable to resolve server {0}"), dl.Host);
                break;
            case HTTPClient.Status.ConnectionError:
                errDesc = string.Format(Tr($"Unable to connect to server {0}:{1}"), dl.Host, dl.Port);
                break;
            case HTTPClient.Status.Requesting:
                errDesc = string.Format(Tr("Request to server {0} failed to produce a result."), dl.Host);
                break;
            case HTTPClient.Status.SslHandshakeError:
                errDesc = string.Format(Tr("SSL certificate/Communication failed with {0}."), dl.Host);
                break;
            case HTTPClient.Status.Body:
                errDesc = string.Format(Tr("Unable to save Cache file to disk at location {0}."),installer.GodotVersion.CacheLocation);
                break;
            default:
                errDesc = Tr("Unknown error has ocurred, unable to download package.");
                break;
        }

        var res = await AppDialogs.YesNoDialog.ShowDialog(Tr("Download Engine Failed"), errDesc, Tr("Retry"), Tr("Cancel"));
        if (res)
            await OnInstallClicked(gle);
    }

	async Task OnInstallClicked(GodotLineEntry gle) {
        Available.List.RemoveChild(gle);
        Downloading.List.AddChild(gle);
        Downloading.Visible = true;
        gle.ToggleDownloadProgress(true);

        GodotInstaller installer = null;

        if (gle.GithubVersion == null && gle.MirrorVersion == null)
            installer = GodotInstaller.FromCustomEngineDownload(gle.CustomEngine);
        else if (gle.GithubVersion == null && gle.CustomEngine == null)
            installer = GodotInstaller.FromMirror(gle.MirrorVersion, IsMono());
        else
            installer = GodotInstaller.FromGithub(gle.GithubVersion, IsMono());
        
        installer.Connect("chunk_received", gle, "OnChunkReceived");
        installer.Connect("download_completed", this, "OnDownloadCompleted", new Array { gle });
        installer.Connect("download_failed", this, "OnDownloadFailed", new Array { gle });
        
        gle.ToggleDownloadProgress(true);

        gle.StartDownloadStats(installer.DownloadSize);

        await installer.Download();
    }


    async void OnUninstallClicked(GodotLineEntry gle) {
        Task<bool> result;
        GodotInstaller installer = GodotInstaller.FromVersion(gle.GodotVersion);
        if (gle.Source == gle.GodotVersion.Location) {
            result = AppDialogs.YesNoDialog.ShowDialog(
                Tr("Remove Godot Install"),
                string.Format(Tr("You are about to remove the reference to {0}, are you sure you want to continue?"),gle.GodotVersion.Tag)
            );
        } else {
            result = AppDialogs.YesNoDialog.ShowDialog(
                Tr("Remove Godot Install"),
                string.Format(Tr("You are about to uninstall {0}, are you sure you want to continue?"),gle.GodotVersion.Tag)
            );
        }

        while (!result.IsCompleted)
            await this.IdleFrame();

        if (result.Result) {
            foreach(ProjectFile pf in CentralStore.Projects) {
                if (pf.GodotVersion == gle.GodotVersion.Id) {
                    pf.GodotVersion = Guid.Empty.ToString();
                }
            }

            if (CentralStore.Settings.DefaultEngine == gle.GodotVersion.Id) {
                CentralStore.Settings.DefaultEngine = Guid.Empty.ToString();
            }

            CentralStore.Versions.Remove(gle.GodotVersion);
            CentralStore.Instance.SaveDatabase();

            if (gle.GodotVersion.GithubVersion != null || gle.GodotVersion.MirrorVersion != null || gle.GodotVersion.CustomEngine != null)
                installer.Uninstall();

            await PopulateList();
        } else
            gle.ToggleDownloadUninstall(true);
    }

    void OnDefaultSelected(GodotLineEntry gle) {
        if (gle.GodotVersion.Id == CentralStore.Settings.DefaultEngine)
            return; // Don't need to do anything
        else {
            foreach(GodotLineEntry igle in Installed.List.GetChildren()) {
                if (igle.IsDefault)
                    igle.ToggleDefault(false);
            }
            CentralStore.Settings.DefaultEngine = gle.GodotVersion.Id;
            CentralStore.Instance.SaveDatabase();
            gle.ToggleDefault(true);
        }
    }

    public async Task PopulateList() {
        foreach (Node child in Installed.List.GetChildren())
            child.QueueFree();
        foreach (Node child in Available.List.GetChildren())
            child.QueueFree();

        await this.IdleFrame();

        foreach (GodotVersion gdv in CentralStore.Versions) {
            GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
            gle.GodotVersion = gdv;
            gle.GithubVersion = gdv.GithubVersion;
            gle.MirrorVersion = gdv.MirrorVersion;
            gle.Mono = gdv.IsMono;
            gle.Downloaded = true;
            gle.ToggleDefault(CentralStore.Settings.DefaultEngine == gdv.Id);
            Installed.List.AddChild(gle);
            gle.Connect("uninstall_clicked", this, "OnUninstallClicked");
            gle.Connect("default_selected", this, "OnDefaultSelected");
            gle.Connect("right_clicked", this, "OnRightClicked_Installed");
        }
        
        // Handle CustomEngineDownload first, before official mirrors
        foreach (CustomEngineDownload ced in CentralStore.CustomEngines)
        {
            GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
            gle.CustomEngine = ced;
            Available.List.AddChild(gle);
            gle.Connect("install_clicked", this, "OnInstallClicked");
            gle.Connect("right_clicked", this, "OnRightClicked_Installable");
        }

        if (DownloadSource.Selected == 0) {
            // Handle Github
            foreach(GithubVersion gv in CentralStore.GHVersions) {
                GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
                gle.GithubVersion = gv;
                gle.Mono = IsMono();
                Available.List.AddChild(gle);
                gle.Connect("install_clicked", this, "OnInstallClicked");
                gle.Connect("right_clicked", this, "OnRightClicked_Installable");
            }
        } else {
            // Handle Mirror
            foreach(MirrorVersion mv in CentralStore.MRVersions[DownloadSource.GetSelectedId()].Reverse()) {
                GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
                gle.MirrorVersion = mv;
                gle.Mono = IsMono();
                Available.List.AddChild(gle);
                gle.Connect("install_clicked", this, "OnInstallClicked");
                gle.Connect("right_clicked", this, "OnRightClicked_Installable");
            }
        }

        UpdateVisibility();
    }

    void OnRightClicked_Installable(GodotLineEntry gle)
    {
        _enginePopup.GodotLineEntry = gle;
        _enginePopup.SetItemText(0,"Install");
        _enginePopup.SetItemDisabled(1,true);
        _enginePopup.SetItemDisabled(2,true);
        _enginePopup.SetItemDisabled(3, true);
        _enginePopup.Popup_(new Rect2(GetGlobalMousePosition(), _enginePopup.RectSize));
    }

    void OnRightClicked_Installed(GodotLineEntry gle)
    {
        _enginePopup.GodotLineEntry = gle;
        _enginePopup.SetItemText(0, "Uninstall");
        _enginePopup.SetItemDisabled(1, false);
        _enginePopup.SetItemDisabled(2, false);
        _enginePopup.SetItemDisabled(3, false);
        _enginePopup.Popup_(new Rect2(GetGlobalMousePosition(), _enginePopup.RectSize));
    }

    public void _IdPressed(int id)
    {
        switch (id)
        {
            case 0:
                if (_enginePopup.GodotLineEntry.Downloaded)
                {
                    // Uninstall
                    OnUninstallClicked(_enginePopup.GodotLineEntry);
                }
                else
                {
                    // Install
                    OnInstallClicked(_enginePopup.GodotLineEntry);
                }

                break;
            case 1:
                OnDefaultSelected(_enginePopup.GodotLineEntry);
                break;
            case 2:
                OS.Clipboard = _enginePopup.GodotLineEntry.GodotVersion.GetExecutablePath();
                OS.Alert("Location copied to Clipboard", "Copy Engine Location");
                break;
            case 3:
                OS.ShellOpen(_enginePopup.GodotLineEntry.GodotVersion.GetExecutablePath().GetBaseDir());
                break;
        }
    }

    private void UpdateVisibility() {
        Array<string> gdName = new Array<string>();
        foreach (GodotLineEntry igle in Installed.List.GetChildren()) {
            gdName.Add(igle.Label);
        }

        foreach(GodotLineEntry agle in Available.List.GetChildren()) {
            agle.Visible = false;
            // Is in Installed List?
            if (gdName.IndexOf(agle.Label) != -1)
                continue;

            // Does it not have MirrorVersion?
            if (agle.MirrorVersion == null) {
                agle.Visible = true;
                continue;
            }

            Array<string> tags = new Array<string>();

            if (IsAlpha()) tags.Add("alpha");
            if (IsBeta()) tags.Add("beta");
            if (IsRC()) tags.Add("rc");
            if (IsMono()) tags.Add("mono");

            agle.Visible = Enumerable.SequenceEqual(agle.MirrorVersion.Tags, tags);
        }
    }

    private int downloadedBytes = 0;
    void OnChunkReceived(int bytes) {
        downloadedBytes += bytes;
        AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Downloaded {0}..."),Util.FormatSize(downloadedBytes)));
    }

    public async Task GatherGithubReleases() {
        AppDialogs.BusyDialog.UpdateHeader(Tr("Fetching Releases from Github..."));
        AppDialogs.BusyDialog.UpdateByline(Tr("Connecting..."));
        AppDialogs.BusyDialog.ShowDialog();
        downloadedBytes = 0;
        Github.Github.Instance.Connect("chunk_received", this, "OnChunkReceived");
        var task = Github.Github.Instance.GetAllReleases();
        while(!task.IsCompleted) {
            await this.IdleFrame();
        }

        Github.Github.Instance.Disconnect("chunk_received", this, "OnChunkReceived");
        
        AppDialogs.BusyDialog.UpdateHeader(Tr("Processing Release Information from Github..."));
        AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"),0,task.Result.Count));
        int i = 0;
        foreach(Github.Release release in task.Result) {
            i++;
            AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"),i,task.Result.Count));
            GithubVersion gv = GithubVersion.FromAPI(release);
            CentralStore.GHVersions.Add(gv);
            await this.IdleFrame();
        }
        CentralStore.Instance.SaveDatabase();

        AppDialogs.BusyDialog.HideDialog();
    }

    public async Task GatherMirrorReleases() {
        int id = DownloadSource.GetSelectedId();
        MirrorSite mirror = CentralStore.Mirrors.Where(x => x.Id == id).FirstOrDefault<MirrorSite>();
        if (mirror == null)
            return;
        
        AppDialogs.BusyDialog.UpdateHeader(string.Format(Tr("Fetching Releases from {0}..."), mirror.Name));
        AppDialogs.BusyDialog.UpdateByline(Tr("Connecting..."));
        AppDialogs.BusyDialog.ShowDialog();
        downloadedBytes = 0;
        Mirrors.MirrorManager.Instance.Connect("chunk_received", this, "OnChunkReceived");
        var task = Mirrors.MirrorManager.Instance.GetEngineLinks(id);
        
        while(!task.IsCompleted)
            await this.IdleFrame();
        
        Mirrors.MirrorManager.Instance.Disconnect("chunk_received", this, "OnChunkReceived");

        AppDialogs.BusyDialog.UpdateHeader(string.Format(Tr("Processing Release Information from {0}..."), mirror.Name));
        AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"), 0, task.Result.Count));
        int i = 0;
        foreach (MirrorVersion version in task.Result) {
            i++;
            AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"), i, task.Result.Count));
            CentralStore.MRVersions[id].Add(version);
            await this.IdleFrame();
        }
        CentralStore.Instance.SaveDatabase();

        AppDialogs.BusyDialog.HideDialog();
    }

    public async Task GatherReleases() {
        if (DownloadSource.Selected == 0)
            await GatherGithubReleases();
        else
            await GatherMirrorReleases();
    }
}