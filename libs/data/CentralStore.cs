using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Linq;
using Directory = System.IO.Directory;
using Path = System.IO.Path;
using SFile = System.IO.File;
using Dir = System.IO.Directory;

public class CentralStore {
#region C# Pattern for Singleton
	static CentralStore _instance;

	private CentralStoreData _data = null;

	protected CentralStore() {
		if (!LoadDatabase()) {
			_data = new CentralStoreData();
		}
	}
	
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
	public static Array<MirrorSite> Mirrors { get => Instance._data.Mirrors; }
	public static Dictionary<int, Array<MirrorVersion>> MRVersions { get => Instance._data.MRVersions; }
	public static Array<Category> Categories { get => Instance._data.Categories; }
	public static Array<int> PinnedCategories { get => Instance._data.PinnedCategories; }
	public static Array<AssetPlugin> Plugins { get => Instance._data.Plugins; }
	public static Array<AssetProject> Templates { get => Instance._data.Templates; }
	public static Array<CustomEngineDownload> CustomEngines { get => Instance._data.CustomEngines; }
#endregion

#region Instance Methods
	public bool LoadDatabase()
	{
		File db = new File();
		if (db.Open(Util.GetDatabaseFile(), File.ModeFlags.Read) == Error.Ok) {
			var data = db.GetAsText();
			db.Close();
			_data = JsonConvert.DeserializeObject<CentralStoreData>(data);
			var unique = _data.Settings.ScanDirs.Distinct<string>();
			_data.Settings.ScanDirs = new Array<string>(unique.ToArray<string>());
			return true;
		}
		return false;
	}

	public void SaveDatabase() {
		File db = new File();
		SortGodotVersions();
		if (db.Open(Util.GetDatabaseFile(), File.ModeFlags.Write) == Error.Ok) {
			var data = JsonConvert.SerializeObject(_data);
			db.StoreString(data);
			db.Close();
			return;
		}
	}

	void SortGodotVersions()
	{
		_data.Versions = new Array<GodotVersion>(_data.Versions.OrderByDescending(x => x).ToArray());
	}

	public bool HasProject(string name) {
		var res = from pf in _data.Projects
				where pf.Location == name
				select pf;
		return res.FirstOrDefault() != null;
	}

	public bool HasTemplate(string name) {
		var res = from pt in Templates
					where pt.Asset.Title == name
					select pt;
		return res.FirstOrDefault() != null;
	}

	public bool HasTemplateId(string id) {
		var res = from pt in Templates
					where pt.Asset.AssetId == id
					select pt;
		return res.FirstOrDefault() != null;
	}

	public bool HasPlugin(string name) {
		var res = from pt in Plugins
					where pt.Asset.Title == name
					select pt;
		return res.FirstOrDefault() != null;
	}

	public bool HasPluginId(string id) {
		var res = from pt in Plugins
				where pt.Asset.AssetId == id
				select pt;
		return res.FirstOrDefault() != null;
	}

	public AssetProject GetTemplate(string name) {
		var res = from pt in Templates
					where pt.Asset.Title == name
					select pt;
		return res.FirstOrDefault<AssetProject>();
	}

	public AssetProject GetTemplateId(string id) {
		var res = from pt in Templates
					where pt.Asset.AssetId == id
					select pt;
		return res.FirstOrDefault<AssetProject>();
	}

	public AssetPlugin GetPlugin(string name) {
		var res = from pt in Plugins
					where pt.Asset.Title == name
					select pt;
		return res.FirstOrDefault<AssetPlugin>();
	}

	public AssetPlugin GetPluginId(string id) {
		var res = from pt in Plugins
					where pt.Asset.AssetId == id
					select pt;
		return res.FirstOrDefault<AssetPlugin>();
	}

	public GodotVersion FindVersion(string id) {
		var query = from gv in Versions
					where gv.Id == id
					select gv;
		return query.FirstOrDefault<GodotVersion>();
	}

	public GodotVersion GetVersion(string id)
	{
		if (Versions.Count <= 0) return null;
		var query = from gv in Versions
					where gv.Id == id
					select gv;
		return query.First<GodotVersion>();
	}

	public bool HasCategory(string name) {
		var res = from c in Categories
					where c.Name.ToLower() == name.ToLower()
					select c;
		return res.FirstOrDefault() != null;
	}

	public bool HasCategoryId(int id) {
		var res = from c in Categories
					where c.Id == id
					select c;
		return res.FirstOrDefault() != null;
	}

	public Category GetCategoryById(int id) {
		var res = from c in Categories
					where c.Id == id
					select c;
		return res.FirstOrDefault();
	}

	public Category GetCategoryByName(string name) {
		var res = from c in Categories
					where c.Name.ToLower() == name.ToLower()
					select c;
		return res.FirstOrDefault();
	}

	public void PinCategory(Category cat)
	{
		if (IsCategoryPinned(cat))
			return;
		PinnedCategories.Add(cat.Id);
	}

	public void UnpinCategory(Category cat)
	{
		if (!IsCategoryPinned(cat))
			return;
		PinnedCategories.Remove(cat.Id);
	}

	public bool IsCategoryPinned(Category cat)
	{
		return PinnedCategories.Contains(cat.Id);
	}

	public Array<Category> GetPinnedCategories()
	{
		var pinned = new Array<Category>();
		foreach (int id in PinnedCategories)
			pinned.Add(GetCategoryById(id));
		return pinned;
	}

	public Array<Category> GetUnpinnedCategories()
	{
		var unpinned = new Array<Category>();
		foreach (Category cat in Categories)
		{
			if (!IsCategoryPinned(cat))
				unpinned.Add(cat);
		}

		return unpinned;
	}

	public bool HasCustomEngineId(int id)
	{
		var res = from e in CustomEngines
			where e.Id == id
			select e;
		return res.FirstOrDefault() != null;
	}
#endregion

}
