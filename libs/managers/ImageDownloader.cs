using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;

public class ImageDownloader : Object {
	GDCSHTTPClient client;

	public Task ActiveTask { get; set; }

	private string sUrl;
	private string sRedirected;
	private string sOutPath;
	private bool bIsRedirected;

	public string Url { get { return sUrl; }}

	public ImageDownloader(string url, string outPath) {
		sUrl = url;
		sOutPath = outPath;
		bIsRedirected = false;
		client = new GDCSHTTPClient();
	}

	public async Task<bool> StartDownload() {
		System.Uri uri;
		if (bIsRedirected)
			uri = new System.Uri(sRedirected);
		else
			uri = new System.Uri(sUrl);
		
		Task<HTTPClient.Status> cres;

		if (uri.Scheme == "https") {
			cres = client.StartClient(uri.Host, true);
		} else {
			cres = client.StartClient(uri.Host);
		}

		while (!cres.IsCompleted)
			await this.IdleFrame();
		
		if (!client.SuccessConnect(cres.Result, false))
			return false;
		
		var tresult = client.MakeRequest(uri.PathAndQuery);
		while (!tresult.IsCompleted)
			await this.IdleFrame();
		
		HTTPResponse result = tresult.Result;
		client.Close();

		if (result.ResponseCode == 302) {
			bIsRedirected = true;
			sRedirected = result.Headers["Location"] as string;
			Task<bool> recurse = StartDownload();
			while (!recurse.IsCompleted)
				await this.IdleFrame();
			return recurse.Result;
		}

		if (result.ResponseCode != 200) {
			return false;
		}

		File fh = new File();
		Error err = fh.Open(sOutPath, File.ModeFlags.Write);
		if (err == Error.Ok) {
			fh.StoreBuffer(result.BodyRaw);
			fh.Close();
		} else {
			GD.Print($"Failed to open file {sOutPath}, Error: {err}");
		}

		return true;
	}
}