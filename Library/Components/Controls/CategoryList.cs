using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.Internal;
using Octokit;
using Label = Godot.Label;

namespace GodotManager.Library.Components.Controls;

[Tool]
public partial class CategoryList : VBoxContainer
{
	#region Signals
	[Signal] public delegate void ListToggledEventHandler(CategoryList cl);
	[Signal] public delegate void PinToggledEventHandler(CategoryList cl);
	[Signal] public delegate void DragDropCompletedEventHandler(CategoryList origin, CategoryList destination, ProjectLineItem project);
	#endregion
	
	#region Quick Create
	public static CategoryList FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Controls/CategoryList.tscn");
		return scene.Instantiate<CategoryList>();
	}
	#endregion
	
	#region Node Paths
	[NodePath] private Label Header = null;
	[NodePath] private Button Pin = null;
	[NodePath] private Button ExpandToggle = null;
	[NodePath] private VBoxContainer List = null;
	
	[Resource("res://Assets/Icons/svg/drop_down1.svg")]
	private Texture2D _expanded;
	[Resource("res://Assets/Icons/svg/drop_down2.svg")]
	private Texture2D _collapsed;
	#endregion

	#region Private Variables

	private Category _category;
	private bool _pinnable = false;
	private bool _toggable = false;
	private string _title;
	#endregion
	
	#region Public Properties

	public Category Category
	{
		get => _category;
		set
		{
			_category = value;
			if (Header is null || _category is null)
				return;
			Header.Text = value.Name;
			Expanded = value.IsExpanded;
			Pinned = value.IsPinned;
		}
	}

	[Export]
	public string Title
	{
		get => _title;
		set
		{
			_title = value;
			if (Header is null)
				return;
			Header.Text = _title;
		}
	}

	public bool Expanded
	{
		get => _category?.IsExpanded ?? false;
		set
		{
			if (_category == null)
				return;
			_category.IsExpanded = value;
			if (ExpandToggle is null)
				return;
			ExpandToggle.ButtonPressed = value;
			ExpandToggle.Icon = value ? _expanded : _collapsed;
		}
	}

	[Export]
	public bool Pinnable
	{
		get => _pinnable;
		set
		{
			_pinnable = value;
			if (Pin is not null)
				Pin.Visible = value;
		}
	}

	public bool Pinned
	{
		get => _category?.IsPinned ?? false;
		set
		{
			if (_category is null)
				return;
			
			_category.IsPinned = value;
			if (Pin is null)
				return;
			Pin.ButtonPressed = value;
			Pin.SelfModulate = value ? Colors.Green : Colors.White;
		}
	}

	[Export]
	public bool Toggable
	{
		get => _toggable;
		set
		{
			_toggable = value;
			if (ExpandToggle is not null)
				ExpandToggle.Visible = value;
		}
	}

	public VBoxContainer ItemList => List;

	public ProjectLineItem Selected
	{
		get
		{
			foreach (ProjectLineItem pli in GetChildren())
			{
				if (pli.SelfModulate == Colors.White)
					return pli;
			}

			return null;
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();

		Category = _category;
		Toggable = _toggable;
		Pinnable = _pinnable;
		if (_category is null)
			Title = _title;
		ExpandToggle.Pressed += () =>
		{
			Expanded = ExpandToggle.ButtonPressed;
			List.Visible = ExpandToggle.ButtonPressed;
		};
		Pin.Pressed += () => Pinned = Pin.ButtonPressed;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		var dictData = data.AsGodotDictionary<string, Node>();
		var parent = dictData["parent"] as CategoryList;
		if (parent == this)
			return false;
		return dictData["source"] is ProjectLineItem;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		var dictData = data.AsGodotDictionary<string, Node>();
		var parent = dictData["parent"] as CategoryList;
		var pli = dictData["source"] as ProjectLineItem;
		if (pli is null || parent is null)
			return;
		parent.List.RemoveChild(pli);
		List.AddChild(pli);
		pli.ProjectFile.Category = Category;
		EmitSignal(SignalName.DragDropCompleted, parent, this, pli);
		parent.SortListing();
		SortListing();
	}

	#endregion

	#region Public Methods

	public async void SortListing()
	{
		await this.IdleFrame();
		var pliCache = new List<ProjectLineItem>();
		var pfCache = new List<ProjectFile>();

		foreach (ProjectLineItem pli in List.GetChildren())
		{
			pliCache.Add(pli);
			pfCache.Add(pli.ProjectFile);
			List.RemoveChild(pli);
		}

		var fav = pfCache.Where(pf => pf.Favorite)
			.OrderByDescending(pf => pf.LastAccessed);

		var non_fav = pfCache.Where(pf => !pf.Favorite)
			.OrderByDescending(pf => pf.LastAccessed);

		foreach (IOrderedEnumerable<ProjectFile> apf in new ArrayList() { fav, non_fav })
		{
			foreach (ProjectFile pf in apf)
			{
				int indx = pfCache.IndexOf(pf);
				if (indx == -1)
					continue;
				List.AddChild(pliCache[indx]);
			}
		}
	}
	#endregion
	
	#region Private Methods
	#endregion
}