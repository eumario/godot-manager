using Godot;
using System.Threading.Tasks;
using Uri = System.Uri;

public class Downloader : Object {
	internal int bytesReceived = 0;
	internal int totalSize = 0;
	internal string downloadUrl = "";
	internal HTTPClient client;
	internal Uri downloadUri;

	[Signal]
	public delegate void chunk_received(int bytes);

	public Downloader() {
		client = new HTTPClient();
	}

	public static Downloader DownloadGithub(GithubVersion gh, bool downloadMono = false) {
		Downloader dl = new Downloader();
		dl.downloadUrl = downloadMono ? gh.PlatformMonoDownloadURL : gh.PlatformDownloadURL;
		dl.totalSize = downloadMono ? gh.PlatformMonoDownloadSize : gh.PlatformDownloadSize;
		dl.downloadUri = new Uri(dl.downloadUrl);
		return dl;
	}

	public static Downloader DownloadTuxfamily() {
		Downloader dl = new Downloader();
		return dl;
	}

	private async Task<HTTPClient.Status> StartClient() {
		client.BlockingModeEnabled = false;
		var res = client.ConnectToHost(downloadUri.Host, downloadUri.Port, downloadUri.Scheme == "https", downloadUri.Scheme == "https");

		if (res != Error.Ok)
			return HTTPClient.Status.ConnectionError;

		while (client.GetStatus() == HTTPClient.Status.Connecting ||
				client.GetStatus() == HTTPClient.Status.Resolving) {
			client.Poll();
			await this.IdleFrame();
		}

		return client.GetStatus();
	}

	private string[] GetRequestHeaders() {
		return new string[] {
			"Accept: application/vnd.github.v3+json",
			"User-Agent: Godot-Manager-v0.1"
		};
	}

	private bool SuccessConnect(HTTPClient.Status result) {
		switch(result) {
			case HTTPClient.Status.CantResolve:
				GD.PrintErr($"Unable to resolve {downloadUri.Host}");
				OS.Alert($"Unable to resolve {downloadUri.Host}", "Downloader Failure");
				return false;
			case HTTPClient.Status.CantConnect:
				GD.PrintErr($"Failed to connect to {downloadUri.Host}");
				OS.Alert($"Failed to connect to {downloadUri.Host}", "Downloader Failure");
				return false;
			case HTTPClient.Status.ConnectionError:
				GD.PrintErr($"Connection error with {downloadUri.Host}");
				OS.Alert($"Connection error with {downloadUri.Host}", "Downloader Failure");
				return false;
			case HTTPClient.Status.SslHandshakeError:
				GD.PrintErr($"Failed to negotiate SSL Connection with {downloadUri.Host}");
				OS.Alert($"Failed to negotiate SSL Connection with {downloadUri.Host}", "Downloader Failure");
				return false;
			case HTTPClient.Status.Connected:
				return true;
			default:
				return false;
		}
	}

	private async Task<HTTPResponse> MakeRequest() {
		HTTPResponse resp = null;
		var res = client.Request(HTTPClient.Method.Get, downloadUri.PathAndQuery, GetRequestHeaders());
		if (res != Error.Ok)
			return null;

		while (client.GetStatus() == HTTPClient.Status.Requesting) {
			client.Poll();
			await this.IdleFrame();
		}

		if (client.HasResponse()) {
			resp = new HTTPResponse();
			var task = resp.FromClient(this, client, true);
			while (!task.IsCompleted) {
				await this.IdleFrame();
			}
		}
		return resp;
	}

	public async Task<bool> DownloadFile(string pathTo) {
		HTTPResponse resp = null;
		Task<HTTPClient.Status> cres = StartClient();

		while (!cres.IsCompleted) {
			await this.IdleFrame();
		}

		if (!SuccessConnect(cres.Result))
			return false;

		Task<HTTPResponse> tres = MakeRequest();

		while (!tres.IsCompleted)
			await this.IdleFrame();

		resp = tres.Result;
		client.Close();

		if (resp.ResponseCode == 302) {
			downloadUrl = resp.Headers["Location"] as string;
			downloadUri = new Uri(downloadUrl);
			Task<bool> recurse = DownloadFile(pathTo);
			while (!recurse.IsCompleted)
				await this.IdleFrame();
			return recurse.Result;
		} else if (resp.ResponseCode == 200) {
			Mutex mutex = new Mutex();
			mutex.Lock();
			File file = new File();
			if (file.Open(pathTo, File.ModeFlags.Write) != Error.Ok) {
				return false;
			}

			file.StoreBuffer(resp.BodyRaw);
			file.Close();
			mutex.Unlock();
		}


		return true;
	}
}