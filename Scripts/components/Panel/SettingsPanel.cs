using Godot;
using GodotSharpExtras;
using Godot.Collections;

public class SettingsPanel : Panel
{
#region Node Paths
    #region Page Buttons
    [NodePath("VB/Header/HC/PC/HC/General")]
    Button _generalBtn = null;

    [NodePath("VB/Header/HC/PC/HC/Projects")]
    Button _projectsBtn = null;

    [NodePath("VB/Header/HC/PC/HC/About")]
    Button _aboutBtn = null;
    #endregion

    [NodePath("VB/MC/TC")]
    TabContainer _pages = null;

    [NodePath("VB/Header/HC/ActionButtons")]
    ActionButtons _actionButtons = null;

    #region General Page
    [NodePath("VB/MC/TC/General/GC/CheckForUpdates")]
    CheckBox _checkForUpdates = null;

    [NodePath("VB/MC/TC/General/GC/HBCI/UpdateCheckInterval")]
    OptionButton _updateCheckInterval = null;
    #endregion

    #region Projects Page
    #endregion

    #region About Page
    #endregion
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();

        // Event Handlers for Pages
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
        _generalBtn.Connect("pressed", this, "OnGeneralPressed");
        _projectsBtn.Connect("pressed", this, "OnProjectsPressed");
        _aboutBtn.Connect("pressed", this, "OnAboutPressed");

        // Event Handlers for General
        _checkForUpdates.Connect("toggled", this, "OnToggleCheckForUpdates");
        _updateCheckInterval.Disabled = !_checkForUpdates.Pressed;
    }

#region Event Handlers for Notebook
    void OnPageChanged(int page) {
    }

    void OnGeneralPressed() {
        _generalBtn.Pressed = true;
        _projectsBtn.Pressed = false;
        _aboutBtn.Pressed = false;
        _pages.CurrentTab = 0;
    }

    void OnProjectsPressed() {
        _generalBtn.Pressed = false;
        _projectsBtn.Pressed = true;
        _aboutBtn.Pressed = false;
        _pages.CurrentTab = 1;
    }

    void OnAboutPressed() {
        _generalBtn.Pressed = false;
        _projectsBtn.Pressed = false;
        _aboutBtn.Pressed = true;
        _pages.CurrentTab = 2;
    }
#endregion

#region Event Handlers for General Page
    public void OnToggleCheckForUpdates(bool toggle) {
        _updateCheckInterval.Disabled = !toggle;
    }
#endregion

#region Event Handlers for Projects Page
#endregion

#region Event Handler for About Page
#endregion
}
