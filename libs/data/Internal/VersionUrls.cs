using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Reflection;

[JsonObject(MemberSerialization.OptIn)]
public class VersionUrls : Object
{

	[JsonProperty]
	public string Win32; // Github.Release.Assets[indx].Name.FindN("[mono_]win32")

	[JsonProperty]
	public int Win32_Size; // Github.Release.Assets[indx].Size;

	[JsonProperty]
	public string Win64; // Github.Release.Assets[indx].Name.FindN("[mono_]win64")

	[JsonProperty]
	public int Win64_Size; // Github.Release.Assets[indx].Size;

	[JsonProperty]
	public string Linux32; // Github.Release.Assets[indx].Name.FindN("[mono_]x11[_|.]32")
	
	[JsonProperty]
	public int Linux32_Size; // Github.Release.Assets[indx].Size;

	[JsonProperty]
	public string Linux64; // Github.Release.Assets[indx].Name.FindN("[mono_]x11[_|.]64)

	[JsonProperty]
	public int Linux64_Size; // Github.Release.Assets[indx].Size;

	[JsonProperty]
	public string OSX; // Github.Release.Assets[indx].Name.FindN("[mono_]osx")

	[JsonProperty]
	public int OSX_Size; // Github.Release.Assets[indx].Size;

	[JsonProperty]
	public string Templates; // Github.Release.Assets[indx].Name.FindN("[mono_]export_templates.tpz")

	[JsonProperty]
	public int Templates_Size; // Github.Release.Assets[indx].Size;

	[JsonProperty]
	public string Headless; // Github.Release.Assets[indx].Name.FindN("[mono_]linux_headless[_|.]64")
	
	[JsonProperty]
	public int Headless_Size; // Github.Release.Assets[indx].Size;

	[JsonProperty]
	public string Server; // Github.Release.Assets[indx].Name.FindN("[mono_]linux_server[_|.]64")

	[JsonProperty]
	public int Server_Size; // Github.Release.Assets[indx].Size;

	public object this[string key] {
		get {
			switch (key) {
				case "Win32": return Win32;
				case "Win32_Size": return Win32_Size;
				case "Win64": return Win64;
				case "Win64_Size": return Win64_Size;
				case "Linux32": return Linux32;
				case "Linux32_Size": return Linux32_Size;
				case "Linux64": return Linux64;
				case "Linux64_Size": return Linux64_Size;
				case "OSX": return OSX;
				case "OSX_Size": return OSX_Size;
				case "Templates": return Templates;
				case "Templates_Size": return Templates_Size;
				case "Headless": return Headless;
				case "Headless_Size": return Headless_Size;
				case "Server": return Server;
				case "Server_Size": return Server_Size;
				default: return null;
			}
		}

		set {
			switch(key) {
				case "Win32": Win32 = value as string; break;
				case "Win32_Size": Win32_Size = (int)value; break;
				case "Win64": Win64 = value as string; break;
				case "Win64_Size": Win64_Size = (int)value; break;
				case "Linux32": Linux32 = value as string; break;
				case "Linux32_Size": Linux32_Size = (int)value; break;
				case "Linux64": Linux64 = value as string; break;
				case "Linux64_Size": Linux64_Size = (int)value; break;
				case "OSX": OSX = value as string; break;
				case "OSX_Size": OSX_Size = (int)value; break;
				case "Templates": Templates = value as string; break;
				case "Templates_Size": Templates_Size = (int)value; break;
				case "Headless": Headless = value as string; break;
				case "Headless_Size": Headless_Size = (int)value; break;
				case "Server": Server = value as string; break;
				case "Server_Size": Server_Size = (int)value; break;
				default: break;
			}
		}
	}

}