using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

public class GifTexture {

	Image gif;
	FrameDimension dim;

	public Godot.AnimatedTexture Texture;
	Godot.Collections.Array<Godot.Image> gdiFrames;
	Godot.Collections.Array<int> delays;

	public GifTexture(string file) {
		gif = Image.FromFile(file.GetOSDir());
		dim = new FrameDimension(gif.FrameDimensionsList[0]);
		gdiFrames = new Godot.Collections.Array<Godot.Image>();
		delays = new Godot.Collections.Array<int>();
		Texture = new Godot.AnimatedTexture();
		Texture.Frames = gif.GetFrameCount(dim);
		Texture.Fps = 30.0f;

#if GODOT_WINDOWS
		// Windows Returns 4-byte array of delays for frames
		var times = gif.GetPropertyItem(0x5100).Value;
		for(int i = 0; i < Texture.Frames; i++)
			delays.Add(BitConverter.ToInt32(times, 4*i % times.Length));
#else
		// Linux/Android/iOS/Mac OS Mono runtime returns 4 bytes per selected active frame.
		for(int i = 0; i < Texture.Frames; i++) {
			gif.SelectActiveFrame(dim, i);
			delays.Add(BitConverter.ToInt32(gif.GetPropertyItem(0x5100).Value,0));
		}
#endif

		Rectangle rect = new Rectangle(0, 0, gif.Width, gif.Height);
		for (int i = 0; i < Texture.Frames; i++) {
			gif.SelectActiveFrame(dim, i);
			PropertyItem item = gif.GetPropertyItem(0x5100); // FrameDealy in libgdiplus
			using(Bitmap resultingImage = new Bitmap(gif.Width, gif.Height))
			using(Graphics grD = Graphics.FromImage(resultingImage))
			using(MemoryStream ms = new MemoryStream())
			{
				Godot.Image img = new Godot.Image();
				// Draw image to Buffer
				grD.DrawImage(gif, rect, rect, GraphicsUnit.Pixel);
				// Save Image to Memory Stream
				resultingImage.Save(ms, ImageFormat.Png);
				ms.Position = 0;
				// Load up into Godot Image
				img.LoadPngFromBuffer(ms.ToArray());
				// Convert to ImageTexture
				Godot.ImageTexture gditFrame = new Godot.ImageTexture();
				gditFrame.CreateFromImage(img);
				// Set Image Texture as a Frame.
#pragma warning disable CS0618
				Texture.SetFrameTexture(i, gditFrame);
				Texture.SetFrameDelay(i,delays[i] * 0.01f);
#pragma warning restore CS0618
			}
		}
	}

}