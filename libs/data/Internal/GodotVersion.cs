using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using DateTime = System.DateTime;
using Guid = System.Guid;
using Version = System.Version;

[JsonObject(MemberSerialization.OptIn)]
public class GodotVersion : Godot.Object, IComparable<GodotVersion> {
	[JsonProperty] public string Id; // This will be a UUID
	[JsonProperty] public string Tag; // This will be used to display to the user
	[JsonProperty] public bool IsMono; // This is used to determine if the file downloaded is Mono
	[JsonProperty] public string Location; // Location of where Godot is
	[JsonProperty] public string ExecutableName; // Name of the Final Executable
	[JsonProperty] public string CacheLocation; // Location of where the cache file is.
	[JsonProperty] public string Url;	// URL downloaded from (Will match Location for Custom)
	[JsonProperty] public DateTime DownloadedDate; // Date Downloaded (Added for Godot)
	[JsonProperty] public bool HideConsole;	// If we should hide the console for Godot Editor.
	[JsonProperty] public GithubVersion GithubVersion;
	[JsonProperty] public MirrorVersion MirrorVersion;
	[JsonProperty] public CustomEngineDownload CustomEngine;
	[JsonProperty] public string SharedSettings;

	public GodotVersion() {
		Id = Guid.Empty.ToString();
		Tag = "";
		Location = "";
		Url = "";
		DownloadedDate = DateTime.MinValue;
		HideConsole = false;
		SharedSettings = string.Empty;
	}

	public bool IsGodot4() => Tag.StartsWith("4") || Tag.StartsWith("v4");

	public string GetDisplayName() {
		return $"Godot {Tag + (IsMono ? " - Mono" : "")}";
	}

	public string GetExecutablePath() {
		string exe_path = "";
#if GODOT_MACOS || GODOT_OSX
		exe_path = Location.Join((IsMono ? "Godot_mono.app" : "Godot.app"), "Contents", "MacOS", ExecutableName).NormalizePath();
#else
		exe_path = Location.Join(ExecutableName).NormalizePath();
#endif
		return exe_path;
	}

	int IComparable<GodotVersion>.CompareTo(GodotVersion other) {
		int tag_compare = CompareVersionTags(Tag, other.Tag);

		if (tag_compare != 0) {
			return tag_compare;
		}

		// Non Mono/.Net versions sort higher than Mono/.Net of the same version
		return (IsMono == other.IsMono) ? 0 : (IsMono ? -1 : 1);
	}

	private static int CompareVersionTags(string tag_a, string tag_b) {
		// Match version strings like '3.5.1', 'v2.4', '0.1.0-alpha2', '4.3-dev.3'
		// Capture version part and -tag separately
		// Ignore any trailing characters
		Regex ver_pattern = new Regex(@"^v?([0-9]+\.[0-9]+(?:\.[0-9]+)?)(-[0-9a-zA-Z-\.]*)?");
		Match a_match = ver_pattern.Match(tag_a);
		Match b_match = ver_pattern.Match(tag_b);

		int valid_compare = a_match.Success.CompareTo(a_match.Success);
		if (!a_match.Success && !b_match.Success) {
			// Neither Tag has a valid version string, fall back to string comparison
			return tag_a.CompareTo(tag_b);
		} else if (valid_compare != 0) {
			// Valid versions sort higher
			return valid_compare;
		}

		Version a_version = Version.Parse(a_match.Groups[1].ToString());
		string[] a_extra = a_match.Groups[2].ToString().Split('.');

		Version b_version = Version.Parse(b_match.Groups[1].ToString());
		string[] b_extra = b_match.Groups[2].ToString().Split('.');

		int ver_compare = a_version.CompareTo(b_version);
		if (ver_compare != 0) {
			return ver_compare;
		} else if (a_extra.Length == 0 || b_extra.Length == 0) {
			// Whichever one has no -tag goes higher e.g. 1.0.1 vs 1.0.1-beta4
			return b_extra.Length.CompareTo(a_extra.Length);
		}

		// The -tag is compared like a version number, with parts separated by . but there 
		// can be any number of parts, and only parts with just digits are compared numerically.
		// numerical parts are always lower than non-numerical parts.

		Func<string, bool> isDigits = s => ('1' + s).IsValidInteger();

		Func<string, string, int> mixedComp;
		mixedComp = (string a, string b) => isDigits(b) ? 1 : (isDigits(a) ? -1 : a.CompareTo(b));

		Func<string, string, int> comp;
		comp = (string a, string b) => isDigits(a + b) ? a.ToInt() - b.ToInt() : mixedComp(a, b);

		int tag_compare = a_extra.Zip(b_extra, comp).FirstOrDefault(x => x != 0);
		if (tag_compare != 0) {
			return tag_compare;
		}

		// Finally, more terms in the -tag sorts higher
		return a_extra.Length.CompareTo(b_extra.Length);
	}
}
