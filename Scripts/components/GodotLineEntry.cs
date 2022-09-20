using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Linq;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using Uri = System.Uri;
using Guid = System.Guid;
using Environment = System.Environment;

public class GodotLineEntry : HBoxContainer
{
    [Signal]
    public delegate void install_clicked(GodotLineEntry entry);

    [Signal]
    public delegate void uninstall_clicked(GodotLineEntry entry);

    [Signal]
    public delegate void default_selected(GodotLineEntry entry);

    [Signal]
    public delegate void right_clicked(GodotLineEntry entry);

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

    [NodePath("vc/Location")] private HBoxContainer _loc = null;

    [NodePath("vc/Location/DownloadLocation")]
    private Label _downloadLocation = null;

    [NodePath("DownloadSpeedTimer")]
    private Timer _downloadSpeedTimer = null;

    private StreamTexture downloadIcon;
    private StreamTexture uninstallIcon;
#endregion

#region Private String Variables
    private string sLabel = "Godot Version x.x.x (Stable)";
    private string sSource = "Source: TuxFamily.org";
    private string sFilesize = "Size: 32MB";
    private string sLocation = @"E:\Apps\GodotManager\versions\TestLocation";
    private bool bDownloaded = false;
    private bool bDefault = false;
    private bool bMono = false;
    private GodotVersion gvGodotVersion = null;
    private GithubVersion gvGithubVersion = null;
    private MirrorVersion gvMirrorVersion = null;
    private CustomEngineDownload gvCustomEngine = null;

    private int iLastByteCount = 0;
    Array<double> adSpeedStack;
    DateTime dtStartTime;
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
                Source = value.Url;
                Location = value.GetExecutablePath();
                if (value.GithubVersion != null)
                    Filesize = Util.FormatSize(value.GithubVersion.PlatformDownloadSize);
                if (value.MirrorVersion != null)
                    Filesize = Util.FormatSize(value.MirrorVersion.PlatformDownloadSize);
                if (value.CustomEngine != null)
                    Filesize = Util.FormatSize(value.CustomEngine.DownloadSize);
                if (_loc != null)
                    _loc.Visible = true;
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
            MirrorVersion = gvMirrorVersion;
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
            if (_loc != null)
                _loc.Visible = false;
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

    public MirrorVersion MirrorVersion {
        get {
            return gvMirrorVersion;
        }

        set {
            gvMirrorVersion = value;
            if (value == null)
                return;
            Label = value.Version;
            if (_loc != null)
                _loc.Visible = false;
            Source = value.PlatformDownloadURL;
            switch(Platform.OperatingSystem) {
                case "Windows":
                case "UWP (Windows 10)":
                    if (Platform.Bits == "32")
                    {
                        Filesize = Util.FormatSize(value.Win32_Size);
                    } else {
                        Filesize = Util.FormatSize(value.Win64_Size);
                    }
                    break;
                case "Linux (or BSD)":
                    if (Platform.Bits == "32") {
                        Filesize = Util.FormatSize(value.Linux32_Size);
                    } else {
                        Filesize = Util.FormatSize(value.Linux64_Size);
                    }
                    break;
                case "macOS":
                    Filesize = Util.FormatSize(value.OSX64_Size);
                    break;
            }
        }
    }

    public CustomEngineDownload CustomEngine
    {
        get
        {
            return gvCustomEngine;
        }
        set
        {
            gvCustomEngine = value;
            if (value == null)
                return;
            Label = value.TagName;
            Source = value.Url;
            Filesize = value.DownloadSize == 0 ? "Unknown" : Util.FormatSize(value.DownloadSize);
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
                _source.Text = string.Format(Tr("Source: {0}"),value);
        }
    }

    public string Filesize {
        get {
            return sFilesize;
        }
        set {
            sFilesize = value;
            if (_filesize != null)
                _filesize.Text = string.Format(Tr("Size: {0}"),value);
        }
    }

    public string Location
    {
        get
        {
            return sLocation;
        }
        set
        {
            sLocation = value;
            if (_downloadLocation != null)
                _downloadLocation.Text = sLocation.GetBaseDir();
            if (_loc != null)
                _loc.Visible = true;
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

    public int TotalSize { get; set; }
#endregion


    public override void _Ready()
    {
        this.OnReady();

        GithubVersion = gvGithubVersion;
        MirrorVersion = gvMirrorVersion;
        CustomEngine = gvCustomEngine;
        GodotVersion = gvGodotVersion;

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

    public void StartDownloadStats(int totalSize) {
        dtStartTime = DateTime.Now;
        TotalSize = totalSize;
        _progressBar.MinValue = 0;
        _progressBar.MaxValue = totalSize;
        _downloadSpeedTimer.Start();
    }

    public void StopDownloadStats() {
        _downloadSpeedTimer.Stop();
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

    [SignalHandler("gui_input")]
    void OnGuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton iemb && iemb.Pressed &&
            (ButtonList)iemb.ButtonIndex == ButtonList.Right)
        {
            EmitSignal("right_clicked", this);
        }
    }

    [SignalHandler("gui_input", nameof(_download))]
    void OnDownload_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb && iemb.Pressed && (ButtonList)iemb.ButtonIndex == ButtonList.Left) {
            if (_download.Texture == downloadIcon)
                EmitSignal("install_clicked", this);
            else
                EmitSignal("uninstall_clicked", this);
            ToggleDownloadUninstall((_download.Texture == downloadIcon));
        }
    }

    [SignalHandler("gui_input", nameof(_default))]
    void OnDefault_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb && iemb.Pressed && (ButtonList)iemb.ButtonIndex == ButtonList.Left) {
            EmitSignal("default_selected", this);
        }
    }

    [SignalHandler("timeout", nameof(_downloadSpeedTimer))]
    void OnDownloadSpeedTimer_Timeout() {
        Mutex mutex = new Mutex();
        mutex.Lock();
        var lbc = iLastByteCount;
        var tb = _progressBar.Value;
        var speed = tb - lbc;
        adSpeedStack.Add(speed);
        var avgSpeed = adSpeedStack.Sum() / adSpeedStack.Count;
        _downloadSpeed.Text = string.Format(Tr("Speed: {0}/s"),Util.FormatSize(avgSpeed));
        TimeSpan elapsedTime = DateTime.Now - dtStartTime;
        if (tb == 0)
            return;
        TimeSpan estTime = TimeSpan.FromSeconds( (TotalSize - tb) / ((double)tb / elapsedTime.TotalSeconds));
        _etaRemaining.Text = Tr("ETA: ") + estTime.ToString("hh':'mm':'ss");
        iLastByteCount = (int)_progressBar.Value;
        mutex.Unlock();
    }

    public void OnChunkReceived(int bytes) {
        _progressBar.Value += bytes;
        _fileSize.Text = $"{Util.FormatSize(_progressBar.Value)}/{Util.FormatSize(TotalSize)}";
    }
}
