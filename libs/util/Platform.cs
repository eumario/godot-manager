using Godot;
using Environment = System.Environment;

public class Platform : Node
{
    public static string OperatingSystem
    {
		get {
#if GODOT_WINDOWS
        	return "Windows";
#elif GODOT_LINUXBSD || GODOT_X11
        	return "Linux (or BSD)";
#elif GODOT_SERVER
        	return "Server (Linux or BSD)";
#elif GODOT_MACOS || GODOT_OSX
        	return "macOS";
#elif GODOT_ANDROID
        	return "Android";
#elif GODOT_IOS
        	return "iOS";
#elif GODOT_HTML5
        	return "HTML5";
#elif GODOT_HAIKU
        	return "Haiku";
#elif GODOT_UWP
        	return "UWP (Windows 10)";
#elif GODOT
        	return "Other";
#else
        	return "Unknown";
#endif
		}
    }

	public static string Bits {
		get {
			if (Environment.Is64BitProcess)
				return "64";
			else
				return "32";
		}
	}

    public static string PlatformType
    {
		get {
#if GODOT_PC
        	return "PC";
#elif GODOT_MOBILE
        	return "Mobile";
#elif GODOT_WEB
        	return "Web";
#elif GODOT
        	return "Other";
#else
        	return "Unknown";
#endif
		}
    }
}