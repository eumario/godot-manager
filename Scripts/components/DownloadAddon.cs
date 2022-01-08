using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotSharpExtras;

public class DownloadAddon : ReferenceRect
{
#region Signals
    [Signal]
    public delegate void download_complete(AssetLib.Asset asset);
#endregion

#region Node Paths
    [NodePath("PC/CC/P/VB/MC/TitleBarBG/HB/Title")]
    Label _Title = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/FileName")]
    Label _FileName = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/Location")]
    Label _Location = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/FSLabel")]
    Label _FSLabel = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/FileSize")]
    Label _FileSize = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/Speed")]
    Label _Speed = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/ETALabel")]
    Label _ETALabel = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/Eta")]
    Label _Eta = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/ProgressBar")]
    ProgressBar _ProgressBar = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")]
    Button _CancelButton = null;

    [NodePath("DownloadSpeedTimer")]
    Timer _DownloadSpeedTimer = null;
#endregion

#region Private Variables
    int iTotalBytes;
    int iLastByteCount;
    int iFileSize;
    System.DateTime dtStartTime;
    double dSpeed;
    GDCSHTTPClient client;
    System.Uri dlUri;
#endregion

#region Public Accessors
    public AssetLib.Asset Asset;
#endregion

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        iTotalBytes = 0;
        iLastByteCount = 0;
        iFileSize = 0;
        dSpeed = 0.0f;
        client = new GDCSHTTPClient();
        client.Connect("chunk_received", this, "OnChunkReceived");
        _DownloadSpeedTimer.Connect("timeout", this, "OnDownloadSpeedTimer_Timeout");
        _CancelButton.Connect("pressed", this, "OnCancelPressed");
    }

    void OnCancelPressed() {
        client.Cancel();
    }

    void OnChunkReceived(int bytes) {
        iTotalBytes += bytes;
    }

    void OnDownloadSpeedTimer_Timeout() {
        Mutex mutex = new Mutex();
        mutex.Lock();
        var lbc = iLastByteCount;
        var tb = iTotalBytes;
        var speed = tb - lbc;
        _Speed.Text = Util.FormatSize(speed) + "/s";
        if (iFileSize <= 0) {
            _FileSize.Text = Util.FormatSize(tb);
            System.TimeSpan elapsedTime = System.DateTime.Now - dtStartTime;
            _Eta.Text = elapsedTime.ToString("hh':'mm':'ss");
        } else {
            _FileSize.Text = Util.FormatSize(tb) + "/" + Util.FormatSize(iFileSize);
            System.TimeSpan elapsedTime = System.DateTime.Now - dtStartTime;
            if (tb == 0)
                return;
            System.TimeSpan estTime = System.TimeSpan.FromSeconds( (iFileSize - tb) / ((double)tb / elapsedTime.TotalSeconds));
            _Eta.Text = estTime.ToString("hh':'mm':'ss");
        }
        iLastByteCount = iTotalBytes;
        mutex.Unlock();
    }

    public async Task<bool> StartNetwork() {
        Task<HTTPClient.Status> cres = client.StartClient(dlUri.Host, (dlUri.Scheme == "https"));

        while (!cres.IsCompleted)
            await this.IdleFrame();
        
        if (!client.SuccessConnect(cres.Result)) {
            Visible = false;
            return false;
        }
        
        // When using MakeRequest(), it will get the body before we get the headers back from the
        // function call.  We need to see about using HEAD to get the filesize, before the actual
        // request, this will also allow us to use HEAD to get redirect, without a body, I believe.
        var tresult = client.HeadRequest(dlUri.PathAndQuery);
        while (!tresult.IsCompleted)
            await this.IdleFrame();
        client.Close();

        HTTPResponse result = tresult.Result;
        Array<int> redirect_codes = new Array<int> { 301, 302, 303, 307, 308 };
        
        if (redirect_codes.IndexOf(result.ResponseCode) >= 0) {
            dlUri = new System.Uri(result.Headers["Location"] as string);
            Task<bool> recurse = StartNetwork();
            await recurse;
            return recurse.Result;
        }

        if (result.ResponseCode != 200) {
            Visible = false;
            return false;
        }

        if (result.Headers.Contains("Content-Length")) {
            if (int.TryParse(result.Headers["Content-Length"] as string, out iFileSize)) {
                _FileSize.Text = Util.FormatSize(iFileSize);
                _ProgressBar.MaxValue = iFileSize;
                _ProgressBar.Value = 0;
            } else {
                iFileSize = -1;
                _FSLabel.Text = "Downloaded:";
                _FileSize.Text = "0 bytes";
                _ETALabel.Text = "Elapsed:";
                _Eta.Text = "00:00:00";
                _ProgressBar.Value = 0;
                _ProgressBar.MaxValue = 100;

            }
        } else {
            iFileSize = -1;
            _ProgressBar.Value = 0;
            _ProgressBar.MaxValue = 100;
            _FSLabel.Text = "Downloaded:";
            _FileSize.Text = "0 bytes";
            _ETALabel.Text = "Elapsed:";
            _Eta.Text = "00:00:00";
        }

        cres = client.StartClient(dlUri.Host, (dlUri.Scheme == "https"));
        while (!cres.IsCompleted)
            await this.IdleFrame();
        
        if (!client.SuccessConnect(cres.Result)) {
            Visible = false;
            return false;
        }

        // Begin Actual Network download of addon/project/demo....
        dtStartTime = System.DateTime.Now;
        _DownloadSpeedTimer.Start(1);
        tresult = client.MakeRequest(dlUri.PathAndQuery);

        while (!tresult.IsCompleted) {
            await this.IdleFrame();
            if (tresult.IsCanceled)
                break;
        }

        
        client.Close();
        _DownloadSpeedTimer.Stop();
        if (tresult.IsCanceled) {
            Visible = false;
            return false;
        }

        result = tresult.Result;

        if (result.Cancelled) {
            AppDialogs.MessageDialog.ShowMessage("Download Cancelled",$"Addon download '{Asset.Title}' cancelled.");
            Visible = false;
            return false;
        }
            

        string sPath = $"user://cache/AssetLib/{Asset.AssetId}-{dlUri.AbsolutePath.GetFile()}";
        if (!sPath.EndsWith(".zip"))
            sPath += ".zip";
        
        GD.Print($"AbsPath: {dlUri.AbsolutePath}");
        GD.Print($"FileName: {sPath}");
        GD.Print($"Buffer Size: {result.BodyRaw.Length}");

        File fh = new File();
        Error err = fh.Open(sPath, File.ModeFlags.Write);
        if (err == Error.Ok) {
            fh.StoreBuffer(result.BodyRaw);
            fh.Close();
        } else {
            GD.Print($"Failed to open file {sPath}, Error: {err}");
            Visible = false;
            return false;
        }

        Visible = false;
        return true;
    }

    public void LoadInformation() {
        _Title.Text = $"Downloading {Asset.Type.Capitalize()}";
        _FileName.Text = Asset.Title;
        _Location.Text = Asset.DownloadProvider;
        _FSLabel.Text = "File Size:";
        _FileSize.Text = "Unknown";
        _Speed.Text = "Unknown";
        _ETALabel.Text = "ETA:";
        _Eta.Text = "Unknown";
        _ProgressBar.Value = 0;
        _ProgressBar.MaxValue = 100;
        iTotalBytes = 0;
        iLastByteCount = 0;
        iFileSize = 0;
        dSpeed = 0.0f;
    }

    public async Task StartDownload() {
        if (Asset == null)
            return;
        Visible = true;
        LoadInformation();
        dlUri = new System.Uri(Asset.DownloadUrl);
        await StartNetwork();
    }
}
