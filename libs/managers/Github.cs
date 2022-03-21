using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTimeOffset = System.DateTimeOffset;

namespace Github {
	public class Github : Node {
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

		GDCSHTTPClient client = null;

		private RateLimit lastLimit;

		public RateLimit Limit {
			get {
				return lastLimit;
			}
		}

		private Github() {
			client = new GDCSHTTPClient();
			lastLimit = new RateLimit();
		}

		private void UpdateLimit(HTTPResponse response) {
			Limit.Limit = (response.Headers["X-RateLimit-Limit"] as string).ToInt();
			Limit.Remaining = (response.Headers["X-RateLimit-Remaining"] as string).ToInt();
			Limit.Reset = DateTimeOffset.FromUnixTimeSeconds((response.Headers["X-RateLimit-Reset"] as string).ToInt()).DateTime;
			Limit.Used = (response.Headers["X-RateLimit-Used"] as string).ToInt();
		}

		public async Task<Release> GetLatestRelease() {
			Release ret = null;
			Task<HTTPClient.Status> cres = client.StartClient("api.github.com",true);

			while (!cres.IsCompleted) {
				await this.IdleFrame();
			}

			if (!client.SuccessConnect(cres.Result))
				return ret;
			
			string path = "/repos/godotengine/godot/releases/latest";
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted) {
				await this.IdleFrame();
			}

			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			
			client.Close();
			
			UpdateLimit(result);

			if (result.ResponseCode != 200)
				return null;
			else
				ret = JsonConvert.DeserializeObject<Release>(result.Body, DefaultSettings.defaultJsonSettings);

			mutex.Unlock();

			return ret;			
		}

		public async Task<Release> GetLatestManagerRelease() {
			Release ret = null;
			Task<HTTPClient.Status> cres = client.StartClient("api.github.com", true);

			while (!cres.IsCompleted) {
				await this.IdleFrame();
			}

			if (!client.SuccessConnect(cres.Result))
				return ret;
			
			string path = "/repos/eumario/test-godot-manager/releases/latest";
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted) {
				await this.IdleFrame();
			}

			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;

			client.Close();

			UpdateLimit(result);

			if (result.ResponseCode != 200)
				return null;
			else
				ret = JsonConvert.DeserializeObject<Release>(result.Body, DefaultSettings.defaultJsonSettings);
			
			mutex.Unlock();

			return ret;
		}

		public async Task<Array<Release>> GetReleases(int per_page=0, int page=1) {
			Array<Release> ret = new Array<Release>();
			Task<HTTPClient.Status> cres = client.StartClient("api.github.com",true);
			
			while (!cres.IsCompleted) {
				await this.IdleFrame();
			}

			if (!client.SuccessConnect(cres.Result))
				return ret;

			string path = "/repos/godotengine/godot/releases";
			if (per_page > 0)
				path += $"?per_page={per_page}";
			if (page > 1 && per_page > 0)
				path += $"&page={page}";
			else if (page > 1)
				path += $"?page={page}";
			
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted) {
				await this.IdleFrame();
			}

			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			client.Close();

			UpdateLimit(result);

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