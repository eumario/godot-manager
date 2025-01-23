using Godot;
using GodotManager.Library.Components.Controls;

namespace GodotManager.Library.Managers;

public partial class SignalBus : Node
{
    [Signal]
    public delegate void SettingsChangedEventHandler();

    [Signal]
    public delegate void SettingsUpdatedEventHandler();

    [Signal]
    public delegate void SettingsSavedEventHandler();
    
    [Signal]
    public delegate void DownloadBoxCreatedEventHandler(DownloadBox downloadBox);
}