using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;

public class GifAnimatedTexture {

	public Godot.AnimatedTexture Texture;
	private Godot.Collections.Array<Godot.ImageTexture> gdFrames;

	public GifAnimatedTexture(string file) {
		if (File.Exists(file.GetOSDir() + ".res")) {
			Texture = Godot.GD.Load<Godot.AnimatedTexture>(file + ".res");
			return;
		}
		gdFrames = new Godot.Collections.Array<Godot.ImageTexture>();
		Image gif = Image.Load(file.GetOSDir());
		Texture = new Godot.AnimatedTexture();
		Texture.Frames = gif.Frames.Count >= 256 ? 256 : gif.Frames.Count;
		Texture.Fps = 24.0f;

		for(int i = 0; i < Texture.Frames; i++) {
			using(MemoryStream ms = new MemoryStream()) {
				GifFrameMetadata gifMeta = gif.Frames[i].Metadata.GetGifMetadata();
				Image iframe = gif.Frames.CloneFrame(i);
				iframe.SaveAsPng(ms);
				ms.Position = 0;
				Godot.Image img = new Godot.Image();
				img.LoadPngFromBuffer(ms.ToArray());
				Godot.ImageTexture gditFrame = new Godot.ImageTexture();
				gditFrame.CreateFromImage(img);
				gdFrames.Add(gditFrame);
#pragma warning disable CS0618
				Texture.SetFrameTexture(i, gditFrame);
				Texture.SetFrameDelay(i, gifMeta.FrameDelay * 0.01f);
#pragma warning restore CS0618
			}
		}

		Godot.ResourceSaver.Save(file + ".res", Texture, Godot.ResourceSaver.SaverFlags.BundleResources & Godot.ResourceSaver.SaverFlags.Compress);
	}
}