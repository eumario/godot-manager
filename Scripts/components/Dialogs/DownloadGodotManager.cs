using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System.Linq;
using Uri = System.Uri;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using Dir = System.IO.Directory;
using System.Threading.Tasks;


public class DownloadGodotManager : ReferenceRect
{
	#region Signals
	[Signal]
	public delegate void download_complete(Github.Release release, Github.Asset asset);
	#endregion

	#region Node Paths
	[NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/FileName")]
	Label _FileName = null;

	[NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/Location")]
	Label _Location = null;

	[NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/FileSize")]
	Label _FileSize = null;

	[NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/Speed")]
	Label _Speed = null;

	[NodePath("PC/CC/P/VB/MCContent/VB/GridContainer/Eta")]
	Label _Eta = null;

	[NodePath("PC/CC/P/VB/MCContent/VB/ProgressBar")]
	ProgressBar _ProgressBar = null;

	[NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")]
	Button _Cancel = null;

	[NodePath("DownloadSpeedTimer")]
	Timer _DownloadSpeedTimer = null;
	#endregion
	
	#region Private Variables
	int iTotalBytes;
	int iLastByteCount;
	int iFileSize;
	DateTime dtStartTime;
	GDCSHTTPClient client;
	Uri dlUri;
	Array<double> adSpeedStack;
	Github.Release grRelease = null;
	Github.Asset gaAsset = null;
	#endregion

	#region Initializing Controls
	public override void _Ready()
	{
		this.OnReady();
		iTotalBytes = 0;
		iLastByteCount = 0;
		iFileSize = 0;
		adSpeedStack = new Array<double>();
	}
	#endregion
	
	#region Event Handlers
	[SignalHandler("pressed", nameof(_Cancel))]
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
		_FileSize.Text = Util.FormatSize(tb) + "/" + Util.FormatSize(iFileSize);
		TimeSpan elapsedTime = DateTime.Now - dtStartTime;
		if (tb == 0)
			return;
		TimeSpan estTime = TimeSpan.FromSeconds((iFileSize - tb) / ((double)tb / elapsedTime.TotalSeconds));
		_Eta.Text = estTime.ToString("hh':'mm':'ss");
		iLastByteCount = iTotalBytes;
		mutex.Unlock();
	}
	#endregion

	#region Private Functions
	void InitClient()
	{
		if (client != null)
			CleanupClient();
		
		client = new GDCSHTTPClient();
		client.Connect("chunk_received", this, "OnChunkReceived");
	}

	void CleanupClient() {
		client.Disconnect("chunk_received", this, "OnChunkReceived");
		client.QueueFree();
		client = null;
	}

	async Task<bool> StartNetwork() {
		adSpeedStack.Clear();
		Task<HTTPClient.Status> cres = client.StartClient(dlUri.Host, (dlUri.Scheme == "https"));

		while (!cres.IsCompleted)
			await this.IdleFrame();
		
		if (!client.SuccessConnect(cres.Result))
		{
			Visible = false;
			return false;
		}

		var tresult = client.HeadRequest(dlUri.PathAndQuery);
		while (!tresult.IsCompleted)
			await this.IdleFrame();
		client.Close();

		HTTPResponse result = tresult.Result;
		Array<int> redirect_codes = new Array<int> {301,302,303,307,308};

		if (redirect_codes.IndexOf(result.ResponseCode) >= 0)
		{
			dlUri = new Uri(result.Headers["Location"] as string);
			CleanupClient();
			InitClient();
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

		cres = client.StartClient(dlUri.Host, (dlUri.Scheme == "https"));
		while (!cres.IsCompleted)
			await this.IdleFrame();
		
		if (!client.SuccessConnect(cres.Result))
		{
			CleanupClient();
			Visible = false;
			return false;
		}

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
		if (tresult.IsCanceled)
		{
			CleanupClient();
			Visible = false;
			return false;
		}

		result = tresult.Result;

		if (result.Cancelled)
		{
			AppDialogs.MessageDialog.ShowMessage(Tr("Download Cancelled"), Tr("Download of update was cancelled."));
			CleanupClient();
			Visible = false;
			return false;
		}

		string sPath = $"{Util.GetUpdateFolder()}/update.zip".GetOSDir().NormalizePath();

		if (!Dir.Exists(sPath.GetBaseDir()))
			Dir.CreateDirectory(sPath.GetBaseDir());
		
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
		EmitSignal("download_complete", grRelease, gaAsset);

		return true;
	}
	#endregion

	#region Dialog Functions
	public async void ShowDialog(Github.Release release) {
		string platform = "";
#if GODOT_WINDOWS || GODOT_UWP
		platform = "windows";
#elif GODOT_LINUXBSD || GODOT_X11
		platform = "linux";
#elif GODOT_MACOS || GODOT_OSX
		platform = "mac";
#endif
		var t = from rAsset in release.Assets
				where rAsset.Name.FindN(platform) > -1
				select rAsset;
		if (t.FirstOrDefault() is Github.Asset ghAsset) {
			gaAsset = ghAsset;
		} else {
			return;
		}
		_FileName.Text = gaAsset.Name;
		_Location.Text = gaAsset.BrowserDownloadUrl;
		_FileSize.Text = Util.FormatSize(gaAsset.Size);
		_Speed.Text = Tr("Unknown");
		_Eta.Text = Tr("Unknown");
		_ProgressBar.Value = 0;
		_ProgressBar.MaxValue = 100;
		iTotalBytes = 0;
		iLastByteCount = 0;
		iFileSize = gaAsset.Size;
		Visible = true;
		InitClient();
		dlUri = new Uri(gaAsset.BrowserDownloadUrl);
		await StartNetwork();
	}
	#endregion
}
