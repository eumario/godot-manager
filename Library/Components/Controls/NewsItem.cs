using Godot;
using System;
using Godot.Sharp.Extras;
using GodotManager.Library.Managers;
using GodotManager.Library.Utility;

public partial class NewsItem : Control
{
	public static NewsItem FromScene() => GD.Load<PackedScene>("res://Library/Components/Controls/NewsItem.tscn").Instantiate<NewsItem>();
	
	#region Node Paths
	[NodePath] private Button _newsHeadline;
	[NodePath] private TextureRect _authorAvatar;
	[NodePath] private Label _author;
	[NodePath] private Label _publishDate;
	[NodePath] private TextureRect _articleImage;
	[NodePath] private RichTextLabel _articleBlerb;
	#endregion
	
	#region Public Properties

	private string _headline;
	public string Headline
	{
		get => _headline;
		set
		{
			_headline = value;
			if (_newsHeadline != null)
				_newsHeadline.Text = value;
		}
	}
	private Texture2D _avatar;
	public Texture2D Avatar
	{
		get => _avatar;
		set
		{
			_avatar = value;
			if (_authorAvatar != null)
				_authorAvatar.Texture = value;
		}
	}
	private string _authorName;
	public string AuthorName
	{
		get => _authorName;
		set
		{
			_authorName = value;
			if (_author != null)
				_author.Text = value;
		}
	}
	private string _date;
	public string Date
	{
		get => _date;
		set
		{
			_date = value;
			if (_publishDate != null)
				_publishDate.Text = value;
		}
	}
	private Texture2D _image;
	public Texture2D Image
	{
		get => _image;
		set
		{
			_image = value;
			if (_articleImage != null)
			{
				_articleImage.Texture = value;
				var targetHeight = _articleImage.Size.Y + _newsHeadline.Size.Y + _publishDate.Size.Y + 45;
				if (CustomMinimumSize.Y < targetHeight)
					CustomMinimumSize = new Vector2(CustomMinimumSize.X, targetHeight);
				UpdateMinimumSize();
			}
		}
	}
	private string _blerb;
	public string Blerb
	{
		get => _blerb;
		set
		{
			_blerb = value;
			if (_articleBlerb != null)
			{
				_articleBlerb.Text = value;
				UpdateMinimumSize();
			}
		}
	}
	public string Url { get; set; }
	public ImageDownloader ImageDld { get; set; }
	public ImageDownloader AvatarDld { get; set; }

	public TextureRect ImageRect => _articleImage;
	public TextureRect AvatarRect => _authorAvatar;
	#endregion
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();

		Headline = _headline;
		Avatar = _avatar;
		AuthorName = _authorName;
		Date = _date;
		Image = _image;
		Blerb = _blerb;

		_newsHeadline.Pressed += () =>
		{
			GD.Print($"Launching Url: {Url}");
			Util.LaunchWeb(Url);
		};
	}
}
