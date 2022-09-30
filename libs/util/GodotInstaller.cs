using Godot;
using Godot.Collections;

using Guid = System.Guid;
using Uri = System.Uri;
using DateTime = System.DateTime;
using FPath = System.IO.Path;
using SFile = System.IO.File;
using SDirectory = System.IO.Directory;
using System.IO.Compression;
using System.Threading.Tasks;

public class GodotInstaller : Object {

	[Signal] public delegate void chunk_received(int size);
	[Signal] public delegate void download_completed(GodotInstaller self);
	[Signal] public delegate void download_failed(GodotInstaller self, HTTPClient.Status error);

	GDCSHTTPClient _client = null;

	GodotVersion _version = null;

	public GodotVersion GodotVersion {
		get {
			return _version;
		}
	}

	public int DownloadSize {
		get {
			if (_version.GithubVersion == null && _version.CustomEngine == null)
				return _version.MirrorVersion.PlatformDownloadSize;
			
			if (_version.CustomEngine == null && _version.MirrorVersion == null)
				return _version.IsMono ? 
						_version.GithubVersion.PlatformMonoDownloadSize : 
						_version.GithubVersion.PlatformDownloadSize;
			return _version.CustomEngine.DownloadSize;
		}
	}

	public GodotInstaller(GodotVersion version) {
		_version = version;
		_client = new GDCSHTTPClient();
		_client.Connect("chunk_received", this, "OnChunkReceived");
		_client.Connect("headers_received", this, "OnHeadersReceived");
	}

	public static GodotInstaller FromGithub(GithubVersion gh, bool is_mono = false) {
		string gdFile = is_mono ? gh.PlatformMonoDownloadURL : gh.PlatformDownloadURL;
		gdFile = new Uri(gdFile).AbsolutePath.GetFile();
		GodotVersion gv = new GodotVersion() {
			Id = Guid.NewGuid().ToString(),
			Tag = gh.Name,
			Url = is_mono ? gh.PlatformMonoDownloadURL : gh.PlatformDownloadURL,
#if GODOT_MACOS || GODOT_OSX
			Location = $"{CentralStore.Settings.EnginePath}/{gh.Name + (is_mono ? "_mono" : "") }",
#else
			Location = $"{CentralStore.Settings.EnginePath}/{(is_mono ? gdFile.ReplaceN(".zip","") : gh.Name)}",
#endif
			CacheLocation = $"{CentralStore.Settings.CachePath}/Godot/{gdFile}".GetOSDir().NormalizePath(),
			DownloadedDate = DateTime.UtcNow,
			GithubVersion = gh,
			IsMono = is_mono
		};

		GodotInstaller installer = new GodotInstaller(gv);
		return installer;
	}

	public static GodotInstaller FromMirror(MirrorVersion mv, bool is_mono = false) {
		GodotVersion gv = new GodotVersion() {
			Id = Guid.NewGuid().ToString(),
			Tag = mv.Version,
			Url = mv.PlatformDownloadURL,
#if GODOT_MACOS || GODOT_OSX
			Location = $"{CentralStore.Settings.EnginePath}/{mv.Version + (is_mono ? "_mono" : "") }",
#else
			Location = $"{CentralStore.Settings.EnginePath}/{(is_mono ? mv.PlatformZipFile.ReplaceN(".zip","") : mv.Version)}",
#endif
			CacheLocation = $"{CentralStore.Settings.CachePath}/Godot/{mv.PlatformZipFile}".GetOSDir().NormalizePath(),
			DownloadedDate = DateTime.UtcNow,
			MirrorVersion = mv,
			IsMono = is_mono
		};

		GodotInstaller installer = new GodotInstaller(gv);
		return installer;
	}

	public static GodotInstaller FromCustomEngineDownload(CustomEngineDownload ced)
	{
		GodotVersion gv = new GodotVersion()
		{
			Id = Guid.NewGuid().ToString(),
			Tag = ced.TagName,
			Url = ced.Url,
#if GODOT_MACOS || GODOT_OSX
			Location = $"{CentralStore.Settings.EnginePath}/{ced.TagName}",
#else
			Location = $"{CentralStore.Settings.EnginePath}/{ced.TagName}",
#endif
			CacheLocation = $"{CentralStore.Settings.CachePath}/Godot/{ced.Url.GetFile()}",
			DownloadedDate = DateTime.Now,
			CustomEngine = ced
		};

		GodotInstaller installer = new GodotInstaller(gv);
		return installer;
	}

	void OnHeadersReceived(Dictionary headers)
	{
		if (DownloadSize == 0)
		{
			if (headers.Contains("Transfer-Encoding")) // ContainsKey("Transfer-Encoding"))
				return;
			int size = 0;
			if (int.TryParse((string)headers["Content-Length"], out size))
			{
				_version.CustomEngine.DownloadSize = size;
			}
		}
	}

	public static GodotInstaller FromVersion(GodotVersion vers) {
		return new GodotInstaller(vers);
	}

	void OnChunkReceived(int size) {
		EmitSignal("chunk_received", size);
	}

	public async Task<HTTPResponse> FollowRedirect(string url = "") {
		Uri dlUri;
		if (url == "")
			dlUri = new Uri(_version.Url);
		else
			dlUri = new Uri(url);
		
		var res = await _client.StartClient(dlUri.Host, dlUri.Port, dlUri.Scheme == "https");

		if (res != HTTPClient.Status.Connected) {
			EmitSignal("download_failed", this, res);
			return null;
		}

		var resp = _client.MakeRequest(dlUri.PathAndQuery, true);

		while (!resp.IsCompleted)
			await this.IdleFrame();

		if (resp.Result.ResponseCode == 302) {
			return await FollowRedirect((string)resp.Result.Headers["Location"]);
		}

		return resp.Result;
	}

	public async Task Download() {
		Uri dlUri = new Uri(_version.Url);
		if (CentralStore.Settings.UseProxy)
			_client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, dlUri.Scheme == "https");
		else
			_client.ClearProxy();
		
		var resp = FollowRedirect();

		while (!resp.IsCompleted)
			await this.IdleFrame();
		
		if (resp.Result == null) {
			EmitSignal("download_failed", this, HTTPClient.Status.Requesting);
			return;
		}

		Mutex mutex = new Mutex();
		mutex.Lock();
		File file = new File();
		if (file.Open(_version.CacheLocation, File.ModeFlags.Write) != Error.Ok) {
			EmitSignal("download_failed", this, HTTPClient.Status.Body);
			return;
		}

		file.StoreBuffer(resp.Result.BodyRaw);
		file.Close();
		mutex.Unlock();
		EmitSignal("download_completed", this);
	}

	public void Install() {
		string instDir = _version.Location;
#if GODOT_WINDOWS || GODOT_UWP || GODOT_LINUXBSD || GODOT_X11
		if (_version.IsMono)
			instDir = instDir.GetBaseDir();
#endif
		ZipFile.ExtractToDirectory(_version.CacheLocation,instDir);

		Array<string> fileList = new Array<string>();
		using (ZipArchive za = ZipFile.OpenRead(_version.CacheLocation.GetOSDir().NormalizePath())) {
			foreach(ZipArchiveEntry zae in za.Entries) {
				fileList.Add(zae.Name);
			}
		}

#if GODOT_WINDOWS || GODOT_UWP
		foreach (string fname in fileList) {
			if (fname.EndsWith(".exe") && fname.StartsWith("Godot")) {
				_version.ExecutableName = fname;
				break;
			}
		}
#elif GODOT_LINUXBSD || GODOT_X11
		foreach (string fname in fileList) {
			if (System.Environment.Is64BitProcess) {
				if (fname.EndsWith(".64") && fname.StartsWith("Godot")) {
					_version.ExecutableName = fname;
					break;
				} else if (fname.EndsWith(".x86_64") && fname.StartsWith("Godot")) {
					_version.ExecutableName = fname;
					break;
				}
			} else {
				if (fname.EndsWith(".32") && fname.StartsWith("Godot")) {
					_version.ExecutableName = fname;
					break;
				} else if (fname.EndsWith(".x86_32") && fname.StartsWith("Godot")) {
					_version.ExecutableName = fname;
					break;
				}
			}
		}

		Util.Chmod(_version.GetExecutablePath(), 0755);
#elif GODOT_MACOS || GODOT_OSX

		_version.ExecutableName = "Godot";
		Util.Chmod(_version.GetExecutablePath(), 0755);

#endif
		if (CentralStore.Settings.SelfContainedEditors) {
			File fh = new File();
			fh.Open($"{_version.Location}/._sc_".GetOSDir().NormalizePath(), File.ModeFlags.Write);
			fh.StoreString(" ");
			fh.Close();
		}
	}

	internal Array<string> RecurseDirectory(string path) {
		Array<string> files = new Array<string>();
		foreach(string dir in SDirectory.EnumerateDirectories(path)) {
			foreach(string file in RecurseDirectory(FPath.Combine(path,dir).NormalizePath())) {
				files.Add(file);
			}
			files.Add(FPath.Combine(path,dir).NormalizePath());
		}

		foreach(string file in SDirectory.EnumerateFiles(path)) {
			files.Add(file.NormalizePath());
		}

		files.Add(path.NormalizePath());

		return files;
	}

	public void Uninstall() {
		foreach(string file in RecurseDirectory(_version.Location)) {
			if (SDirectory.Exists(file))
				SDirectory.Delete(file);
			else
				SFile.Delete(file);
		}

		SFile.Delete(_version.CacheLocation);
	}
}
