using Godot;
using Godot.Collections;

public static class Util {
	public static string GetResourceBase(this string path, string file) {
		return System.IO.Path.Combine(path.GetBaseDir(), file.Replace("res://", "")).Replace(@"\","/");
	}

	public static ImageTexture LoadImage(this string path, int width = 64, int height = 64, Image.Interpolation interpolate = Image.Interpolation.Cubic) {
		Image img = new Image();
		img.Load(path);
		img.Resize(width,height,interpolate);
		ImageTexture texture = new ImageTexture();
		texture.CreateFromImage(img);
		return texture;
	}

	static string[] ByteSizes = new string[5] { "B", "KB", "MB", "GB", "TB"};


	public static string FormatSize(double bytes) {
		double len = bytes;
		int order = 0;
		while (len >= 1024 && order < ByteSizes.Length - 1) {
			order++;
			len = len / 1024;
		}
		return string.Format("{0:0.##} {1}", len, ByteSizes[order]);
	}

	public static string NormalizePath(this string path) {
#if GODOT_WINDOWS
		return path.Replace("/",@"\");
#else
		return path.Replace(@"\","/");
#endif
	}

	public static string Join(this string path, params string[] addTo) {
		foreach(string part in addTo) {
			path += "/" + part;
		}
		return path.NormalizePath();
	}

	public static SignalAwaiter IdleFrame(this Godot.Object obj) {
		return obj.ToSignal(Engine.GetMainLoop(), "idle_frame");
	}

	public static string EngineVersion {
		get {
			Dictionary vers = Engine.GetVersionInfo();
			return $"{vers["major"]}.{vers["minor"]}.{vers["patch"]}";
		}
	}
}