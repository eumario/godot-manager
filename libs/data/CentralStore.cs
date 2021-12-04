using Godot;
using Godot.Collections;
using Newtonsoft.Json;

public class CentralStore {
	static CentralStore _instance;

	private Array<ProjectFile> _projects;
	private Array<GodotVersion> _versions;
	private Array<GithubVersion> _ghVersions;
	private Array<TuxfamilyVersion> _tfVersions;
	private Array<Category> _categories;
	
	public static CentralStore Instance {
		get {
			if (_instance == null)
				_instance = new CentralStore();
			return _instance;
		}
	}

	protected CentralStore() {
		_projects = new Array<ProjectFile>();
		_versions = new Array<GodotVersion>();
		_ghVersions = new Array<GithubVersion>();
		_tfVersions = new Array<TuxfamilyVersion>();
		_categories = new Array<Category>();
	}

	public Array<ProjectFile> Projects {
		get {
			return _projects;
		}
	}

	public Array<GodotVersion> Versions {
		get {
			return _versions;
		}
	}

	public Array<GithubVersion> GHVersions {
		get {
			return _ghVersions;
		}
	}

	public Array<TuxfamilyVersion> TFVersions {
		get {
			return _tfVersions;
		}
	}

	public Array<Category> Categories {
		get {
			return _categories;
		}
	}

	public void LoadDatabase() {
		File db = new File();

		// Load Projects
		if (db.Open("user://projects.json",File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_projects = JsonConvert.DeserializeObject<Array<ProjectFile>>(data);
		}

		// Load Versions
		if (db.Open("user://versions.json", File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_versions = JsonConvert.DeserializeObject<Array<GodotVersion>>(data);
		}

		if (db.Open("user://gh_versions.json", File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_ghVersions = JsonConvert.DeserializeObject<Array<GithubVersion>>(data);
		}

		if (db.Open("user://tf_versions.json", File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_tfVersions = JsonConvert.DeserializeObject<Array<TuxfamilyVersion>>(data);
		}

		// Load Categories
		if (db.Open("user://categories.json", File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_categories = JsonConvert.DeserializeObject<Array<Category>>(data);
		}
	}

	public bool HasProject(string name) {
		if (System.IO.Path.GetFullPath(name) == name) {
			foreach (ProjectFile pf in _projects) {
				if (pf.Location == name)
					return true;
			}
		} else {
			foreach (ProjectFile pf in _projects) {
				if (pf.Name == name)
					return true;
			}
		}
		return false;
	}

	public void SaveDatabase() {
		File db = new File();

		if (db.Open("user://projects.json", File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_projects);
			db.StoreString(data);
			db.Close();
		}

		if (db.Open("user://versions.json", File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_versions);
			db.StoreString(data);
			db.Close();
		}

		if (db.Open("user://gh_versions.json", File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_ghVersions);
			db.StoreString(data);
			db.Close();
		}

		if (db.Open("user://tf_versions.json", File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_tfVersions);
			db.StoreString(data);
			db.Close();
		}

		if (db.Open("user://categories.json", File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_categories);
			db.StoreString(data);
			db.Close();
		}
	}

}