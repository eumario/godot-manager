using System.Linq;
using Godot;
using Godot.Sharp.Extras;

// namespace
namespace GodotManager.Library.Components.Controls;

public partial class PaginationNav : CenterContainer
{
	#region Signals
	[Signal] public delegate void PageChangedEventHandler(int page);
	#endregion
	
	#region Node Paths
	[NodePath] private Button _firstPage;
	[NodePath] private Button _prevPage;
	[NodePath] private HBoxContainer _pageCount;
	[NodePath] private Button _nextPage;
	[NodePath] private Button _lastPage;
	#endregion
	
	#region Private Variables
	private int _totalPages = 0;
	private int _currentPage = 0;
	#endregion
	
	#region Public Variables
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		_firstPage.Pressed += () => StepPage(0);
		_prevPage.Pressed += () => StepPage(-1);
		_nextPage.Pressed += () => StepPage(1);
		_lastPage.Pressed += () => StepPage(-2);
	}
	#endregion
	
	#region Event Handlers

	public void StepPage(int page)
	{
		_pageCount.GetChild<Button>(_currentPage).Disabled = false;
		if (page == 0)
			_currentPage = 0;
		else if (page == -2)
			_currentPage = _pageCount.GetChildCount() - 1;
		else
			_currentPage += page;
		_pageCount.GetChild<Button>(_currentPage).Disabled = true;
		CheckPage();
		EmitSignal(SignalName.PageChanged, _currentPage);
	}

	private void HandlePageChanged(int i)
	{
		var childCount = _pageCount.GetChildCount();
		if (childCount > 0 && childCount > _currentPage)
			_pageCount.GetChild<Button>(_currentPage).Disabled = false;

		_currentPage = i;
		if (childCount > 0 && childCount > _currentPage)
			_pageCount.GetChild<Button>(_currentPage).Disabled = true;
		CheckPage();
		EmitSignal(SignalName.PageChanged, i);
	}
	#endregion
	
	#region Private Support Functions

	private void CheckPage()
	{
		var childCount = _pageCount.GetChildCount() - 1;
		_firstPage.Disabled = _currentPage == 0;
		_prevPage.Disabled = _currentPage == 0;
		_lastPage.Disabled = _currentPage == childCount;
		_prevPage.Disabled = _currentPage == childCount;

		var from = _currentPage - 5 < 0 ? 0 : _currentPage - 5;
		var to = from + 9 > childCount ? childCount : from + 9;
		GetTree().SetGroupFlags(0,$"{Name}_navButtons", "visible", false);
		
		for (var i = from; i < to; i++)
			_pageCount.GetNode<Button>($"Nav{i}").Visible = true;
	}
	#endregion
	
	#region Public Support Functions

	public void UpdateConfig(int totalPages)
	{
		if (_totalPages == totalPages) return;
		_totalPages = totalPages;
		foreach (var child in _pageCount.GetChildren())
			child.QueueFree();

		for (var i = 0; i < _totalPages; i++)
		{
			var page = i;
			var btn = new Button();
			btn.Name = $"Nav{page}";
			btn.Text = $"{page + 1}";
			btn.Size = new Vector2(25, 0);
			btn.Pressed += () => HandlePageChanged(page);
			btn.Flat = true;
			btn.AddToGroup($"{Name}_navButtons");
			_pageCount.AddChild(btn);
			if (i > 9)
			{
				btn.Visible = false;
			}
		}

		if (totalPages > 0)
		{
			_currentPage = 0;
			_pageCount.GetChild<Button>(0).Disabled = true;
			CheckPage();
		}
	}

	public void SetPage(int page)
	{
		_pageCount.GetNode<Button>($"Nav{_currentPage}").Disabled = false;
		_pageCount.GetNode<Button>($"Nav{page}").Disabled = true;
		_currentPage = page;
		CheckPage();
	}
	#endregion
}
