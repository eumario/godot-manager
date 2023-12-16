using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data;
using GodotManager.Library.Managers;
using GodotManager.Library.Utility;

// namespace
namespace GodotManager.Library.Components.Panels;

public partial class NewsPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths
	[NodePath] private Button _refreshNews;
	[NodePath] private VBoxContainer _newsList;
	#endregion
	
	#region Private Variables
	private readonly Uri _newsUri = new Uri("https://godotengine.org/blog");
	private readonly Uri _baseUri = new Uri("https://godotengine.org");
	private List<ImageDownloader> _downloads;
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		_downloads = new List<ImageDownloader>();
		GetParent<TabContainer>().TabChanged += OnPageChanged;
		_refreshNews.Pressed += RefreshNews;
	}
	#endregion
	
	#region Event Handlers
	private void OnPageChanged(long i)
	{
		if (GetParent<TabContainer>().GetCurrentTabControl() == this && _newsList.GetChildCount() == 0)
			RefreshNews();
	}
	
	#endregion
	
	#region Private Support Functions

	private void RefreshNews()
	{
		if (_newsList.GetChildCount() != 0)
			_newsList.QueueFreeChildren();

		var dlg = BusyDialog.FromScene();
		dlg.HeaderText = "Fetching News from Godot Project Website";
		dlg.BylineText = "Downloading...";
		GetTree().Root.AddChild(dlg);
		dlg.PopupCentered(new Vector2I(352, 150));
		// Fetch and Process News

		var news = new DownloadInstance(_newsUri);
		news.Failed += async () =>
		{
			dlg.CallDeferred("UpdateByline", "Download failed.");
			await Task.Delay(1500);
			dlg.QueueFree();
		};
		news.Cancelled += async () =>
		{
			dlg.CallDeferred("UpdateByline", "Fetching of News Cancelled.");
			await Task.Delay(1500);
			dlg.QueueFree();
		};
		news.Progress += (size, total) =>
		{
			dlg.CallDeferred("UpdateByline", $"Fetched {size} of {total} bytes");
		};
		news.Completed += (data) =>
		{
			dlg.CallDeferred("UpdateByline", "Parsing news entries...");
			var feed = ParseNews(data);

			foreach (var item in feed)
			{
				var newsItem = NewsItem.FromScene();
				newsItem.Headline = item["title"];
				newsItem.AuthorName = item["author"];
				newsItem.Date = item["date"].Replace("&nbsp;", " ");
				newsItem.Url = _baseUri.AbsoluteUri + item["link"].Substr(1,item["link"].Length);
				newsItem.Blerb = item["contents"];

				Uri uri = new Uri(item["image"]);
				string imgPath = Database.Settings.CachePath.Join("images", "news", uri.AbsolutePath.GetFile());
				if (!File.Exists(imgPath))
				{
					var dld = new ImageDownloader(uri);
					_downloads.Add(dld);
					newsItem.ImageDld = dld;
					dld.DownloadCompleted += (sender, s) =>
					{
						newsItem.Image = Util.LoadImage(s.GetOsDir().NormalizePath());
						UpdateQueue(dld, newsItem.ImageRect);
					};
					dld.DownloadCancelled += (sender, args) => UpdateQueue(dld, newsItem.ImageRect);
					dld.DownloadFailed += (sender, args) => UpdateQueue(dld, newsItem.ImageRect);
				}
				else
					newsItem.Image = Util.LoadImage(imgPath.GetOsDir().NormalizePath());

				uri = new Uri(item["avatar"]);
				imgPath = Database.Settings.CachePath.Join("images", "news", uri.AbsolutePath.GetFile());
				if (!File.Exists(imgPath))
				{
					var dld = new ImageDownloader(uri);
					_downloads.Add(dld);
					newsItem.AvatarDld = dld;
					dld.DownloadCompleted += (sender, s) =>
					{
						newsItem.Avatar = Util.LoadImage(s.GetOsDir().NormalizePath());
						UpdateQueue(dld, newsItem.AvatarRect);
					};
					dld.DownloadCancelled += (sender, args) => UpdateQueue(dld, newsItem.AvatarRect);
                    dld.DownloadFailed += (sender, args) => UpdateQueue(dld, newsItem.AvatarRect);
				}
				else
					newsItem.Avatar = Util.LoadImage(imgPath.GetOsDir().NormalizePath());

				_newsList.CallDeferred("add_child", newsItem);
			}

			CallDeferred("StartQueue");
			
			dlg.QueueFree();
		};
		
		news.StartDownload();
	}

	private List<System.Collections.Generic.Dictionary<string, string>> ParseNews(byte[] data)
	{
		var items = new List<System.Collections.Generic.Dictionary<string, string>>();

		var xml = new XmlParser();

		var err = xml.OpenBuffer(data);
		if (err != Error.Ok) return items;

		while (true)
		{
			err = xml.Read();
			if (err != Error.Ok)
			{
				if (err != Error.FileEof)
					GD.PrintErr($"Error {err} reading XML");
				break;
			}

			if (xml.GetNodeType() != XmlParser.NodeType.Element || xml.GetNodeName() != "article") continue;
			var tag_open_offset = xml.GetNodeOffset();
			xml.SkipSection();
			xml.Read();
			var tag_close_offset = xml.GetNodeOffset();
			items.Add(ParseNewsItem(data, tag_open_offset, tag_close_offset));
		}
		
		return items;
	}

	private System.Collections.Generic.Dictionary<string, string> ParseNewsItem(byte[] buffer, ulong start, ulong end)
	{
		var item = new System.Collections.Generic.Dictionary<string, string>();
		var xml = new XmlParser();
		var error = xml.OpenBuffer(buffer);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Error parsing news item. Error Code: {error}");
			return null;
		}

		xml.Seek(start);

		while (xml.GetNodeOffset() != end)
		{
			if (xml.GetNodeType() == XmlParser.NodeType.Element)
			{
				switch (xml.GetNodeName())
				{
					case "div":
						if (xml.GetNamedAttributeValueSafe("class").Contains("thumbnail"))
						{
							var image_style = xml.GetNamedAttributeValueSafe("style");
							var url_start = image_style.Find("'") + 1;
							var url_end = image_style.RFind("'")-1;
							var image_url = _baseUri.AbsoluteUri +
							                image_style.Substr(url_start + 1, url_end - url_start);

							item["image"] = image_url;
							item["link"] = xml.GetNamedAttributeValueSafe("href");
						}
						break;
					case "h3":
						xml.Read();
						item["title"] = xml.GetNodeType() == XmlParser.NodeType.Text
							? xml.GetNodeData().StripEdges()
							: "";
						
						break;
					case "span":
						if (xml.GetNamedAttributeValueSafe("class").Contains("date"))
						{
							xml.Read();
							item["date"] = xml.GetNodeType() == XmlParser.NodeType.Text ? xml.GetNodeData() : "";
						}

						if (xml.GetNamedAttributeValueSafe("class").Contains("by"))
						{
							xml.Read();
							item["author"] = xml.GetNodeType() == XmlParser.NodeType.Text
								? xml.GetNodeData().StripEdges()
								: "";
						}
						break;
					case "p":
						if (xml.GetNamedAttributeValueSafe("class").Contains("excerpt"))
						{
							xml.Read();
							item["contents"] = xml.GetNodeType() == XmlParser.NodeType.Text ? xml.GetNodeData() : "";
						}
						break;
					case "img":
						if (xml.GetNamedAttributeValueSafe("class").Contains("avatar"))
						{
							var part = xml.GetNamedAttributeValueSafe("src");
							item["avatar"] = _baseUri.AbsoluteUri + part.Substr(1, part.Length - 1);
						}
						break;
				}
			}

			xml.Read();
		}
		
		return item;
	}

	private void UpdateQueue(ImageDownloader dld, TextureRect rect)
	{
		GD.Print("Removing downloader");
		_downloads.Remove(dld);
		if (dld.Finished == false)
		{
			GD.Print("Download completed successfully.");
			rect.Texture = GD.Load<Texture2D>("res://Assets/Icons/svg/missing_icon.svg");
		}
		else
		{
			GD.Print("Download failed or Cancelled.");
		}
		
		foreach (var item in _downloads)
		{
			if (item.Started) continue;
			item.DownloadImage();
			GD.Print("Started next download.");
			return;
		}
	}

	private void StartQueue()
	{
		GD.Print("Starting Queue...");
		GD.Print($"Queue Size: {_downloads.Count}");
		for (var i = 0; i < 3; i++)
		{
			if (i < _downloads.Count - 1)
			{
				_downloads[i].DownloadImage();
				GD.Print($"Started {i} in queue");
			}
		}
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}
