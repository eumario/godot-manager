using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System.Linq;
using Uri = System.Uri;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

public class DownloadAddon : ReferenceRect
{
#region Signals
    [Signal]
    public delegate void download_complete(AssetLib.Asset asset, AssetProject ap, AssetPlugin apl);
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

    [NodePath("IndeterminateProgress")]
    Tween _IndeterminateProgress = null;
#endregion

#region Private Variables
    int iTotalBytes;
    int iLastByteCount;
    int iFileSize;
    DateTime dtStartTime;
    GDCSHTTPClient client;
    Uri dlUri;
    bool bDownloading = false;
    Array<double> adSpeedStack;

    private Array<string> Templates = new Array<string> {"Templates", "Projects", "Demos"};
#endregion

#region Public Accessors
    public AssetLib.Asset Asset;
#endregion

    public override void _Ready()
    {
        this.OnReady();
        iTotalBytes = 0;
        iLastByteCount = 0;
        iFileSize = 0;
        adSpeedStack = new Array<double>();
    }

    [SignalHandler("pressed", nameof(_CancelButton))]
    void OnCancelPressed() {
        client.Cancel();
    }

    
    void OnChunkReceived(int bytes) {
        iTotalBytes += bytes;
        if (iFileSize >= 0) {
            _ProgressBar.Value = iTotalBytes;
            _ProgressBar.Update();
        }
    }

    [SignalHandler("timeout", nameof(_DownloadSpeedTimer))]
    void OnDownloadSpeedTimer_Timeout() {
        Mutex mutex = new Mutex();
        mutex.Lock();
        var lbc = iLastByteCount;
        var tb = iTotalBytes;
        var speed = tb - lbc;
        adSpeedStack.Add(speed);
        var avgSpeed = adSpeedStack.Sum() / adSpeedStack.Count;
        _Speed.Text = $"{Util.FormatSize(avgSpeed)}/s";
        if (iFileSize <= 0) {
            _FileSize.Text = Util.FormatSize(tb);
            TimeSpan elapsedTime = DateTime.Now - dtStartTime;
            _Eta.Text = elapsedTime.ToString("hh':'mm':'ss");
        } else {
            _FileSize.Text = Util.FormatSize(tb) + "/" + Util.FormatSize(iFileSize);
            TimeSpan elapsedTime = DateTime.Now - dtStartTime;
            if (tb == 0)
                return;
            TimeSpan estTime = TimeSpan.FromSeconds( (iFileSize - tb) / ((double)tb / elapsedTime.TotalSeconds));
            _Eta.Text = estTime.ToString("hh':'mm':'ss");
        }
        iLastByteCount = iTotalBytes;
        mutex.Unlock();
    }

    async Task StartIndeterminateTween() {
        _ProgressBar.RectRotation = 0;
        _ProgressBar.RectPivotOffset = new Vector2(_ProgressBar.RectSize.x/2,_ProgressBar.RectSize.y/2);
        _ProgressBar.Value = 0;
        _ProgressBar.PercentVisible = false;
        while (bDownloading) {
            _IndeterminateProgress.InterpolateProperty(_ProgressBar, "value", 0, 100, 0.5f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
            _IndeterminateProgress.Start();
            while (_IndeterminateProgress.IsActive() && bDownloading)
                await this.IdleFrame();
            _ProgressBar.RectRotation = 180;
            _IndeterminateProgress.InterpolateProperty(_ProgressBar, "value", 100, 0, 0.5f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
            _IndeterminateProgress.Start();
            while (_IndeterminateProgress.IsActive() && bDownloading)
                await this.IdleFrame();
            _ProgressBar.RectRotation = 0;
        }
        if (_IndeterminateProgress.IsActive())
            _IndeterminateProgress.StopAll();
        _ProgressBar.RectRotation = 0;
        _ProgressBar.Value = 0;
        _ProgressBar.PercentVisible = true;
    }

    public async Task<bool> StartNetwork()
	{
		bDownloading = true;
        adSpeedStack.Clear();
		InitClient();
        if (CentralStore.Settings.UseProxy)
            client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, dlUri.Scheme == "https");
        else
            client.ClearProxy();

		Task<HTTPClient.Status> cres = client.StartClient(dlUri.Host, dlUri.Port, (dlUri.Scheme == "https"));

		while (!cres.IsCompleted)
			await this.IdleFrame();

		if (!client.SuccessConnect(cres.Result, true))
		{
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

		if (redirect_codes.IndexOf(result.ResponseCode) >= 0)
		{
			dlUri = new Uri(result.Headers["Location"] as string);
            CleanupClient();
			Task<bool> recurse = StartNetwork();
			await recurse;
			return recurse.Result;
		}

		if (result.ResponseCode != 200)
		{
            CleanupClient();
			Visible = false;
			return false;
		}

		UpdateFields(result);

        if (CentralStore.Settings.UseProxy)
            client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, dlUri.Scheme == "https");
        else
            client.ClearProxy();
		cres = client.StartClient(dlUri.Host, dlUri.Port, (dlUri.Scheme == "https"));
		while (!cres.IsCompleted)
			await this.IdleFrame();

		if (!client.SuccessConnect(cres.Result))
		{
			CleanupClient();
			Visible = false;
			return false;
		}

		// Begin Actual Network download of addon/project/demo....
		dtStartTime = DateTime.Now;
		_DownloadSpeedTimer.Start(1);
		tresult = client.MakeRequest(dlUri.PathAndQuery, true);

		while (!tresult.IsCompleted)
		{
			await this.IdleFrame();
			if (tresult.IsCanceled)
				break;
		}


		client.Close();
		bDownloading = false;
		if (tresult.IsCanceled)
		{
			CleanupClient();
			Visible = false;
			return false;
		}

		result = tresult.Result;

		if (result.Cancelled)
		{
			AppDialogs.MessageDialog.ShowMessage(Tr("Download Cancelled"),
                string.Format(Tr($"Addon download '{0}' cancelled."),Asset.Title));
			CleanupClient();
			Visible = false;
			return false;
		}


		string sPath = $"{CentralStore.Settings.CachePath}/AssetLib/{Asset.AssetId}-{dlUri.AbsolutePath.GetFile()}";
		if (!sPath.EndsWith(".zip"))
			sPath += ".zip";
        
        File fh = new File();
        Error err = fh.Open(sPath, File.ModeFlags.Write);
        if (err != Error.Ok) {
            GD.Print($"Failed to open file {sPath}, Error: {err}");
            return false;
        }
        
        fh.StoreBuffer(result.BodyRaw);
        fh.Close();

		Visible = false;
		CleanupClient();

        AssetProject ap = null;
        AssetPlugin apl = null;
        
        if (Templates.IndexOf(Asset.Category) != -1) {
            ap = new AssetProject();
            ap.Asset = Asset;
            ap.Location = sPath;
            if (CentralStore.Instance.HasTemplateId(Asset.AssetId)) {
                CentralStore.Templates.Remove(CentralStore.Instance.GetTemplateId(Asset.AssetId));
            }
            CentralStore.Templates.Add(ap);
        } else {
            apl = new AssetPlugin();
            apl.Asset = Asset;
            apl.Location = sPath;
            if (CentralStore.Instance.HasPluginId(Asset.AssetId)) {
                CentralStore.Plugins.Remove(CentralStore.Instance.GetPluginId(Asset.AssetId));
            }
            CentralStore.Plugins.Add(apl);
        }
        CentralStore.Instance.SaveDatabase();

        EmitSignal("download_complete", Asset, ap, apl);

		return true;
	}

	private void UpdateFields(HTTPResponse result)
	{
		if (result.Headers.Contains("Content-Length"))
		{
			if (int.TryParse(result.Headers["Content-Length"] as string, out iFileSize))
			{
				_FileSize.Text = Util.FormatSize(iFileSize);
				_ProgressBar.MaxValue = iFileSize;
				_ProgressBar.Value = 0;
			}
			else
			{
				iFileSize = -1;
				_FSLabel.Text = Tr("Downloaded:");
				_FileSize.Text = "0 bytes";
				_ETALabel.Text = Tr("Elapsed:");
				_Eta.Text = "00:00:00";
				_ProgressBar.Value = 0;
				_ProgressBar.MaxValue = 100;
				Task task = StartIndeterminateTween();

			}
		}
		else
		{
			iFileSize = -1;
			_ProgressBar.Value = 0;
			_ProgressBar.MaxValue = 100;
			_FSLabel.Text = Tr("Downloaded:");
			_FileSize.Text = "0 bytes";
			_ETALabel.Text = Tr("Elapsed:");
			_Eta.Text = "00:00:00";
			Task task = StartIndeterminateTween();
		}
	}

	private void InitClient()
	{
        if (client != null)
            CleanupClient();
		client = new GDCSHTTPClient();
		client.Connect("chunk_received", this, "OnChunkReceived");
	}

	private void CleanupClient()
	{
		client.Disconnect("chunk_received", this, "OnChunkReceived");
		client.QueueFree();
		client = null;
	}

	public void LoadInformation() {
        _Title.Text = string.Format(Tr("Downloading {0}"),Asset.Type.Capitalize());
        _FileName.Text = Asset.Title;
        _Location.Text = Asset.DownloadProvider;
        _FSLabel.Text = Tr("File Size:");
        _FileSize.Text = Tr("Unknown");
        _Speed.Text = Tr("Unknown");
        _ETALabel.Text = Tr("ETA:");
        _Eta.Text = Tr("Unknown");
        _ProgressBar.Value = 0;
        _ProgressBar.MaxValue = 100;
        iTotalBytes = 0;
        iLastByteCount = 0;
        iFileSize = 0;
    }

    public async Task StartDownload() {
        if (Asset == null)
            return;
        Visible = true;
        LoadInformation();
        dlUri = new Uri(Asset.DownloadUrl);
        await StartNetwork();
    }
}
