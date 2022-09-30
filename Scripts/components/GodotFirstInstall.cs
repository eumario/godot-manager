using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;
using Godot.Sharp.Extras;
using Mirrors;

public class GodotFirstInstall : Panel
{
    [NodePath] private Button Refresh;
    [NodePath] private OptionButton TagSelection;
    [NodePath] private OptionButton MirrorSelect;
    [NodePath] private CategoryList CategoryList;

    [Resource("res://components/GodotLineEntry.tscn")] private PackedScene GodotLE;
    public override async void _Ready()
    {
        this.OnReady();

        if (CentralStore.Mirrors.Count == 0 || CentralStore.Settings.LastMirrorCheck <
            (DateTime.UtcNow - CentralStore.Settings.CheckInterval))
        {
            var res = MirrorManager.Instance.GetMirrors();
            while (!res.IsCompleted) await this.IdleFrame();

            foreach (MirrorSite site in res.Result)
            {
                var cres = from csite in CentralStore.Mirrors
                    where csite.Id == site.Id
                    select csite;
                if (cres.FirstOrDefault<MirrorSite>() == null)
                {
                    CentralStore.Mirrors.Add(site);
                    CentralStore.MRVersions[site.Id] = new Array<MirrorVersion>();
                    CentralStore.Settings.LastUpdateMirrorCheck[site.Id] = new UpdateCheck()
                    {
                        LastCheck = DateTime.UtcNow - TimeSpan.FromDays(1)
                    };
                }
            }
        }
        
        foreach(MirrorSite site in CentralStore.Mirrors)
            MirrorSelect.AddItem(site.Name, site.Id);

        TagSelection.GetPopup().HideOnCheckableItemSelection = false;
        TagSelection.GetPopup().Connect("id_pressed", this, "OnIdPressed_TagSelection");
        
        TagSelection.UpdateTr(0, Tr("Mono / C#"));
        TagSelection.UpdateTr(1, Tr("Release Type"));
        TagSelection.UpdateTr(2, Tr("Stable"));
        TagSelection.UpdateTr(3, Tr("Alpha"));
        TagSelection.UpdateTr(4, Tr("Beta"));
        TagSelection.UpdateTr(5, Tr("Release Candidate"));
    }

    async void OnIdPressed_TagSelection(int id)
    {
        if (id == 0)
            TagSelection.GetPopup().SetItemChecked(id, !IsMono());
        else
        {
            for (int i = 2; i < TagSelection.GetPopup().GetItemCount(); i++)
            {
                TagSelection.GetPopup().SetItemChecked(i, (i == id));
            }
        }

        await PopulateList();
    }

    bool IsMono()
    {
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

    void OnlyMono()
    {
        if (MirrorSelect.Selected == 0)
        {
            for (int i = 2; i < TagSelection.GetPopup().GetItemCount(); i++)
            {
                TagSelection.GetPopup().SetItemDisabled(i, true);
            }
        }
    }

    void AllTags()
    {
        if (MirrorSelect.Selected > 0)
        {
            for (int i = 2; i < TagSelection.GetPopup().GetItemCount(); i++)
            {
                TagSelection.GetPopup().SetItemDisabled(i, false);
            }
        }
    }

    [SignalHandler("item_selected", nameof(MirrorSelect))]
    async void OnItemSelected_MirrorSelect(int indx)
    {
        if (indx == 0)
        {
            OnlyMono();
            if (CentralStore.GHVersions.Count == 0)
            {
                var t = GatherReleases();
                while (!t.IsCompleted) await this.IdleFrame();
            }
            else
            {
                if (CentralStore.Settings.CheckForUpdates &&
                    (DateTime.UtcNow - CentralStore.Settings.LastCheck) >= CentralStore.Settings.CheckInterval)
                {
                    await CheckForUpdates();
                }
            }
        }
        else
        {
            AllTags();
            int id = MirrorSelect.GetSelectedId();
            if (CentralStore.MRVersions[id].Count == 0)
            {
                await GatherReleases();
            }
            else
            {
                if (CentralStore.Settings.CheckForUpdates &&
                    (DateTime.UtcNow - CentralStore.Settings.LastUpdateMirrorCheck[id].LastCheck) >=
                    CentralStore.Settings.CheckInterval)
                {
                    await CheckForUpdates();
                }
            }
        }

        if (CentralStore.Settings.UseLastMirror)
            CentralStore.Settings.LastEngineMirror = indx;
        await PopulateList();
    }

    async Task CheckForUpdates()
    {
        MirrorSite site = null;
        if (MirrorSelect.Selected == 0)
        {
            AppDialogs.BusyDialog.UpdateHeader(Tr("Grabbing information from Github"));
            AppDialogs.BusyDialog.UpdateByline(Tr("Getting the latest version information from Github for Godot Engine..."));
        }
        else
        {
            site = CentralStore.Mirrors[MirrorSelect.Selected - 1];
            AppDialogs.BusyDialog.UpdateHeader(string.Format(Tr("Grabbing information for mirror {0}"), site.Name));
            AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Grabbing latest version information for mirror {0} for Godot Engine..."), site.Name));
        }
        AppDialogs.BusyDialog.ShowDialog();
        if (MirrorSelect.Selected == 0)
            await CheckForGithub();
        else
            await CheckForMirror();

        if (MirrorSelect.Selected == 0)
            CentralStore.Settings.LastCheck = DateTime.UtcNow;
        else
            CentralStore.Settings.LastUpdateMirrorCheck[site.Id].LastCheck = DateTime.UtcNow;
        
        AppDialogs.BusyDialog.HideDialog();
        CentralStore.Instance.SaveDatabase();
    }

    private async Task CheckForMirror()
    {
        int id = MirrorSelect.GetSelectedId();
        CentralStore.MRVersions[id].Clear();
        var t = GatherMirrorReleases();
        while (!t.IsCompleted) await this.IdleFrame();
        await PopulateList();
    }

    private async Task CheckForGithub()
    {
        var tres = Github.Github.Instance.GetLatestRelease();
        while (!tres.IsCompleted) await this.IdleFrame();
        var gv = GithubVersion.FromAPI(tres.Result);
        var l = from version in CentralStore.GHVersions
            where version.Name == gv.Name
            select gv;
        var c = l.FirstOrDefault<GithubVersion>();
        if (c == null)
        {
            CentralStore.GHVersions.Clear();
            var t = GatherGithubReleases();
            while (!t.IsCompleted) await this.IdleFrame();
            await PopulateList();
        }
    }

    async Task PopulateList()
    {
        foreach (Node child in CategoryList.List.GetChildren())
            child.QueueFree();

        await this.IdleFrame();

        if (MirrorSelect.Selected == 0)
        {
            foreach (GithubVersion gv in CentralStore.GHVersions)
            {
                GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
                gle.GithubVersion = gv;
                gle.Mono = IsMono();
                CategoryList.List.AddChild(gle);
                gle.Connect("installed_clicked", this, "OnInstallClicked");
            }
        }
        else
        {
            foreach (MirrorVersion mv in CentralStore.MRVersions[MirrorSelect.GetSelectedId()].Reverse())
            {
                GodotLineEntry gle = GodotLE.Instance<GodotLineEntry>();
                gle.MirrorVersion = mv;
                gle.Mono = IsMono();
                CategoryList.List.AddChild(gle);
                gle.Connect("install_clicked", this, "OnInstallClicked");
            }
        }
    }

    private int downloadedBytes;
    
    void OnChunkReceived(int bytes) {
        downloadedBytes += bytes;
        AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Downloaded {0}..."),Util.FormatSize(downloadedBytes)));
    }
    
    public async Task GatherGithubReleases()
    {
        AppDialogs.BusyDialog.UpdateHeader(Tr("Fetching Releases from Github..."));
        AppDialogs.BusyDialog.UpdateByline(Tr("Connecting..."));
        AppDialogs.BusyDialog.ShowDialog();
        downloadedBytes = 0;
        Github.Github.Instance.Connect("chunk_received", this, "OnChunkReceived");
        var task = Github.Github.Instance.GetAllReleases();
        while (!task.IsCompleted) await this.IdleFrame();
        
        Github.Github.Instance.Disconnect("chunk_received", this, "OnChunkReceived");
        
        AppDialogs.BusyDialog.UpdateHeader(Tr("Processing Release Information from Github..."));
        AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"), 0, task.Result.Count));
        int i = 0;
        foreach (Github.Release release in task.Result)
        {
            i++;
            AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"), i, task.Result.Count));
            GithubVersion gv = GithubVersion.FromAPI(release);
            CentralStore.GHVersions.Add(gv);
            await this.IdleFrame();
        }
        CentralStore.Instance.SaveDatabase();
        
        AppDialogs.BusyDialog.HideDialog();
    }

    public async Task GatherMirrorReleases()
    {
        int id = MirrorSelect.GetSelectedId();
        MirrorSite mirror = CentralStore.Mirrors.FirstOrDefault(x => x.Id == id);
        if (mirror == null)
            return;
        
        AppDialogs.BusyDialog.UpdateHeader(string.Format(Tr("Fetching Releases from {0}..."), mirror.Name));
        AppDialogs.BusyDialog.UpdateByline(Tr("Connecting..."));
        AppDialogs.BusyDialog.ShowDialog();
        downloadedBytes = 0;
        Mirrors.MirrorManager.Instance.Connect("chunk_received", this, "OnChunkReceived");
        var task = Mirrors.MirrorManager.Instance.GetEngineLinks(id);

        while (!task.IsCompleted) await this.IdleFrame();
        
        Mirrors.MirrorManager.Instance.Disconnect("chunk_received", this, "OnChunkReceived");
        
        AppDialogs.BusyDialog.UpdateHeader(string.Format(Tr("Processing Release Information from {0}"), mirror.Name));
        AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"), 0, task.Result.Count));
        int i = 0;
        foreach (MirrorVersion version in task.Result)
        {
            i++;
            AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing {0}/{1}"), i, task.Result.Count));
            CentralStore.MRVersions[id].Add(version);
            await this.IdleFrame();
        }
        CentralStore.Instance.SaveDatabase();
        
        AppDialogs.BusyDialog.HideDialog();
    }

    public async Task GatherReleases()
    {
        if (MirrorSelect.Selected == 0)
            await GatherGithubReleases();
        else
            await GatherMirrorReleases();
    }
}
