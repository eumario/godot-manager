using Godot;
using Godot.Collections;
using StreamReader = System.IO.StreamReader;
using StreamWriter = System.IO.StreamWriter;

public class ProjectConfig : Object {
	string buffer;

	const string HEADER = @"; Engine configuration file.
; It's best edited using the editor UI and not directly,
; since the parameters that go here are not all obvious.
; 
; Format:
;   [section] ; section goes between []
;   param=value ; assign values to parameters
";

	Dictionary<string, Dictionary<string, string>> sections;
	string fileName;

	public ProjectConfig(string fname="") {
		fileName = fname;
		buffer = "";
		sections = new Dictionary<string, Dictionary<string, string>>();
	}

	public string this[string section, string key] {
		get => sections[section][key];
		set => sections[section][key] = value;
	}

	public string GetValue(string section, string key, string defval = null) {
		string retval = defval;
		if (HasSection(section) && HasSectionKey(section,key)) {
			retval = this[section,key];
			if (retval.BeginsWith("\"") && retval.EndsWith("\""))
				retval = retval.Substring(1,retval.Length-2);
		}
		return retval;
	}

	public void SetValue(string section, string key, string value) {
		if (!HasSection(section))
			sections[section] = new Dictionary<string, string>();
		this[section,key] = value;
	}

	public bool HasSection(string section) => sections.Keys.Contains(section);
	public bool HasSectionKey(string section, string key) => sections[section].Keys.Contains(key);

	public void DebugPrint() {
		GD.PrintErr("DEBUG>  <<< Sections >>>");
		foreach(string section in sections.Keys) {
			GD.PrintErr($"DEBUG> [{section}]");
			foreach(string key in sections[section].Keys) {
				GD.PrintErr($"DEBUG> >>{key}={sections[section][key]}");
			}
		}
	}

	public Error Load(string fname = "") { 
		if (fname != "")
			fileName = fname;
		
		sections = new Dictionary<string, Dictionary<string, string>>();

		sections["header"] = new Dictionary<string, string>();
		string current_section = "header";
		string last_key = "";

		using (StreamReader reader = new StreamReader(fileName)) {
			string data = "";
			while ((data = reader.ReadLine()) != null) {
				var line = data.StripEdges();
				
				// Handle Comments
				if (line.BeginsWith(";"))
					continue;
				
				// Handle Section Definition
				if (line.BeginsWith("[")) {
					current_section = line.Substring(1,line.Length - 2);
					sections[current_section] = new Dictionary<string, string>();
					continue;
				}

				// Handle Key-Value Pairs
				if (line.IndexOf("=") != -1) {
					string[] parts = line.Split("=");
					string key = parts[0];
					last_key = key;
					string value = parts[1];
					sections[current_section][key] = value;
					continue;
				}

				// Handle Key-Value pairs that go across multiple lines
				if (!string.IsNullOrEmpty(line)) {
					sections[current_section][last_key] += "\n" + line;
				}
			}
		}
		return Error.Ok;
	}

	public Error Save(string fname = "") {
		if (fname != "")
			fileName = fname;
		
		using (StreamWriter writer = new StreamWriter(fileName)) {
			writer.WriteLine(HEADER);

			foreach(string key in sections["header"].Keys) {
				writer.WriteLine($"{key}={sections["header"][key]}");
			}

			writer.WriteLine("");

			foreach(string section in sections.Keys) {
				if (section == "header")
					continue;
				
				writer.WriteLine($"[{section}]");
				writer.WriteLine("");

				foreach(string key in sections[section].Keys) {
					writer.WriteLine($"{key}={sections[section][key]}");
				}

				writer.WriteLine("");
			}
		}
		return Error.Ok;
	}

	public Error OldLoad(string fname = "") {
		if (fname != "")
			fileName = fname;
		
		using (File fh = new File()) {
			Error ret = fh.Open(fileName, File.ModeFlags.Read);
			if (ret == Error.Ok) {
				buffer = fh.GetAsText();
			} else {
				return ret;
			}
			fh.Close();
		}
		sections["header"] = new Dictionary<string, string>();
		string current_section = "header";
		string last_key = "";
		foreach(string line in buffer.Split("\n")) {
			var nline = line.StripEdges();
			if (nline.BeginsWith(";")) // Comment, ignore.
				continue;
			if (nline.BeginsWith("[")) { // Section Header
				current_section = nline.Substring(1,nline.Length-2);
				sections[current_section] = new Dictionary<string, string>();
				continue;
			}
			if (nline.IndexOf("=") != -1) { // Key-Value Pair
				string[] parts = nline.Split("=");
				string key = parts[0];
				last_key = key;
				string value = parts[1];
				sections[current_section][key] = value;
				continue;
			}
			if (!string.IsNullOrEmpty(nline)) { // Godot3 stores Dictionaries, and possibly arrays across multiple lines
				sections[current_section][last_key] += "\n" + nline;
			}
		}
		return Error.Ok;
	}

	public Error OldSave(string fname = "") {
		if (fname != "")
			fileName = fname;

		using (File fh = new File()) {
			Error ret = fh.Open(fileName, File.ModeFlags.Write);
			if (ret != Error.Ok) {
				return ret;
			}
			fh.StoreBuffer(HEADER.ToUTF8());

			foreach(string key in sections["header"].Keys) {
				fh.StoreLine($"{key}={sections["header"][key]}");
			}

			fh.StoreLine(" ");

			foreach(string section in sections.Keys) {
				if (section == "header")
					continue;
				
				fh.StoreLine($"[{section}]");
				fh.StoreLine(" ");
				foreach(string key in sections[section].Keys) {
					fh.StoreLine($"{key}={sections[section][key]}");
				}
				fh.StoreLine(" ");
			}

			fh.Close();
		}
		return Error.Ok;
	}
}