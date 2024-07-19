using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
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
	private readonly Uri _newsUri = new Uri("https://godotengine.org/rss.json");

	private readonly Uri _authorUri =
		new Uri("https://raw.githubusercontent.com/godotengine/godot-website/master/_data/authors.yml");
	private readonly Uri _baseUri = new Uri("https://godotengine.org");
	private List<ImageDownloader> _downloads;

	private Callable _updateByline;
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
		_updateByline = Callable.From((BusyDialog dlg, string arg) => dlg.UpdateByline(arg));
	}

	public override void _ExitTree()
	{
		foreach (var child in _newsList.GetChildren())
		{
			_newsList.RemoveChild(child);
			child.QueueFree();
		}
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
		dlg.HeaderText = "Fetching author information from Godot Project Website";
		dlg.BylineText = "Downloading...";
		GetTree().Root.AddChild(dlg);
		dlg.PopupCentered(new Vector2I(352, 150));

		var authors = new DownloadInstance(_authorUri);
		authors.Failed += async () =>
		{
			_updateByline.CallDeferred(dlg, "Download failed.");
			await Task.Delay(1500);
			dlg.QueueFree();
		};
		authors.Cancelled += async () =>
		{
			_updateByline.CallDeferred(dlg, "Fetching of News Cancelled.");
			await Task.Delay(1500);
			dlg.QueueFree();
		};
		authors.Progress += (size, total) =>
		{
			_updateByline.CallDeferred(dlg, $"Fetched {size} of {total} bytes");
		};
		authors.Completed += (data) =>
		{
			var entries = data.GetStringFromUtf8().Split("\n");
			var author = new AuthorEntry();
			foreach (var (line, lineNo) in entries.WithIndex())
			{
				_updateByline.CallDeferred(dlg, $"Parsing {lineNo} of {entries.Length}");
				if (line.StartsWith("- name: "))
					author.Name = line.Replace("- name: ", "").Replace("\"", "").Replace("'", "");

				if (line.StartsWith("  image: "))
					author.AvatarUrl = line.Replace("  image: ", "");

				if (!author.HasAll()) continue;
				if (!Database.HasAuthor(author.Name))
					Database.AddAuthor(author);
				author = new AuthorEntry();
			}

			FetchNews(dlg);
		};
		
		authors.StartDownload();
	}

	private void FetchNews(BusyDialog dlg)
	{
		Callable.From(() => dlg.UpdateHeader("Fetching news items from RSS Feed...")).CallDeferred();
		_updateByline.CallDeferred(dlg, "Downloading...");

		var news = new DownloadInstance(_newsUri);
		news.Failed += async () =>
		{
			_updateByline.CallDeferred(dlg, "Download failed.");
			await Task.Delay(1500);
			dlg.QueueFree();
		};
		news.Cancelled += async () =>
		{
			_updateByline.CallDeferred(dlg, "Fetching of News Cancelled.");
			await Task.Delay(1500);
			dlg.QueueFree();
		};
		news.Progress += (size, total) =>
		{
			_updateByline.CallDeferred(dlg, $"Fetched {size} of {total} bytes");
		};
		news.Completed += async (bytes) =>
		{
			_updateByline.CallDeferred(dlg, "Parsing Entries...");

			var data = Json.ParseString(bytes.GetStringFromUtf8()).AsGodotDictionary();
			if (!data.ContainsKey("title"))
			{
				_updateByline.CallDeferred(dlg, "Failed to parse RSS Stream...");
				await Task.Delay(1500);
				dlg.QueueFree();
				return;
			}

			foreach (var item in data["items"].AsGodotArray())
			{
				var nitem = item.AsGodotDictionary();
				var newsItem = NewsItem.FromScene();

				newsItem.Headline = nitem["title"].AsString();
				newsItem.AuthorName = nitem["dc:creator"].AsString();
				newsItem.Date = nitem["pubDate"].AsString();
				newsItem.Url = nitem["guid"].AsString();
				newsItem.Blerb = nitem["description"].AsString();

				Uri uri = new Uri(nitem["image"].AsString());
				string imgPath = Database.Settings.CachePath.Join("images", "news", uri.AbsolutePath.GetFile());
				if (!File.Exists(imgPath))
				{
					var dld = new ImageDownloader(uri);
					_downloads.Add(dld);
					newsItem.ImageDld = dld;
					dld.DownloadCompleted += (_, locPath) =>
					{
						Util.RunInMainThread(() =>
						{
							newsItem.Image = Util.LoadImage(locPath.GetOsDir().NormalizePath());
							UpdateQueue(dld, newsItem.ImageRect);
						});
					};
					dld.DownloadCancelled += (_, _) => UpdateQueue(dld, newsItem.ImageRect);
					dld.DownloadFailed += (_, _) => UpdateQueue(dld, newsItem.ImageRect);
				}
				else
					newsItem.Image = Util.LoadImage(imgPath.GetOsDir().NormalizePath());

				var avatarUrl = Database.GetAuthorI(newsItem.AuthorName) ?? Database.GetAuthor("default");
				if (avatarUrl is null)
				{
					Callable.From(() => _newsList.AddChild(newsItem)).CallDeferred();
					continue;
				}
				
				uri = new Uri(_baseUri, avatarUrl.AvatarUrl);
				imgPath = Database.Settings.CachePath.Join("images", "news", uri.AbsolutePath.GetFile());
				if (!File.Exists(imgPath))
				{
					if (_downloads.All(x => x.Tag != avatarUrl.Name))
					{
						var dld = new ImageDownloader(uri, avatarUrl.Name);
						_downloads.Add(dld);
						newsItem.AvatarDld = dld;
						dld.DownloadCompleted += (_, pathLoc) =>
						{
							Util.RunInMainThread(() =>
							{
								var img = Util.LoadImage(pathLoc.GetOsDir().NormalizePath());
								newsItem.Avatar = img;
								UpdateQueue(dld, newsItem.AvatarRect);
							});
						};
						dld.DownloadCancelled += (_, _) => UpdateQueue(dld, newsItem.AvatarRect);
						dld.DownloadFailed += (_, _) => UpdateQueue(dld, newsItem.AvatarRect);	
					}
					else
					{
						var dld = _downloads.FirstOrDefault(x => x.Tag == avatarUrl.Name);
						dld.DownloadCompleted += (_, pathLoc) =>
						{
							Util.RunInMainThread(() =>
							{
								var img = Util.LoadImage(pathLoc.GetOsDir().NormalizePath());
								newsItem.Avatar = img;
							});
						};
					}
				}
				else
					newsItem.Avatar = Util.LoadImage(imgPath.GetOsDir().NormalizePath());
				Callable.From(() => _newsList.AddChild(newsItem)).CallDeferred();
			}

			Callable.From(StartQueue).CallDeferred();
			dlg.QueueFree();
		};
		
		news.StartDownload();
	}

	private void UpdateQueue(ImageDownloader dld, TextureRect rect)
	{
		_downloads.Remove(dld);
		if (dld.Finished == false)
		{
			GD.Print("Download failed or Cancelled.");
			rect.Texture = GD.Load<Texture2D>("res://Assets/Icons/svg/missing_icon.svg");
		}
		
		dld.Dispose();
		
		foreach (var item in _downloads)
		{
			if (item.Started) continue;
			item.DownloadImage();
			return;
		}
	}

	private void StartQueue()
	{
		GD.Print("Starting Queue...");
		GD.Print($"Queue Size: {_downloads.Count}");
		for (var i = 0; i < 3; i++)
		{
			if (i >= _downloads.Count - 1) continue;
			_ = _downloads[i].DownloadImage();
			GD.Print($"Started {i} in queue");
		}
	}
	#endregion
	
	#region Public Support Functions
	#endregion
}