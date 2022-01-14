using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;

public class GifTexture : Godot.ImageTexture {
	public GifTexture(string file) {
		Image gif = Image.Load(file.GetOSDir());
		using(MemoryStream ms = new MemoryStream()) {
			Image iframe = gif.Frames.CloneFrame(0);
			iframe.SaveAsPng(ms);
			ms.Position = 0;
			Godot.Image img = new Godot.Image();
			img.LoadPngFromBuffer(ms.ToArray());
			CreateFromImage(img);
		}
	}
}