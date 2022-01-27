using Godot;
using Godot.Collections;
using GodotSharpExtras;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;

public class GodotLineEntry : HBoxContainer
{
    [Signal]
    public delegate void install_clicked(GodotLineEntry entry);

    [Signal]
    public delegate void uninstall_clicked(GodotLineEntry entry);

    [Signal]
    public delegate void default_selected(GodotLineEntry entry);

#region Private Node Variables
    [NodePath("vc/VersionTag")]
    private Label _label = null;
    [NodePath("vc/hc/Source")]
    private Label _source = null;
    [NodePath("vc/hc/Filesize")]
    private Label _filesize = null;
    [NodePath("Download")]
    private TextureRect _download = null;
    [NodePath("Default")]
    private TextureRect _default = null;

    [NodePath("vc/DownloadProgress")]
    private HBoxContainer _downloadProgress = null;
    [NodePath("vc/DownloadProgress/ProgressBar")]
    private ProgressBar _progressBar = null;
    [NodePath("vc/DownloadProgress/Filesize")]
    private Label _fileSize = null;

    [NodePath("vc/ETA")]
    private HBoxContainer _eta = null;
    [NodePath("vc/ETA/EtaRemaining")]
    private Label _etaRemaining = null;
    [NodePath("vc/ETA/DownloadSpeed")]
    private Label _downloadSpeed = null;

    [NodePath("DownloadSpeedTimer")]
    private Timer _downloadSpeedTimer = null;

    private StreamTexture downloadIcon;
    private StreamTexture uninstallIcon;
#endregion

#region Private String Variables
    private string sLabel = "Godot Version x.x.x (Stable)";
    private string sSource = "Source: TuxFamily.org";
    private string sFilesize = "Size: 32MB";
    private bool bDownloaded = false;
    private bool bDefault = false;
    private bool bMono = false;
    private GodotVersion gvGodotVersion = null;
    private GithubVersion gvGithubVersion = null;
    private Downloader Downloader = null;

    private int iLastByteCount = 0;
    Array<double> adSpeedStack;
    System.DateTime dtStartTime;
#endregion

#region Public Accessors
    public GodotVersion GodotVersion {
        get {
            return gvGodotVersion;
        }

        set {
            gvGodotVersion = value;
            if (value != null) {
                Mono = value.IsMono;
                Label = value.Tag;
            }
        }
    }

    public bool Mono {
        get {
            return bMono;
        }

        set {
            bMono = value;
            GithubVersion = gvGithubVersion;
        }
    }

    public GithubVersion GithubVersion {
        get {
            return gvGithubVersion;
        }

        set {
            gvGithubVersion = value;
            if (value == null)
                return;
            Label = value.Name;
            switch(Platform.OperatingSystem) {
                case "Windows":
                case "UWP (Windows 10)":
                    if (Mono) {
                        if (Platform.Bits == "32") {
                            Source = value.Mono.Win32;
                            Filesize = Util.FormatSize(value.Mono.Win32_Size);
                        } else if (Platform.Bits == "64") {
                            Source = value.Mono.Win64;
                            Filesize = Util.FormatSize(value.Mono.Win64_Size);                    
                        }
                    } else {
                        if (Platform.Bits == "32") {
                            Source = value.Standard.Win32;
                            Filesize = Util.FormatSize(value.Standard.Win32_Size);
                        } else if (Platform.Bits == "64") {
                            Source = value.Standard.Win64;
                            Filesize = Util.FormatSize(value.Standard.Win64_Size);                    
                        }
                    }
                    break;

                case "Linux (or BSD)":
                    if (Mono) {
                        if (Platform.Bits == "32") {
                            Source = value.Mono.Linux32;
                            Filesize = Util.FormatSize(value.Mono.Linux32_Size);
                        } else if (Platform.Bits == "64") {
                            Source = value.Mono.Linux64;
                            Filesize = Util.FormatSize(value.Mono.Linux64_Size);
                        }
                    } else {
                        if (Platform.Bits == "32") {
                            Source = value.Standard.Linux32;
                            Filesize = Util.FormatSize(value.Standard.Linux32_Size);
                        } else if (Platform.Bits == "64") {
                            Source = value.Standard.Linux64;
                            Filesize = Util.FormatSize(value.Standard.Linux64_Size);
                        }
                    }
                    break;

                case "macOS":
                    if (Mono) {
                        Source = value.Mono.OSX;
                        Filesize = Util.FormatSize(value.Mono.OSX_Size);
                    } else {
                        Source = value.Standard.OSX;
                        Filesize = Util.FormatSize(value.Standard.OSX_Size);
                    }
                    break;
                
                default:
                    break;
            }
        }
    }

	public string Label {
        get {
            return sLabel;
        }
        set {
            sLabel = value + (Mono ? " - Mono" : "");
            if (_label != null)
                _label.Text = $"Godot {value + (Mono ? " - Mono" : "")}";
        }
    }

    public string Source {
        get {
            return sSource;
        }

        set {
            sSource = value;
            if (_source != null)
                _source.Text = $"Source: {value}";
        }
    }

    public string Filesize {
        get {
            return sFilesize;
        }
        set {
            sFilesize = value;
            if (_filesize != null)
                _filesize.Text = $"Size: {value}";
        }
    }

    [Export]
    public bool Downloaded {
        get {
            return bDownloaded;
        }

        set {
            bDownloaded = value;
            if (_download != null) {
                ToggleDownloadUninstall(bDownloaded);
                _default.Visible = value;
            }
        }
    }

    public bool IsDownloaded {
        get {
            return bDownloaded;
        }
    }

    public bool IsDefault {
        get {
            return bDefault;
        }
    }
#endregion


    public override void _Ready()
    {
        this.OnReady();

        GodotVersion = gvGodotVersion;
        GithubVersion = gvGithubVersion;

        _download.Connect("gui_input", this, "OnDownload_GuiInput");
        _default.Connect("gui_input", this, "OnDefault_GuiInput");
        _downloadSpeedTimer.Connect("timeout", this, "OnDownloadSpeedTimer_Timeout");
        downloadIcon = GD.Load<StreamTexture>("res://Assets/Icons/download.svg");
        uninstallIcon = GD.Load<StreamTexture>("res://Assets/Icons/uninstall.svg");
        Downloaded = bDownloaded;
        ToggleDefault(bDefault);
        adSpeedStack = new Array<double>();

    }

    public void ToggleDownloadUninstall(bool value) {
        if (value) {
            _download.Texture = uninstallIcon;
            _download.SelfModulate = new Color("fc9c9c");
        }
        else {
            _download.Texture = downloadIcon;
            _download.SelfModulate = new Color("7defa7");
        }
    }

    public void ToggleDownloadProgress(bool value) {
        _downloadProgress.Visible = value;
        _eta.Visible = value;
    }

    public void ToggleDefault(bool value) {
        bDefault = value;
        if (_default != null) {
            if (value) {
                _default.SelfModulate = new Color("ffd684");
            } else {
                _default.SelfModulate = new Color("ffffff");
            }
        }
    }

     void OnDownload_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb && iemb.Pressed && (ButtonList)iemb.ButtonIndex == ButtonList.Left) {
            if (_download.Texture == downloadIcon)
                EmitSignal("install_clicked", this);
            else
                EmitSignal("uninstall_clicked", this);
            ToggleDownloadUninstall((_download.Texture == downloadIcon));
        }
    }

    void OnDefault_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb && iemb.Pressed && (ButtonList)iemb.ButtonIndex == ButtonList.Left) {
            EmitSignal("default_selected", this);
        }
    }

    void OnDownloadSpeedTimer_Timeout() {
        Mutex mutex = new Mutex();
        mutex.Lock();
        var lbc = iLastByteCount;
        var tb = _progressBar.Value;
        var speed = tb - lbc;
        adSpeedStack.Add(speed);
        var avgSpeed = adSpeedStack.Sum() / adSpeedStack.Count;
        _downloadSpeed.Text = $"Speed: {Util.FormatSize(avgSpeed)}/s";
        System.TimeSpan elapsedTime = System.DateTime.Now - dtStartTime;
        if (tb == 0)
            return;
        System.TimeSpan estTime = System.TimeSpan.FromSeconds( (Downloader.totalSize - tb) / ((double)tb / elapsedTime.TotalSeconds));
        _etaRemaining.Text = "ETA: " + estTime.ToString("hh':'mm':'ss");
        iLastByteCount = (int)_progressBar.Value;
        mutex.Unlock();
    }

    void OnChunkReceived(int bytes) {
        _progressBar.Value += bytes;
        _fileSize.Text = $"{Util.FormatSize(_progressBar.Value)}/{Util.FormatSize(Downloader.totalSize)}";
    }

    public GodotVersion CreateGodotVersion() {
        GodotVersion gv = new GodotVersion();
        string gdFile = Mono ? new System.Uri(GithubVersion.PlatformMonoDownloadURL).AbsolutePath.GetFile() : new System.Uri(GithubVersion.PlatformDownloadURL).AbsolutePath.GetFile();
        gv.Id = System.Guid.NewGuid().ToString();
        gv.Tag = GithubVersion.Name;
        gv.Url = Mono ? GithubVersion.PlatformMonoDownloadURL : GithubVersion.PlatformDownloadURL;
#if GODOT_MACOS || GODOT_OSX
        gv.Location = $"user://versions/{GithubVersion.Name + (Mono ? "_mono" : "") }";
#else
        gv.Location = $"user://versions/{(Mono ? gdFile.ReplaceN(".zip","") : GithubVersion.Name)}";
#endif
        gv.CacheLocation = $"user://cache/Godot/{gdFile}";
        gv.DownloadedDate = System.DateTime.UtcNow;
        gv.GithubVersion = GithubVersion;
        gv.IsMono = Mono;

        Array<string> fileList = new Array<string>();
        using (ZipArchive za = ZipFile.OpenRead(ProjectSettings.GlobalizePath(gv.CacheLocation))) {
            foreach (ZipArchiveEntry zae in za.Entries) {
                fileList.Add(zae.Name);
            }
        }

#if GODOT_WINDOWS || GODOT_UWP

        foreach(string fname in fileList) {
            if (fname.EndsWith(".exe") && fname.StartsWith("Godot")) {
                gv.ExecutableName = fname;
                break;
            }
        }

#elif GODOT_LINUXBSD || GODOT_X11

        foreach(string fname in fileList) {
            if (System.Environment.Is64BitProcess) {
                if (fname.EndsWith(".64") && fname.StartsWith("Godot")) {
                    gv.ExecutableName = fname;
                    break;
                }
            } else {
                if (fname.EndsWith(".32") && fname.StartsWith("Godot")) {
                    gv.ExecutableName = fname;
                    break;
                }
            }
        }

        Util.Chmod(gv.GetExecutablePath(), 0755);

#elif GODOT_MACOS || GODOT_OSX

        gv.ExecutableName = "Godot";
        Util.Chmod(gv.GetExecutablePath(), 0755);

#endif


        GodotVersion = gv;
        return gv;
    }

    public async Task StartDownload() {
        Downloader = Downloader.DownloadGithub(GithubVersion,Mono);
        string outFile = $"user://cache/Godot/{Downloader.downloadUri.AbsolutePath.GetFile()}";

#if GODOT_MACOS || GODOT_OSX
        string instDir = $"user://versions/{GithubVersion.Name + (Mono ? "_mono" : "")}";
#else
        string instDir = $"user://versions/{(Mono ? "" : GithubVersion.Name)}";
#endif
        Downloader.Connect("chunk_received", this, "OnChunkReceived");
        _progressBar.MinValue = 0;
        _progressBar.MaxValue = Downloader.totalSize;
        _progressBar.Value = 0;
        _downloadSpeedTimer.Start();
        _fileSize.Text = $"{Util.FormatSize(0)}/{Util.FormatSize(Downloader.totalSize)}";
        dtStartTime = System.DateTime.Now;

        Task<bool> bres = Downloader.DownloadFile(outFile);
        while (!bres.IsCompleted)
            await this.IdleFrame();

        _downloadSpeedTimer.Stop();
        if (bres.Result) {
            ZipFile.ExtractToDirectory(ProjectSettings.GlobalizePath(outFile),
                                        ProjectSettings.GlobalizePath(instDir));
        }
    }
}
