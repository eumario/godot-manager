using Godot;

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
}