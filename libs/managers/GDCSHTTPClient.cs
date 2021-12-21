using System.Threading.Tasks;
using Godot;
using Godot.Collections;

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

	public async Task<HTTPResponse> MakeRequest(string path) {
		HTTPResponse resp = null;
		var res = client.Request(HTTPClient.Method.Get, path, GetRequestHeaders());
		if (res != Error.Ok)
			return null;
		
		while (client.GetStatus() == HTTPClient.Status.Requesting) {
			client.Poll();
			await this.IdleFrame();
		}

		if (client.HasResponse()) {
			resp = new HTTPResponse();
			var task = resp.FromClient(this, client);
			while (!task.IsCompleted) {
				await this.IdleFrame();
			}
			lastResponse = resp;
		}
		EmitSignal("request_completed");
		return resp;
	}

	public bool SuccessConnect(HTTPClient.Status result) {
		switch(result) {
			case HTTPClient.Status.CantResolve:
				GD.PrintErr($"Unable to resolve {sHost}");
				OS.Alert($"Unable to resolve {sHost}", $"{sProperName} Failure");
				return false;
			case HTTPClient.Status.CantConnect:
				GD.PrintErr($"Failed to connect to {sHost}:{(bUseSSL ? 443 : 80)}");
				OS.Alert($"Failed to connect to {sHost}:{(bUseSSL ? 443 : 80)}", $"{sProperName} Failure");
				return false;
			case HTTPClient.Status.ConnectionError:
				GD.PrintErr($"Connection error with {sHost}:{(bUseSSL ? 443 : 80)}");
				OS.Alert($"Connection error with {sHost}:{(bUseSSL ? 443 : 80)}", $"{sProperName} Failure");
				return false;
			case HTTPClient.Status.SslHandshakeError:
				GD.PrintErr($"Failed to negotiate SSL Connection with {sHost}:{(bUseSSL ? 443 : 80)}");
				OS.Alert($"Failed to negotiate SSL Connection with {sHost}:{(bUseSSL ? 443 : 80)}", $"{sProperName} Failure");
				return false;
			case HTTPClient.Status.Connected:
				return true;
			default:
				return false;
		}
	}
}