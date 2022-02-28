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

		public async Task<ConfigureResult> Configure(bool templatesOnly) {
			ConfigureResult ret = null;
			Task<HTTPClient.Status> cres = client.StartClient("godotengine.org");

			while (!cres.IsCompleted) {
				await this.IdleFrame();
			}

			if (!client.SuccessConnect(cres.Result))
				return ret;
			
			string path = "/asset-library/api/configure";

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

		public async Task<QueryResult> Search(string query) {
			QueryResult ret = null;
			Task<HTTPClient.Status> cres = client.StartClient("godotengine.org");

			while (!cres.IsCompleted)
				await this.IdleFrame();
			
			if (!client.SuccessConnect(cres.Result))
				return ret;
			
			string path = $"/asset-library/api/asset{query}";
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

		public async Task<QueryResult> Search(int page = 0, bool templates_only = false, int sort_by = 0, string[] support_list = null, int category = 0, string filter = "") {
			string args = "";
			if (templates_only)
				args += "?type=project&";
			else
				args += "?";
			
			args += $"sort={sort_key[sort_by]}";

			args += $"&godot_version={Util.EngineVersion}";

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

			var result = Search(args);

			while (!result.IsCompleted)
				await this.IdleFrame();
			
			return result.Result;
		}

		public async Task<Asset> GetAsset(string assetId) {
			Asset res = null;
			Task<HTTPClient.Status> cres = client.StartClient("godotengine.org");

			while (!cres.IsCompleted)
				await this.IdleFrame();
			
			if (!client.SuccessConnect(cres.Result))
				return res;
			
			string path = $"/asset-library/api/asset/{assetId}";
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