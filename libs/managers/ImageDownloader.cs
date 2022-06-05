using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Uri = System.Uri;

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
		Uri uri;
		if (bIsRedirected)
			uri = new Uri(sRedirected);
		else
			uri = new Uri(sUrl);
		
		if (CentralStore.Settings.UseProxy)
			client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, uri.Scheme == "https");
		else
			client.ClearProxy();
		
		Task<HTTPClient.Status> cres = client.StartClient(uri.Host, uri.Port, (uri.Scheme == "https"));

		while (!cres.IsCompleted)
			await this.IdleFrame();
		
		if (!client.SuccessConnect(cres.Result, false))
			return false;
		
		var tresult = client.MakeRequest(uri.PathAndQuery);
		while (!tresult.IsCompleted)
			await this.IdleFrame();
		
		HTTPResponse result = tresult.Result;
		client.Close();
		Array<int> redirect_codes = new Array<int> { 301, 302, 303, 307, 308 };
		
		if (redirect_codes.IndexOf(result.ResponseCode) >= 0) {
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
			return false;
		}

		return true;
	}
}