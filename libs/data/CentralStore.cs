using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Linq;

public class CentralStore {
#region C# Pattern for Singleton
	static CentralStore _instance;

	private CentralStoreData _data = null;

	protected CentralStore() {
		if (!LoadDatabase()) {
			_data = new CentralStoreData();
		}
	}
	//public static CentralStore Instance { get => (_instance == null) ? _instance = new CentralStore() : _instance; }
	public static CentralStore Instance {
		get {
			if (_instance == null)
				_instance = new CentralStore();
			return _instance;
		}
	}
#endregion

#region Static Members to Instance Members
	public static Settings Settings { get => Instance._data.Settings; }
	public static Array<ProjectFile> Projects { get => Instance._data.Projects; }
	public static Array<GodotVersion> Versions { get => Instance._data.Versions; }
	public static Array<GithubVersion> GHVersions { get => Instance._data.GHVersions; }
	public static Array<TuxfamilyVersion> TFVersions { get => Instance._data.TFVersions; }
	public static Array<Category> Categories { get => Instance._data.Categories; }
#endregion

#region Instance Methods
	public bool LoadDatabase() {
		GD.Print("Attempting to load database...");
		File db = new File();
		if (db.Open("user://central_store.json", File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_data = JsonConvert.DeserializeObject<CentralStoreData>(data);
			GD.Print("Load Database successful.");
			return true;
		}
		GD.Print("Load Database failed.");
		return false;
	}

	public void SaveDatabase() {
		GD.Print("Attempting to save database...");
		File db = new File();
		if (db.Open("user://central_store.json", File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_data);
			db.StoreString(data);
			db.Close();
			GD.Print("Save Database successful.");
			return;
		}
		GD.Print("Save Database failed.");
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
#endregion

}