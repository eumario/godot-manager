using System.Threading.Tasks;
using Godot;

public class GDCSHTTPClient : Node {
	[Signal]
	public delegate void chunk_received(int size);

	[Signal]
	public delegate void request_completed();

	private HTTPClient client = null;
	private HTTPResponse lastResponse;
	private string sHost;
	private string sProperName;
	private bool bUseSSL;
	private bool bCancelled;

	public GDCSHTTPClient() {
		client = new HTTPClient();
	}

	public HTTPResponse LastResponse {
		get {
			return lastResponse;
		}
	}

	private string[] GetRequestHeaders() {
		return new string[] {
			"Accept: application/vnd.github.v3+json",
			"User-Agent: Godot-Manager/0.1"
		};
	}

	public void Close() {
		client.Close();
	}

	public void Cancel() {
		bCancelled = true;
	}
	public bool IsCancelled() => bCancelled;

	public async Task<HTTPClient.Status> StartClient(string host, bool use_ssl = false) {
		client.BlockingModeEnabled = false;
		sHost = host;
		var split = sHost.Split('.');
		if (split.Length == 2) {
			sProperName = split[0].Capitalize();
		} else if (split.Length == 3) {
			sProperName = split[1].Capitalize();
		} else {
			sProperName = sHost.Capitalize();
		}
		bCancelled = false;
		bUseSSL = use_ssl;
		var res = client.ConnectToHost(host,-1,use_ssl,use_ssl);

		if (res != Error.Ok)
			return HTTPClient.Status.ConnectionError;
		
		while (client.GetStatus() == HTTPClient.Status.Connecting ||
				client.GetStatus() == HTTPClient.Status.Resolving)
		{
			client.Poll();
			await this.IdleFrame();
		}

		return client.GetStatus();
	}

	public async Task<HTTPClient.Status> StartClient(string host, int port, bool use_ssl = false) {
		client.BlockingModeEnabled = false;
		sHost = host;
		var split = sHost.Split('.');
		if (split.Length == 2) {
			sProperName = split[0].Capitalize();
		} else if (split.Length == 3) {
			sProperName = split[1].Capitalize();
		} else {
			sProperName = sHost.Capitalize();
		}
		bCancelled = false;
		bUseSSL = use_ssl;
		var res = client.ConnectToHost(host,port,use_ssl, use_ssl);

		if (res != Error.Ok)
			return HTTPClient.Status.ConnectionError;
		
		while (client.GetStatus() == HTTPClient.Status.Connecting ||
				client.GetStatus() == HTTPClient.Status.Resolving)
		{
			client.Poll();
			await this.IdleFrame();
		}

		return client.GetStatus();
	}

	public async Task<HTTPResponse> HeadRequest(string path) {
		HTTPResponse resp = null;
		var res = client.Request(HTTPClient.Method.Head, path, GetRequestHeaders());
		if (res != Error.Ok)
			return resp;
		
		while (client.GetStatus() == HTTPClient.Status.Requesting) {
			if (bCancelled) {
				break;
			}
			client.Poll();
			await this.IdleFrame();
		}

		if (bCancelled)
			return resp;

		if (client.HasResponse()) {
			resp = new HTTPResponse();
			resp.ResponseCode = client.GetResponseCode();
			resp.Headers = client.GetResponseHeadersAsDictionary();
		}
		return resp;
	}

	public async Task<HTTPResponse> MakeRequest(string path, bool binary = false) {
		HTTPResponse resp = null;
		var res = client.Request(HTTPClient.Method.Get, path, GetRequestHeaders());
		if (res != Error.Ok)
			return null;
		
		while (client.GetStatus() == HTTPClient.Status.Requesting) {
			if (bCancelled) {
				break;
			}
			client.Poll();
			await this.IdleFrame();
		}

		if (bCancelled)
			return resp;

		if (client.HasResponse()) {
			resp = new HTTPResponse();
			var task = resp.FromClient(this, client, binary);
			while (!task.IsCompleted) {
				if (bCancelled) {
					resp.Cancelled = bCancelled;
					break;
				}
				await this.IdleFrame();
			}
			lastResponse = resp;
		}
		EmitSignal("request_completed");
		return resp;
	}

	public bool SuccessConnect(HTTPClient.Status result, bool dialogErrors = false, bool printErrors = true) {
		switch(result) {
			case HTTPClient.Status.CantResolve:
				if (printErrors) GD.PrintErr(string.Format(Tr("Unable to resolve {0}"),sHost));
				if (dialogErrors) OS.Alert(string.Format(Tr("Unable to resolve {0}"),sHost), string.Format(Tr("{0} Failure"),sProperName));
				return false;
			case HTTPClient.Status.CantConnect:
				if (printErrors) GD.PrintErr(string.Format(Tr("Unable to resolve {0}:{1}"),sHost,bUseSSL ? 443 : 80));
				if (dialogErrors) OS.Alert(string.Format(Tr("Unable to resolve {0}:{1}"),sHost,bUseSSL ? 443 : 80), string.Format(Tr("{0} Failure"),sProperName));
				return false;
			case HTTPClient.Status.ConnectionError:
				if (printErrors) GD.PrintErr(string.Format(Tr("Connection error with {0}:{1}"),sHost,bUseSSL ? 443 : 80));
				if (dialogErrors) OS.Alert(string.Format(Tr("Connection error with {0}:{1}"),sHost,bUseSSL ? 443 : 80), string.Format(Tr("{0} Failure"),sProperName));
				return false;
			case HTTPClient.Status.SslHandshakeError:
				if (printErrors) GD.PrintErr(string.Format(Tr("Failed to negotiate SSL Connection with {0}:{1}"),sHost,bUseSSL ? 443 : 80));
				if (dialogErrors) OS.Alert(string.Format(Tr("Failed to negotiate SSL Connection with {0}:{1}"),sHost,bUseSSL ? 443 : 80), string.Format(Tr("{0} Failure"),sProperName));
				return false;
			case HTTPClient.Status.Connected:
				return true;
			default:
				return false;
		}
	}
}