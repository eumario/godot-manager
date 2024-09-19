using System.IO;
using Godot;
using Godot.Sharp.Extras;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Enumerations;
using GodotManager.Library.Utility;

namespace GodotManager.Library.Components.Controls;

public partial class ProjectIconItem : Control, IProjectIcon
{
    
    #region Signals
    [Signal] public delegate void FavoriteClickedEventHandler(ProjectIconItem pii, bool value);
    [Signal] public delegate void ClickedEventHandler(ProjectIconItem pii);
    [Signal] public delegate void RightClickedEventHandler(ProjectIconItem pii);
    [Signal] public delegate void DoubleClickedEventHandler(ProjectIconItem pii);
    [Signal] public delegate void RightDoubleClickedEventHandler(ProjectIconItem pii);
    [Signal] public delegate void DragStartedEventHandler(ProjectIconItem pii);
    [Signal] public delegate void DragEndedEventHandler(ProjectIconItem pii);
    [Signal] public delegate void ContextMenuClickEventHandler(ProjectIconItem pii, ContextMenuItem id);
    #endregion
    
    #region Quick Create
    public static ProjectIconItem FromScene()
    {
        var scene = GD.Load<PackedScene>("res://Library/Components/Controls/ProjectIconItem.tscn");
        return scene.Instantiate<ProjectIconItem>();
    }
    #endregion
    
    #region Node Paths

    [NodePath] private ColorRect _hover;
    [NodePath] private TextureRect _projectIcon;
    [NodePath] private Label _projectName;
    [NodePath] private Label _projectLocation;
    [NodePath] private Label _godotVersionDisplay;
    //[NodePath] private Button _heart;
    [NodePath] private PopupMenu _contextMenu;
    #endregion
    
    #region Resources
    [Resource("res://Assets/Icons/svg/missing_icon.svg")] private Texture2D _missingIcon;
    [Resource("res://Assets/Icons/png/default_project_icon.png")] private Texture2D _defaultProjectIcon;
    #endregion
    
    #region Private Variables
    private GodotVersion _godotVersion;
    private ProjectFile _projectFile;
    private ShaderMaterial _shader;
    private bool _selected = false;
    #endregion
    
    #region Public Properties
    public bool MissingProject { get; set; }

    public GodotVersion GodotVersion
    {
        get => _godotVersion;
        set
        {
            _godotVersion = value;
            if (_godotVersionDisplay == null) return;
            _godotVersionDisplay.Text = value is null ? "Unknown" : $"{_godotVersion.GetHumanReadableVersion()}";
        }
    }

    public ProjectFile ProjectFile
    {
        get => _projectFile;
        set
        {
            if (_projectFile != null)
                _projectFile.ProjectChanged -= UpdateUI;
            _projectFile = value;

            GodotVersion = _projectFile.GodotVersion;

            if (_projectName is null) return;

            value.ProjectChanged += UpdateUI;

            UpdateUI();
        }
    }

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            if (_hover != null)
                _hover.Visible = _selected;
        }
    }
    #endregion
    
    #region Godot Overrides
    public override void _Ready()
    {
        this.OnReady();
        ProjectFile = _projectFile;
        //_shader = (ShaderMaterial)_heart.Material.Duplicate();
        //_shader.SetShaderParameter("s", _projectFile.Favoriate ? 1.0f : 0.0f);
        //_shader.SetShaderParameter("v", _projectFile.Favoriate ? 1.0f : 0.5f);
        // _heart.Toggled += (toggle) =>
        // {
        //     _shader.SetShaderParameter("s", toggle ? 1.0f : 0.0f);
        //     _shader.SetShaderParameter("v", toggle ? 1.0f : 0.5f);
        //     EmitSignal(SignalName.FavoriteClicked, this, toggle);
        // };

        MouseEntered += () =>
        {
            if (Selected) return;
            _hover.Visible = true;
        };
        MouseExited += () =>
        {
            if (Selected) return;
            _hover.Visible = false;
        };
        GuiInput += HandleGuiInput;
        _contextMenu.IdPressed += id =>
        {
            EmitSignal(SignalName.ContextMenuClick, this, id);
        };
    }
    #endregion
    
    #region Godot Event Handlers

    private void HandleGuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton inputEventMouseButton) return;

        switch (inputEventMouseButton.ButtonIndex)
        {
            case MouseButton.Left when inputEventMouseButton.DoubleClick:
                EmitSignal(SignalName.DoubleClicked, this);
                break;
            case MouseButton.Left:
                Selected = true;
                EmitSignal(SignalName.Clicked, this);
                break;
            case MouseButton.Right when inputEventMouseButton.DoubleClick:
                EmitSignal(SignalName.RightDoubleClicked, this);
                break;
            case MouseButton.Right:
                EmitSignal(SignalName.RightClicked, this);
                break;
            default:
                return;
        }
    }
    #endregion
    
    #region Public Methods

    public void ShowContextMenu() => _contextMenu.Popup(new Rect2I((GetScreenPosition() + GetLocalMousePosition()).ToVector2I(), Vector2I.Zero));
    #endregion
    
    #region Private Methods

    private async void UpdateUI()
    {
        MissingProject = !File.Exists(ProjectFile.Location);
        _projectName.Text = ProjectFile.Name;
        _projectLocation.Text = MissingProject ? "Unknown Location" : ProjectFile.Location.GetBaseDir();

        if (MissingProject)
            _projectIcon.Texture = _missingIcon;
        else
        {
            var file = ProjectFile.Location.GetResourceBase(ProjectFile.Icon);
            _projectIcon.Texture = !File.Exists(file) ? _defaultProjectIcon : await Util.LoadImage(file);
        }
    }
    #endregion
}