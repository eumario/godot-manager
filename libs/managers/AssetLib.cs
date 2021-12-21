using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;

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

		GDCSHTTPClient client = null;

		private AssetLib() {
			client = new GDCSHTTPClient();
		}

		public async Task<Dictionary<string,Array<Dictionary<string,string>>>> Configure() {
			Dictionary<string,Array<Dictionary<string,string>>> ret = null;
			Task<HTTPClient.Status> cres = client.StartClient("godotengine.org");

			while (!cres.IsCompleted) {
				await this.IdleFrame();
			}

			if (!client.SuccessConnect(cres.Result))
				return ret;
			
			string path = "/asset-library/configure?type=any";
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted) {
				await this.IdleFrame();
			}

			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			client.Close();

			if (result.ResponseCode != 200)
				return null;
			else
				ret = JsonConvert.DeserializeObject<Dictionary<string,Array<Dictionary<string,string>>>>(result.Body, Github.DefaultSettings.defaultJsonSettings);
			
			mutex.Unlock();

			return ret;
		}
	}
}