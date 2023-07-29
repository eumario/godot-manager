using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Components.Controls;
using GodotManager.Library.Components.Dialogs;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using Octokit;

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
	[NodePath] private GridContainer _gridView;
	[NodePath] private VBoxContainer _categoryView;
	
	#endregion
	
	#region Private Variables

	private Dictionary<int, CategoryList> _categories;
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
		PopulateLists();
	}
	#endregion

	#region Event Handlers
	private void OnButtonClicked_ActionButtons(int index)
	{
		switch (index)
		{
			case 0: // New Project
				var np = CreateProject.FromScene();
				AddChild(np);
				np.PopupCentered(new Vector2I(600,470));
				break;
			case 1: // Import Project
				var dlg = ImportProject.FromScene();
				dlg.ImportCompleted += () =>
				{
					PopulateLists();
					dlg.QueueFree();
				};
				AddChild(dlg);
				dlg.PopupCentered(new Vector2I(320,145));
				break;
			case 2: // Scan Folders
				break;
			case 3: // Add Category
				break;
			case 4: // Remove Category
				break;
			case 5: // Delete
				break;
			case 6: // Remove Missing
				break;
		}
	}

	void OnViewChanged_ViewToggleButtons(int index)
	{
		_sorter.Visible = index == 0;
		GetTree().SetGroup("views", "visible", false);
		switch (index)
		{
			case 0: // ListView
				_listView.Visible = true;
				break;
			case 1: // GridView
				_gridView.Visible = true;
				break;
			case 2: // CategoryView
				_categoryView.Visible = true;
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

	private void PopulateLists()
	{
		if (_categoryView.GetChildCount() == 0)
			SetupCategoryView();
		
		foreach (var project in Database.AllProjects())
		{
			var pli = ProjectLineItem.FromScene();
			pli.ProjectFile = project;
			pli.FavoriteClicked += OnFavClicked_ProjectLineItem;
			_listView.AddChild(pli);
			pli = ProjectLineItem.FromScene();
			pli.ProjectFile = project;
			pli.FavoriteClicked += OnFavClicked_ProjectLineItem;
			if (project.Category is null)
			{
				// Add to Uncategorized / Favorites
				if (project.Favorite)
					_categories[Favorites].ItemList.AddChild(pli);
				else
					_categories[Uncategorized].ItemList.AddChild(pli);
			}
			else
			{
				// Add to Category
				_categories[project.Category.Id].ItemList.AddChild(pli);
			}
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
			foreach (var item in _categories[category].ItemList.GetChildren())
			{
				if (item is not ProjectLineItem pli) continue;
				_categories[category].ItemList.RemoveChild(pli);
				items.Add(pli);
			}
		}

		foreach (var pli in items)
		{
			if (pli.ProjectFile.Category is null)
			{
				if (pli.ProjectFile.Favorite)
					_categories[Favorites].ItemList.AddChild(pli);
				else
					_categories[Uncategorized].ItemList.AddChild(pli);
			}
			else
			{
				if (_categories.TryGetValue(pli.ProjectFile.Category.Id, out var category))
					category.ItemList.AddChild(pli);
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