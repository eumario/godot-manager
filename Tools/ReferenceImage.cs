using Godot;
using System.Collections.Generic;
using System.IO;

namespace GodotManager.Tools;

[Tool, GlobalClass]
public partial class ReferenceImage : TextureRect
{
    [ExportCategory("Preview Image")]
    [Notify, Export(PropertyHint.Dir)] public partial string ImagePath { get; set; }
    [Notify, Export(PropertyHint.Range, "0,1,1,or_greater")] public partial int CurrentIndex { get; set; }
    [Export(PropertyHint.TypeString)] public string CurrentImage { get; set; }
    [ExportToolButton("<< Prev Img")] public Callable PreviousImageButton => Callable.From(PreviousImage);
    [ExportToolButton("Next Img >>")] public Callable NextImageButton => Callable.From(NextImage);
    

    private List<string> _images;
    private int _lastIndex = 0;

    public ReferenceImage()
    {
        ImagePath = "";
        CurrentImage = "";
        CurrentIndex = 0;
        _images = [];
        ImagePathChanged += () => LoadImages();
        CurrentIndexChanged += ValidateIndex;
    }

    [GodotOverride]
    public void OnReady()
    {
        if (LoadImages()) return;

        Texture = GD.Load<Texture2D>(_images[CurrentIndex]);
        CurrentImage = _images[CurrentIndex].GetFile();
    }

    private bool LoadImages()
    {
        _images = [];
        if (ImagePath == "") return true;
        var gpath = ProjectSettings.GlobalizePath(ImagePath);
        foreach (var file in Directory.EnumerateFiles(gpath, "*.*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".svg"))
            {
                _images.Add(ProjectSettings.LocalizePath(file));
            }
        }

        return false;
    }

    private void ValidateIndex()
    {
        if (CurrentIndex >= 0 && CurrentIndex < _images.Count)
        {
            _lastIndex = CurrentIndex;
            Texture = GD.Load<Texture2D>(_images[CurrentIndex]);
            CurrentImage = _images[CurrentIndex].GetFile();
        }
        else
        {
            CurrentIndex = _lastIndex;
        }
    }

    private void PreviousImage()
    {
        CurrentIndex--;
        if (CurrentIndex < 0) CurrentIndex = _images.Count - 1;
        Texture = GD.Load<Texture2D>(_images[CurrentIndex]);
        CurrentImage = _images[CurrentIndex].GetFile();
    }

    private void NextImage()
    {
        CurrentIndex++;
        if (CurrentIndex >= _images.Count) CurrentIndex = 0;
        Texture = GD.Load<Texture2D>(_images[CurrentIndex]);
        CurrentImage = _images[CurrentIndex].GetFile();
    }

    public override partial void _Ready();
}
