using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace Github {
	public class Github : Godot.Node {
		[Signal]
		public delegate void chunk_received(int size);

		[Signal]
		public delegate void request_completed();

#region Singleton Instance
		private static Github _instance;

		public static Github Instance {
			get {
				if (_instance == null)
					_instance = new Github();
				return _instance;
			}
		}
#endregion

		HTTPClient client = null;

		private RateLimit lastLimit;

		public RateLimit Limit {
			get {
				return lastLimit;
			}
		}

		private HTTPResponse lastResponse;
		
		public HTTPResponse LastResponse {
			get {
				return lastResponse;
			}
		}

		private Github() {
			client = new HTTPClient();
			lastLimit = new RateLimit();
		}

		private string[] GetRequestHeaders() {
			return new string[] {
				"Accept: application/vnd.github.v3+json",
				"User-Agent: Godot-Manager-v0.1"
			};
		}

		private async Task<HTTPClient.Status> StartClient() {
			client.BlockingModeEnabled = false;
			var res = client.ConnectToHost("api.github.com",-1,true,true);

			if (res != Error.Ok)
				return HTTPClient.Status.ConnectionError;
			
			while (client.GetStatus() == HTTPClient.Status.Connecting ||
					client.GetStatus() == HTTPClient.Status.Resolving)
			{
				client.Poll();
				await ToSignal(Engine.GetMainLoop(),"idle_frame");
			}

			return client.GetStatus();
		}

		private async Task<HTTPResponse> MakeRequest(string path, string[] headers) {
			HTTPResponse resp = null;
			var res = client.Request(HTTPClient.Method.Get, path, GetRequestHeaders());
			if (res != Error.Ok)
				return null;
			
			while (client.GetStatus() == HTTPClient.Status.Requesting) {
				client.Poll();
				await ToSignal(Engine.GetMainLoop(), "idle_frame");
			}

			if (client.HasResponse()) {
				resp = new HTTPResponse();
				var task = resp.FromClient(this, client);
				while (!task.IsCompleted) {
					await ToSignal(Engine.GetMainLoop(), "idle_frame");
				}
				lastResponse = resp;
			}
			EmitSignal("request_completed");
			return resp;
		}

		public async Task<Array<Release>> GetReleases(int per_page=0, int page=1) {
			Array<Release> ret = new Array<Release>();
			Task<HTTPClient.Status> cres = StartClient();
			
			while (!cres.IsCompleted) {
				await ToSignal(Engine.GetMainLoop(), "idle_frame");
			}

			if (cres.Result != HTTPClient.Status.Connected) {
				switch(cres.Result) {
					case HTTPClient.Status.CantResolve:
						GD.PrintErr("Unable to resolve api.github.com");
						OS.Alert("Unable to resolve api.github.com", "Github Failure");
						return null;
					case HTTPClient.Status.CantConnect:
						GD.PrintErr("Failed to connect to api.github.com");
						OS.Alert("Failed to connect to api.github.com", "Github Failure");
						return null;
					case HTTPClient.Status.ConnectionError:
						GD.PrintErr("Connection error with api.github.com");
						OS.Alert("Connection error with api.github.com", "Github Failure");
						return null;
					case HTTPClient.Status.SslHandshakeError:
						GD.PrintErr("Failed to negotiate SSL Connection with api.github.com");
						OS.Alert("Failed to negotiate SSL Connection with api.github.com", "Github Failure");
						return null;
				}
			}

			string path = "/repos/godotengine/godot/releases";
			if (per_page > 0)
				path += $"?per_page={per_page}";
			if (page > 1 && per_page > 0)
				path += $"&page={page}";
			else if (page > 1)
				path += $"?page={page}";
			
			var tresult = MakeRequest(path, GetRequestHeaders());
			while (!tresult.IsCompleted) {
				await ToSignal(Engine.GetMainLoop(), "idle_frame");
			}

			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			client.Close();

			Limit.Limit = (result.Headers["X-RateLimit-Limit"] as string).ToInt();
			Limit.Remaining = (result.Headers["X-RateLimit-Remaining"] as string).ToInt();
			Limit.Reset = System.DateTimeOffset.FromUnixTimeSeconds((result.Headers["X-RateLimit-Reset"] as string).ToInt()).DateTime;
			Limit.Used = (result.Headers["X-RateLimit-Used"] as string).ToInt();

			// Check for Errors:
			if (result.ResponseCode != 200)
				return null;
			else
				ret = JsonConvert.DeserializeObject<Array<Release>>(result.Body, DefaultSettings.defaultJsonSettings);
			
			mutex.Unlock();

			return ret;
		}
	}
}