using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;

public class AddonLineEntry : ColorRect
{
    #region Signals
    [Signal] public delegate void install_clicked(bool value);
    #endregion
    
    #region Node Paths
    [NodePath("hc/AddonIcon")] private TextureRect AddonIcon = null;
    [NodePath("hc/AddonName")] private Label AddonName = null;
    [NodePath("hc/AddonVersion")] private Label AddonVersion = null;
    [NodePath("hc/InstallUninstall")] private TextureRect InstallUninstall = null;
    #endregion
    
    #region Resources
    [Resource("res://Assets/Icons/icon_add.svg")] private Texture IconAdd = null;
    [Resource("res://Assets/Icons/x.svg")] private Texture IconCancel = null;
    #endregion
    
    #region Private Variables
    private Texture _icon = null;
    private string _title = null;
    private string _version = null;
    private bool _installed = false;
    #endregion
    
    #region Public Variables
    public Texture Icon
    {
        get => AddonIcon != null ? AddonIcon.Texture : _icon;
        set
        {
            _icon = value;
            if (AddonIcon != null) AddonIcon.Texture = _icon;
        }
    }

    public string Title
    {
        get => AddonName != null ? AddonName.Text : _title;
        set
        {
            _title = value;
            if (AddonName != null) AddonName.Text = value;
        }
    }

    public string Version
    {
        get => _version;
        set
        {
            _version = value;
            if (AddonVersion != null) AddonVersion.Text = $"Version: {_version}";
        }
    }

    public bool Installed
    {
        get => _installed;
        set
        {
            _installed = value;
            if (_installed && InstallUninstall != null)
            {
                InstallUninstall.Texture = IconCancel;
                InstallUninstall.SelfModulate = Colors.Red;
            }
            else
            {
                InstallUninstall.Texture = IconAdd;
                InstallUninstall.SelfModulate = Colors.Green;
            }
        }
    }

    #endregion

    public override void _Ready()
    {
        this.OnReady();
        Version = _version;
        Title = _title;
        Icon = _icon;
        Installed = _installed;
    }

    [SignalHandler("gui_input", nameof(InstallUninstall))]
    void OnGuiInput_InstallUninstall(InputEvent @event)
    {
        if (@event is InputEventMouseButton iemb && iemb.Pressed && iemb.ButtonIndex == (int)ButtonList.Left)
        {
            Installed = !Installed;
            EmitSignal("install_clicked", Installed);
        }
    }
}
