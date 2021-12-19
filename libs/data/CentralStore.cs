using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Linq;

public class CentralStore {
	static CentralStore _instance;

	private CentralStoreData _data = null;

	public static CentralStore Instance {
		get {
			if (_instance == null)
				_instance = new CentralStore();
			return _instance;
		}
	}

	protected CentralStore() {
		if (!LoadDatabase()) {
			_data = new CentralStoreData();
		}
	}

	public Array<ProjectFile> Projects {
		get {
			return _data.Projects;
		}
	}

	public Array<GodotVersion> Versions {
		get {
			return _data.Versions;
		}
	}

	public Array<GithubVersion> GHVersions {
		get {
			return _data.GHVersions;
		}
	}

	public Array<TuxfamilyVersion> TFVersions {
		get {
			return _data.TFVersions;
		}
	}

	public Array<Category> Categories {
		get {
			return _data.Categories;
		}
	}

	public bool LoadDatabase() {
		File db = new File();
		if (db.Open("user://central_store.json", File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_data = JsonConvert.DeserializeObject<CentralStoreData>(data);
			return true;
		}
		return false;
	}

	public void SaveDatabase() {
		File db = new File();
		if (db.Open("user://central_store.json", File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_data);
			db.StoreString(data);
			db.Close();
		}
	}

	public bool HasProject(string name) {
		if (System.IO.Path.GetFullPath(name) == name) {
			var res = from pf in _data.Projects
					where pf.Location == name
					select pf;
			return res.FirstOrDefault() != null;
		} else {
			var res = from pf in _data.Projects
					where pf.Name == name
					select pf;
			return res.FirstOrDefault() != null;
		}
	}

	public GodotVersion FindVersion(string id) {
		var query = from gv in Versions
					where gv.Id == id
					select gv;
		return query.FirstOrDefault<GodotVersion>();
	}
}