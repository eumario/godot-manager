using System.Collections.Generic;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data;

namespace GodotManager.Scenes;

public partial class MainWindow : Control
{
	private static MainWindow? _instance;
	private List<Button> _pageButtons = new();
	[NodePath] private PanelContainer? _sidebar;
	[NodePath] private Button? _menu;
	[NodePath] private Button? _minimize;
	[NodePath] private Button? _restMax;
	[NodePath] private Button? _close;
	[NodePath] private TabContainer? _panels;
	[NodePath] private Library.Components.Controls.DownloadArea? _downloadArea;

	[Resource("res://Assets/Icons/svg/restore.svg")]
	private Texture2D? _textureRestore;

	[Resource("res://Assets/Icons/svg/maximize.svg")]
	private Texture2D? _textureMaximize;

	public override void _Ready()
	{
		this.OnReady();
		_instance = this;
		if (Database.Settings.FirstTimeRun)
		{
			// Launch First Time Wizard
			GD.Print("First Time Wizard here...");
		}
		
		_pageButtons = new();
		foreach (var groupBtn in GetTree().GetNodesInGroup("page_buttons"))
		{
			Button button = groupBtn as Button;
			if (button is null) continue;
			_pageButtons.Add(button);
			button.Pressed += () => OnPageButton_Pressed(button);
			if (button.Name == "Projects") button.ButtonPressed = true;
		}

		_menu.Pressed += () =>
		{
			var tween = GetTree().CreateTween();
			var rectSize = _sidebar.CustomMinimumSize;
			rectSize.X = (rectSize.X.Equals(70.0f)) ? 200.0f : 70.0f;
			tween.TweenProperty(_sidebar, "custom_minimum_size", rectSize, 0.25f);
			rectSize.Y = 60.0f;
			tween.TweenProperty(_menu, "custom_minimum_size", rectSize, 0.25f);
		};

		_restMax.Icon = _textureMaximize;

		var winId = DisplayServer.MainWindowId;
		_minimize.Pressed += () => DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized, (int)winId);
		_restMax.Pressed += () =>
		{
			var mode = DisplayServer.WindowGetMode((int)winId);
			var icon = mode == DisplayServer.WindowMode.Windowed ? _textureRestore : _textureMaximize;
			mode = mode == DisplayServer.WindowMode.Windowed
				? DisplayServer.WindowMode.Maximized
				: DisplayServer.WindowMode.Windowed;
			DisplayServer.WindowSetMode(mode, (int)winId);
			_restMax.Icon = icon;
		};
		_close.Pressed += () => GetTree().Quit();
		
		GetTree().SetGroup("window_management", "visible", !Database.Settings.UseSystemTitlebar);
		GetWindow().Borderless = !Database.Settings.UseSystemTitlebar;
	}

	public bool IsSidebarExpanded() => _sidebar.CustomMinimumSize.X.Equals(200.0f);

	void OnPageButton_Pressed(Button button)
	{
		if (button is null) return;
		int index = _pageButtons.IndexOf(button);
		if (index == -1)
			return;

		foreach (var groupBtn in GetTree().GetNodesInGroup("page_buttons"))
		{
			var pageButton = groupBtn as Button;
			if (pageButton != button && pageButton is not null)
				pageButton.ButtonPressed = false;
		}

		button.ButtonPressed = true;
		_panels.CurrentTab = index;
	}

	public static void BrowseFolderDialog(string title, string path, string file, bool showHidden,
		DisplayServer.FileDialogMode mode, string[] filters, Callable callback) =>
		_instance.ShowBrowseFolderDialog(title, path, file, showHidden, mode, filters, callback);

	private void ShowBrowseFolderDialog(string title, string path, string file, bool showHidden,
		DisplayServer.FileDialogMode mode, string[] filters, Callable callback) =>
		DisplayServer.FileDialogShow(title, path, file, showHidden, mode, filters, callback);
}
