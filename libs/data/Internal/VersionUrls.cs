using Godot;
using Godot.Collections;
using Newtonsoft.Json;

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

}