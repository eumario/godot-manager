using Godot;
using Godot.Sharp.Extras;

public class NewsItem : Panel
{
    [NodePath("vb/Headline")] private Label _headline;
    [NodePath("vb/hbby/Avatar")] private TextureRect _avatar;
    [NodePath("vb/hbby/Byline")] private Label _byline;

    [NodePath("vb/hb/Image")] private TextureRect _image;
    [NodePath("vb/hb/Blerb")] private RichTextLabel _blerb;

    private string _sHeadline;
    private string _sAvatar;
    private string _sByline;
    private string _sImage;
    private string _sBlerb;

    public string Headline
    {
        get => _sHeadline;
        set
        {
            _sHeadline = value;
            if (_headline != null)
                _headline.Text = value;
        }
    }

    public string Avatar
    {
        get => _sAvatar;
        set
        {
            _sAvatar = value;
            if (_avatar == null || value == null || value.Empty()) return;
            var img = Util.LoadImage(_sAvatar);
            _avatar.Texture = img;
        }
    }
    public string Byline
    {
        get => _sByline;
        set
        {
            _sByline = value;
            if (_byline != null)
                _byline.Text = value;
        }
    }

    public string Image
    {
        get => _sImage;
        set
        {
            _sImage = value;
            if (_image == null || value == null || value.Empty()) return;
            var img = Util.LoadImage(_sImage);
            _image.Texture = img;
        }
    }

    public string Blerb
    {
        get => _sBlerb;
        set
        {
            _sBlerb = value;
            if (_blerb != null)
                _blerb.BbcodeText = value;
        }
    }

    public string Url;

    public override void _Ready()
    {
        this.OnReady();

        Headline = _sHeadline;
        Byline = _sByline;
        Image = _sImage;
        Blerb = _sBlerb;
        Avatar = _sAvatar;
    }

    [SignalHandler("gui_input")]
    void OnGuiInput(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton iemb))
            return;

        if (iemb.ButtonIndex == 1 && iemb.Pressed)
        {
            OS.ShellOpen(Url);
        }
    }
}
