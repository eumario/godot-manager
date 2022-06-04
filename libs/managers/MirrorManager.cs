using System.Threading.Tasks;
using Github;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using DateTimeOffset = System.DateTimeOffset;

namespace Mirrors {
	public class MirrorManager : Node {

#region Signals
		[Signal] public delegate void chunk_received(int size);
		[Signal] public delegate void request_completed();
#endregion

#region Singleton Instance
		private static MirrorManager _instance;

		public static MirrorManager Instance {
			get {
				if (_instance == null)
					_instance = new MirrorManager();
				return _instance;
			}
		}
#endregion

		GDCSHTTPClient client = null;

		private MirrorManager() {
			client = new GDCSHTTPClient();
		}

		public async Task<Array<MirrorSite>> GetMirrors() {
			Array<MirrorSite> mirrors = new Array<MirrorSite>();
			Task<HTTPClient.Status> cres = client.StartClient("gmm.eumario.info", true);

			while (!cres.IsCompleted)
				await this.IdleFrame();
			
			if (!client.SuccessConnect(cres.Result))
				return mirrors;
			
			string path = "/mirrors";
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted)
				await this.IdleFrame();
			
			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;

			client.Close();

			if (result.ResponseCode != 200)
				return mirrors;
			else
				mirrors = JsonConvert.DeserializeObject<Array<MirrorSite>>(result.Body, DefaultSettings.defaultJsonSettings);
			mutex.Unlock();

			return mirrors;
		}

		public async Task<Array<MirrorVersion>> GetEngineLinks(int mirrorId) {
			Array<MirrorVersion> versions = new Array<MirrorVersion>();
			Task<HTTPClient.Status> cres = client.StartClient("gmm.eumario.info", true);

			while (!cres.IsCompleted)
				await this.IdleFrame();
			
			if (!client.SuccessConnect(cres.Result))
				return versions;
			
			string path = $"/mirrors/{mirrorId}";
			var tresult = client.MakeRequest(path);
			while (!tresult.IsCompleted)
				await this.IdleFrame();
			
			Mutex mutex = new Mutex();
			mutex.Lock();
			HTTPResponse result = tresult.Result;
			client.Close();

			if (result.ResponseCode == 200)
				versions = JsonConvert.DeserializeObject<Array<MirrorVersion>>(result.Body, DefaultSettings.defaultJsonSettings);
			
			mutex.Unlock();

			return versions;

		}
	}
}