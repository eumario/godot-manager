using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using System.Threading.Tasks;

public class AssetLibEntry : ColorRect
{
#region Node Paths
    [NodePath("hc/Icon")]
    TextureRect _icon = null;

    [NodePath("hc/vc/Title")]
    Label _title = null;

    [NodePath("hc/vc/hc/Category")]
    Label _category = null;

    [NodePath("hc/vc/hc/License")]
    Label _license = null;

    [NodePath("hc/vc/Author")]
    Label _author = null;

    [NodePath("hc2/Downloaded")]
    TextureRect _downloaded = null;

    [NodePath("hc2/UpdateAvailable")]
    TextureRect _updateAvailable = null;
#endregion

#region Private Variables
    Texture tIcon;
    string sTitle;
    string sCategory;
    string sLicense;
    string sAuthor;
    bool bDownloaded;
    bool bUpdateAvailable;
#endregion

#region Public Accessors
    public Texture Icon {
        get { return (_icon != null ? _icon.Texture : tIcon); }
        set {
            tIcon = value;
            if (_icon != null)
                _icon.Texture = value;
        }
    }

    public string Title {
        get { return (_title != null ? _title.Text : sTitle); }
        set {
            sTitle = value;
            if (_title != null)
                _title.Text = value;
        }
    }

    public string Category {
        get { return (_category != null ? _category.Text : sCategory); }
        set {
            sCategory = value;
            if (_category != null)
                _category.Text = Tr("Category: ") + value;
        }
    }

    public string License {
        get { return (_license != null ? _license.Text : sLicense);}
        set {
            sLicense = value;
            if (_license != null)
                _license.Text = Tr("License: ") + value;
        }
    }

    public string Author {
        get { return (_author != null ? _author.Text : sAuthor); }
        set {
            sAuthor = value;
            if (_author != null)
                _author.Text = Tr("Author: ") + value;
        }
    }

    public string AssetId { get; set; }

    public bool Downloaded {
        get => (_downloaded != null ? _downloaded.Visible : bDownloaded);
        set {
            bDownloaded = value;
            if (_downloaded != null)
                _downloaded.Visible = value;
        }
    }

    public bool UpdateAvailable {
        get => (_updateAvailable != null ? _updateAvailable.Visible : bUpdateAvailable);
        set {
            bUpdateAvailable = value;
            if (_updateAvailable != null)
                _updateAvailable.Visible = value;
        }
    }
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        Icon = tIcon;
        Title = sTitle;
        Category = sCategory;
        License = sLicense;
        Author = sAuthor;
        Downloaded = bDownloaded;
        UpdateAvailable = bUpdateAvailable;
    }

    [SignalHandler("mouse_entered")]
    void OnMouseEntered() {
        Color = new Color("2a2e37");
    }

    [SignalHandler("mouse_exited")]
    void OnMouseExited() {
        Color = new Color("002a2e37");
    }

    [SignalHandler("gui_input")]
    async void OnGuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iembEvent) {
            if (iembEvent.Pressed && (ButtonList)iembEvent.ButtonIndex == ButtonList.Left)
            {
                AppDialogs.BusyDialog.UpdateHeader(Tr("Getting asset information..."));
                AppDialogs.BusyDialog.UpdateByline(Tr("Connecting..."));
                AppDialogs.BusyDialog.ShowDialog();
                Task<AssetLib.Asset> asset = AssetLib.AssetLib.Instance.GetAsset(AssetId);
                while (!asset.IsCompleted)
                    await this.IdleFrame();
                AppDialogs.BusyDialog.Visible = false;
                AssetId = asset.Result.AssetId;
                AppDialogs.AssetLibPreview.ShowDialog(asset.Result);
                AppDialogs.AssetLibPreview.Connect("installed_addon", this, nameof(OnInstalledAddon));
                AppDialogs.AssetLibPreview.Connect("preview_closed", this, nameof(OnPreviewClosed));
                AppDialogs.AssetLibPreview.Connect("uninstalled_addon", this, nameof(OnUninstallAddon));
            }
        }
    }

    async void OnInstalledAddon(bool update) {
        Downloaded = true;
        if (update) {
            UpdateAvailable = false;
            Array<ProjectFile> updateList = new Array<ProjectFile>();
            foreach(ProjectFile pf in CentralStore.Projects) {
                if (pf.Assets == null)
                    continue;
                
                if (pf.Assets.Contains(AssetId)) {
                    updateList.Add(pf);
                }
            }

            if (updateList.Count > 0) {
                bool res = await AppDialogs.YesNoDialog.ShowDialog(Tr("Update Plugins"), string.Format(Tr("Found %d project(s) that currently reference this addon, do you wish to update them?"),updateList.Count));
                if (!res)
                    return;
                
                AppDialogs.BusyDialog.UpdateHeader(Tr("Updating Projects..."));
                AppDialogs.BusyDialog.UpdateByline(Tr("Processing..."));
                AppDialogs.BusyDialog.ShowDialog();

                foreach(ProjectFile pf in updateList) {
                    AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Updating Project %s..."),pf.Name));
                    PluginInstaller installer = new PluginInstaller(CentralStore.Instance.GetPluginId(AssetId));
                    installer.Uninstall(pf.Location.GetBaseDir().NormalizePath(),false);
                    installer.Install(pf.Location.GetBaseDir().NormalizePath());
                }
                AppDialogs.BusyDialog.UpdateByline(Tr("Completed."));
                AppDialogs.BusyDialog.HideDialog();
            }
        }
    }

    void OnUninstallAddon() {
        Downloaded = false;
        UpdateAvailable = false;
    }

    void OnPreviewClosed() {
        AppDialogs.AssetLibPreview.Disconnect("installed_addon", this, nameof(OnInstalledAddon));
        AppDialogs.AssetLibPreview.Disconnect("preview_closed", this, nameof(OnPreviewClosed));
        AppDialogs.AssetLibPreview.Disconnect("uninstalled_addon", this, nameof(OnUninstallAddon));
    }
}
