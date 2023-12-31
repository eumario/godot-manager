using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Data.UI;
using GodotManager.Library.Enumerations;
using GodotManager.Library.Managers;
using GodotManager.Library.Utility;

// namespace
namespace GodotManager.Library.Components.Panels;

public partial class ProjectsPanel : Panel
{
	#region Signals
	#endregion
	
	#region Node Paths
	[NodePath] private ActionButtons _actionButtons;
	[NodePath] private ViewToggleButtons _viewToggleButtons;

	[NodePath] private MarginContainer _sorter;
	[NodePath] private HeaderButton _projectName;
	[NodePath] private HeaderButton _godotVersion;
	
	[NodePath] private VBoxContainer _listView;
	[NodePath] private HFlowContainer _gridView;
	[NodePath] private VBoxContainer _categoryView;
	#endregion
	
	#region Private Variables

	private Dictionary<int, CategoryList> _categories;
	private Dictionary<ProjectFile, ProjectNodeCache> _projectCache;
	private const int Favorites = -2;
	private const int Uncategorized = -1;
	#endregion
	
	#region Public Variables
	#endregion
	
	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		_actionButtons.ButtonClicked += OnButtonClicked_ActionButtons;
		_viewToggleButtons.ViewChanged += OnViewChanged_ViewToggleButtons;

		_categories = new Dictionary<int, CategoryList>();
		_actionButtons.SetHidden(3,4,5,6);

		_projectCache = new();
		
		PopulateViews();
		GetWindow().FilesDropped += OnFilesDropped;
	}
	#endregion

	#region Event Handlers
	private void OnButtonClicked_ActionButtons(int index)
	{
		switch ((ProjectActions)index)
		{
			case ProjectActions.NewProject:
				var np = CreateProject.FromScene();
				AddChild(np);
				np.PopupCentered(new Vector2I(600,470));
				break;
			case ProjectActions.ImportProject:
				OnImport();
				break;
			case ProjectActions.ScanFolders:
				break;
			case ProjectActions.AddCategory:
				break;
			case ProjectActions.RemoveCategory:
				break;
			case ProjectActions.DeleteProject:
				break;
			case ProjectActions.RemoveMissing:
				break;
		}
	}

	void OnFilesDropped(string[] files)
	{
		OnImport(files[0]);
	}

	void OnImport(string path = null)
	{
		var dlg = ImportProject.FromScene();
		dlg.ImportCompleted += () =>
		{
			PopulateViews();
			dlg.QueueFree();
		};
		AddChild(dlg);
		if (path != null) dlg.SetLocation(path);
		dlg.PopupCentered(new Vector2I(320,145));
	}

	void OnViewChanged_ViewToggleButtons(int index)
	{
		_sorter.Visible = index == 0;
		GetTree().SetGroup("views", "visible", false);
		switch ((ViewToggle)index)
		{
			case ViewToggle.ListView:
				_actionButtons.SetHidden(3,4,5);
				_listView.Visible = true;
				break;
			case ViewToggle.GridView:
				_actionButtons.SetHidden(3,4,5);
				_gridView.Visible = true;
				break;
			case ViewToggle.CategoryView:
				_categoryView.Visible = true;
				_actionButtons.SetVisible(3,4,5);
				break;
		}
	}
	
	private void OnFavClicked_ProjectLineItem(ProjectLineItem item, bool value)
	{
		item.ProjectFile.Favorite = value;
		Database.UpdateProject(item.ProjectFile);

		ResortListView();
		ResortCategoryView();
	}
	
	#endregion
	
	#region Private Support Functions
	private void SetupCategoryView()
	{
		foreach (var category in Database.AllCategories())
		{
			var cl = CategoryList.FromScene();
			cl.Category = category;
			_categories[category.Id] = cl;
		}
		
		foreach(var cat in _categories.Where(kv => kv.Value.Pinned))
			_categoryView.AddChild(cat.Value);
		
		foreach(var cat in _categories.Where(kv => !kv.Value.Pinned))
			_categoryView.AddChild(cat.Value);

		_categories[Uncategorized] = CategoryList.FromScene();
		_categories[Uncategorized].Category = new Category() {
			Id = Uncategorized,
			IsExpanded = true,
			IsPinned = false,
			LastAccessed = DateTime.UtcNow,
			Name = "Uncategorized"
		};
		_categories[Uncategorized].Toggable = true;
		_categories[Favorites] = CategoryList.FromScene();
		_categories[Favorites].Category = new Category()
		{
			Id = Favorites,
			IsExpanded = true,
			IsPinned = false,
			LastAccessed = DateTime.UtcNow,
			Name = "Favorites"
		};
		_categories[Favorites].Toggable = true;
		_categoryView.AddChild(_categories[Favorites]);
		_categoryView.AddChild(_categories[Uncategorized]);
	}

	private void SetupPliEvents(ProjectLineItem pli)
	{
		pli.FavoriteClicked += OnFavClicked_ProjectLineItem;
		pli.RightClicked += item => item.ShowContextMenu();
		pli.DoubleClicked += item => GodotRunner.EditProject(item.GodotVersion, item.ProjectFile);
		pli.ContextMenuClick += PliOnContextMenuClick;
	}

	private void SetupPiiEvents(ProjectIconItem pii)
	{
		//pii.FavoriteClicked += OnFavClicked_ProjectIconItem;
		pii.RightClicked += item => item.ShowContextMenu();
		pii.DoubleClicked += item => GodotRunner.EditProject(item.GodotVersion, item.ProjectFile);
		pii.ContextMenuClick += PiiOnContextMenuClick;
	}

	private void PopulateViews()
	{
		if (_categoryView.GetChildCount() == 0)
			SetupCategoryView();

		ClearAllViews();

		foreach (var project in Database.AllProjects())
		{
			var cache = new ProjectNodeCache
			{
				ProjectFile = project
			};
			var pii = ProjectIconItem.FromScene();
			pii.ProjectFile = project;
			SetupPiiEvents(pii);
			_gridView.AddChild(pii);
			cache.GridView = pii;
			
			var pli = ProjectLineItem.FromScene();
			pli.ProjectFile = project;
			SetupPliEvents(pli);
			_listView.AddChild(pli);
			cache.ListView = pli;
			
			pli = ProjectLineItem.FromScene();
			pli.ProjectFile = project;
			SetupPliEvents(pli);
			if (project.Category is null)
			{
				// Add to Uncategorized / Favorites
				if (project.Favorite)
					_categories[Favorites].AddLineItem(pli);
				else
					_categories[Uncategorized].AddLineItem(pli);
			}
			else
			{
				// Add to Category
				_categories[project.Category.Id].AddLineItem(pli);
			}

			cache.CategoryView = pli;
			_projectCache[project] = cache;
		}
	}

	private void ClearAllViews()
	{
		foreach (var category in _categoryView.GetChildren<CategoryList>())
			category.ClearFree();

		foreach (var pii in _gridView.GetChildren())
			pii.QueueFree();

		foreach (var pli in _listView.GetChildren())
			pli.QueueFree();

		_projectCache.Clear();
	}

	private async void PiiOnContextMenuClick(ProjectIconItem pii, ContextMenuItem id)
	{
		switch (id)
		{
			case ContextMenuItem.Open:
				GodotRunner.EditProject(pii.GodotVersion, pii.ProjectFile);
				break;
			case ContextMenuItem.Run:
				GodotRunner.RunProject(pii.GodotVersion, pii.ProjectFile);
				break;
			case ContextMenuItem.ProjectFiles:
				OS.ShellOpen($"\"{pii.ProjectFile.Location.GetBaseDir().GetOsDir().NormalizePath()}\"");
				break;
			case ContextMenuItem.DataFolder:
				OS.ShellOpen($"\"{pii.ProjectFile.DataFolder.GetOsDir().NormalizePath()}\"");
				break;
			case ContextMenuItem.EditProject:
				var epd = EditProjectDialog.FromScene();
				epd.ProjectCache = _projectCache[pii.ProjectFile];
				GetTree().Root.AddChild(epd);
				epd.SaveProject += (cache) =>
				{
					Database.UpdateProject(cache.ProjectFile);
					cache.GridView.ProjectFile = cache.ProjectFile;
					cache.ListView.ProjectFile = cache.ProjectFile;
					cache.CategoryView.ProjectFile = cache.ProjectFile;
					cache.ProjectFile.SaveUpdatedData();
				};
				epd.PopupCentered();
				break;
			case ContextMenuItem.RemoveProject:
				var res = await UI.YesNoBox("Remove Project", $"Are you sure you want to remove {pii.ProjectFile.Name}?");
				if (!res) return;
				res = await UI.YesNoBox("Remove Project", $"Do you want to delete the files from disk as well?");
				if (res)
				{
					Directory.Delete(pii.ProjectFile.Location.GetBaseDir().GetOsDir().NormalizePath(), true);
				}

				Database.RemoveProject(pii.ProjectFile);
				Database.FlushDatabase();

				var cache = _projectCache[pii.ProjectFile];
				_projectCache.Remove(pii.ProjectFile);
				cache.QueueFree();
				break;
		}
	}

	private async void PliOnContextMenuClick(ProjectLineItem pli, ContextMenuItem id)
	{
		switch (id)
		{
			case ContextMenuItem.Open:
				GodotRunner.EditProject(pli.GodotVersion, pli.ProjectFile);
				break;
			case ContextMenuItem.Run:
				GodotRunner.RunProject(pli.GodotVersion, pli.ProjectFile);
				break;
			case ContextMenuItem.ProjectFiles:
				OS.ShellOpen($"\"{pli.ProjectFile.Location.GetBaseDir().GetOsDir().NormalizePath()}\"");
				break;
			case ContextMenuItem.DataFolder:
				OS.ShellOpen($"\"{pli.ProjectFile.DataFolder.GetOsDir().NormalizePath()}\"");
				break;
			case ContextMenuItem.EditProject:
				var epd = EditProjectDialog.FromScene();
				epd.ProjectCache = _projectCache[pli.ProjectFile];
				GetTree().Root.AddChild(epd);
				epd.PopupCentered();
				epd.SaveProject += (cache) =>
				{
					Database.UpdateProject(cache.ProjectFile);
					cache.GridView.ProjectFile = cache.ProjectFile;
					cache.ListView.ProjectFile = cache.ProjectFile;
					cache.CategoryView.ProjectFile = cache.ProjectFile;
					cache.ProjectFile.SaveUpdatedData();
				};
				break;
			case ContextMenuItem.RemoveProject:
				var res = await UI.YesNoBox("Remove Project", $"Are you sure you want to remove {pli.ProjectFile.Name}?");
				if (!res) return;
				res = await UI.YesNoBox("Remove Project", $"Do you want to delete the files from disk as well?");
				if (res)
				{
					Directory.Delete(pli.ProjectFile.Location.GetBaseDir().GetOsDir().NormalizePath(), true);
				}

				Database.RemoveProject(pli.ProjectFile);
				Database.FlushDatabase();

				var cache = _projectCache[pli.ProjectFile];
				_projectCache.Remove(pli.ProjectFile);
				cache.QueueFree();
				break;
		}
	}

	private void ResortProjectLineItems(Container container, bool descending = false)
	{
		var list = new List<ProjectLineItem>();
		foreach (var child in container.GetChildren())
		{
			if (child is not ProjectLineItem pli) continue;
			list.Add(pli);
			container.RemoveChild(pli);
		}

		var fav = list.Where(pli => pli.ProjectFile.Favorite).ToList();
		var nonFav = list.Where(pli => !pli.ProjectFile.Favorite).ToList();
		var linq = descending
			? fav.OrderByDescending(n => n.ProjectFile.LastAccessed).ToList()
			: fav.OrderBy(n => n.ProjectFile.LastAccessed).ToList();

		foreach (var node in linq)
			container.AddChild(node);

		linq = descending
			? nonFav.OrderByDescending(n => n.ProjectFile.LastAccessed).ToList()
			: nonFav.OrderBy(n => n.ProjectFile.LastAccessed).ToList();
		foreach(var node in linq)
			container.AddChild(node);
	}

	private void ResortListView()
	{
		ResortProjectLineItems(_listView, false);
	}

	private void ResortCategoryView()
	{
		var items = new List<ProjectLineItem>();
		foreach (var category in _categories.Keys)
		{
			foreach (var item in _categories[category].GetProjectLineItems())
			{
				_categories[category].RemoveLineItem(item);
				items.Add(item);
			}
		}

		foreach (var pli in items)
		{
			if (pli.ProjectFile.Category is null)
			{
				if (pli.ProjectFile.Favorite)
					_categories[Favorites].AddLineItem(pli);
				else
					_categories[Uncategorized].AddLineItem(pli);
			}
			else
			{
				if (_categories.TryGetValue(pli.ProjectFile.Category.Id, out var category))
					category.AddLineItem(pli);
			}
		}

		foreach (var category in _categories.Keys)
		{
			ResortProjectLineItems(_categories[category].ItemList, false);
		}
	}

	private void ResortGridView()
	{
		
	}

	#endregion

	#region Public Support Functions

	#endregion
}