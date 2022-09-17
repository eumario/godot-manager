using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using Uri = System.Uri;

namespace AssetLib {
	public class AssetLib : Node {
		[Signal]
		public delegate void chunk_received(int size);

		[Signal]
		public delegate void request_completed();

		private static AssetLib _instance;

		public static AssetLib Instance {
			get {
				if (_instance == null)
					_instance = new AssetLib();
				return _instance;
			}
		}

		private static string[] sort_key = new string[6] {
			"updated",
			"updated",
			"name",
			"name",
			"cost",
			"cost"
		};

		private static string[] sort_text = new string[6] {
			"Recently Updated",
			"Least Recently Updated",
			"Name (A-Z)",
			"Name (Z-A)",
			"License (A-Z)",
			"License (Z-A)"
		};

		private static string[] support_key = new string[3] {
			"official",
			"community",
			"testing"
		};

		GDCSHTTPClient client = null;

		private AssetLib() {
			client = new GDCSHTTPClient();
		}

		public async Task<ConfigureResult> Configure(string url, bool templatesOnly) {
			ConfigureResult ret = null;
			Uri uri = new Uri(url);
			if (CentralStore.Settings.UseProxy)
				client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, uri.Scheme == "https");
			else
				client.ClearProxy();
			Task<HTTPClient.Status> cres = client.StartClient(uri.Host, uri.Port, uri.Scheme == "https");

			while (!cres.IsCompleted) {
				await this.IdleFrame();
			}

			if (!client.SuccessConnect(cres.Result))
				return ret;
			
			//string path = "/asset-library/api/configure";
			string path = $"{uri.AbsolutePath}configure";

			if (templatesOnly)
				path += "?type=project";
			else
				path += "?type=addon";

			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted) {
				await this.IdleFrame();
			}

			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			client.Close();

			if (result == null) {
				mutex.Unlock();
				return ret;
			}

			if (result.ResponseCode == 200)
				ret = JsonConvert.DeserializeObject<ConfigureResult>(result.Body, Github.DefaultSettings.defaultJsonSettings);
			
			mutex.Unlock();

			return ret;
		}

		// TODO: Add API lookup of Pending Assets
		// TODO-URL: /asset-library/api/asset/edit?&asset=-1 == All Assets Pending
		// TODO-URL: /asset-library/api/asset/edit/{id}
		// TODO-NOTE: No Cookie Needed to access API.

		public async Task<QueryResult> Search(string query) {
			QueryResult ret = null;
			Uri uri = new Uri(query);
			if (CentralStore.Settings.UseProxy)
				client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, uri.Scheme == "https");
			Task<HTTPClient.Status> cres = client.StartClient(uri.Host, uri.Port, uri.Scheme == "https");

			while (!cres.IsCompleted)
				await this.IdleFrame();
			
			if (!client.SuccessConnect(cres.Result))
				return ret;
			
			//string path = $"/asset-library/api/asset{query}";
			string path = $"{uri.AbsolutePath}asset{uri.Query}";
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted)
				await this.IdleFrame();
			
			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			client.Close();

			if (result == null) {
				mutex.Unlock();
				return ret;
			}

			if (result.ResponseCode == 200) {
				if (!result.Cancelled && result.Body != "")
					ret = JsonConvert.DeserializeObject<QueryResult>(result.Body, Github.DefaultSettings.defaultJsonSettings);
			}
			
			mutex.Unlock();
			return ret;
		}

		public async Task<QueryResult> Search(string url, int page = 0, string godot_version = "any", bool templates_only = false, int sort_by = 0, string[] support_list = null, int category = 0, string filter = "") {
			string args = "";
			if (templates_only)
				args += "?type=project&";
			else
				args += "?";
			
			args += $"sort={sort_key[sort_by]}";
			
			args += $"&godot_version={godot_version}";

			if (support_list != null)
				args += $"&support={string.Join("+",support_list)}";
			
			if (category > 0)
				args += $"&category={category}";
			
			if (sort_by % 2 == 1)
				args += $"&reverse=true";
			
			if (filter != "")
				args += $"&filter={Uri.EscapeDataString(filter)}";
			
			if (page > 0)
				args += $"&page={page}";

			var result = Search(url + args);

			while (!result.IsCompleted)
				await this.IdleFrame();
			
			return result.Result;
		}

		public async Task<Asset> GetAsset(string assetId) {
			Asset res = null;
			Uri uri = new Uri($"https://godotengine.org/asset-library/api/asset/{assetId}");
			if (CentralStore.Settings.UseProxy)
				client.SetProxy(CentralStore.Settings.ProxyHost, CentralStore.Settings.ProxyPort, uri.Scheme == "https");
			else
				client.ClearProxy();
			Task<HTTPClient.Status> cres = client.StartClient(uri.Host, uri.Port, uri.Scheme == "https");

			while (!cres.IsCompleted)
				await this.IdleFrame();
			
			if (!client.SuccessConnect(cres.Result))
				return res;
			
			string path = uri.AbsolutePath;
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted)
				await this.IdleFrame();
			
			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			client.Close();

			if (result == null) {
				mutex.Unlock();
				return res;
			}

			if (result.ResponseCode == 200)
				res = JsonConvert.DeserializeObject<Asset>(result.Body, Github.DefaultSettings.defaultJsonSettings);
			
			mutex.Unlock();
			return res;
		}
	}
}