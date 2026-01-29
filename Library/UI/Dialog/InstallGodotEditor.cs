#nullable enable
using System.Linq;
using Godot;
using GodotManager.Library.Database;
using GodotManager.Library.Managers;
using GodotManager.Library.Models;
using GodotManager.Library.Util;

namespace GodotManager.Library.UI.Dialog;

[SceneTree(root: "Nodes")]
public partial class InstallGodotEditor : PanelContainer
{
    private AppContext _appContext;
    private EngineRelease? _selectedRelease;
    private FeatureOption? _dotnetFeature;
    private FeatureOption? _templatesFeature;
    private FeatureOption? _selfContained;
    
    [OnInstantiate]
    public void Initialize()
    {
        _appContext = MainWindow.GetInstance()!.Context;
    }
    
    public override partial void _Ready();
    
    [GodotOverride]
    public void OnReady()
    {
        InstallOptions.Visible = false;
        SelectVersion.Visible = true;
        CloseButton.Pressed += QueueFree;
        CloseButton2.Pressed += QueueFree;
        BackButton.Pressed += ResetStepOne;
        InstallButton.Pressed += HandleInstallQueue;

        var lastVersion = SemanticVersion.Parse("0.0.0");
        VBoxContainer latestContainer = new VBoxContainer();
        var discard = latestContainer;
        
        ReleaseList.QueueFreeAllChildren();
        
        foreach (var release in _appContext.EngineReleases.Where(x => x.Repo == "godot").OrderByDescending(x => x.Version))
        {
            if (lastVersion.Version.Major != release.Version.Major ||
                lastVersion.Version.Minor != release.Version.Minor)
            {
                lastVersion = release.Version;
                var header = new FoldableContainer();
                header.Title = $"Godot {lastVersion.Version.Major}.{lastVersion.Version.Minor}";
                latestContainer = new VBoxContainer();
                header.AddChild(latestContainer);

                header.Folded = !_appContext.IsLatestVersion(release);
                
                ReleaseList.AddChild(header);
            }

            var item = ReleaseItem.Instantiate(release);
            item.LatestRelease = _appContext.IsLatestVersion(release);
            latestContainer.AddChild(item);
            item.InstallEngine += HandleInstall;
        }
        discard.QueueFree();
    }

    private void HandleInstallQueue()
    {
        // Check if we have a Release
        if (_selectedRelease == null) return;
        
        // Get our Tag and Options
        var tag = _selectedRelease.Version.ToString();
        var path = GlobalSettings.EngineCache.PathJoin(tag);
        var csharp = _dotnetFeature?.Install.ButtonPressed ?? false;
        var templates = _templatesFeature?.Install.ButtonPressed ?? false;
        var sc = _selfContained?.Install.ButtonPressed ?? false;
        
        // Get our Output Path and File Names
        var stdPack = path.PathJoin(_selectedRelease.StandardUrl.GetFile());
        var csharpPack = path.PathJoin(_selectedRelease.DotnetUrl.GetFile());
        var tmplPack = path.PathJoin(_selectedRelease.TemplateUrl.GetFile());
        var csharpTmplPack = path.PathJoin(_selectedRelease.DotnetTemplateUrl.GetFile());
        
        // Queue up our Downloads
        DownloadManager.Instance.QueueDownload(tag, _selectedRelease.StandardUrl, _selectedRelease.StandardSize,
            stdPack);
        if (csharp)
            DownloadManager.Instance.QueueDownload(tag, _selectedRelease.DotnetUrl, _selectedRelease.DotnetSize,
                csharpPack);
        if (templates)
            DownloadManager.Instance.QueueDownload(tag, _selectedRelease.TemplateUrl, _selectedRelease.TemplateSize,
                tmplPack);
        if (templates && csharp)
            DownloadManager.Instance.QueueDownload(tag, _selectedRelease.DotnetTemplateUrl, _selectedRelease.DotnetTemplateSize,
                csharpTmplPack);
        
        // Queue up our Install
        InstallManager.Instance.QueueInstall(tag, stdPack, GlobalSettings.EnginePath.PathJoin(tag));
        if (csharp) InstallManager.Instance.QueueInstall(tag, csharpPack, GlobalSettings.EnginePath.PathJoin(tag + "-mono"));
        if (sc) InstallManager.Instance.QueueInstall(tag, "", GlobalSettings.EnginePath.PathJoin(tag).PathJoin("._sc_"));
        if (templates) InstallManager.Instance.QueueInstall(tag, tmplPack, DirHelper.GetEditorDataPath(tag, sc).PathJoin("export_templates"));
        if (templates & csharp) InstallManager.Instance.QueueInstall(tag, csharpTmplPack, DirHelper.GetEditorDataPath(tag + "-mono", sc).PathJoin("export_templates"));
        QueueFree();
    }

    private void ResetStepOne()
    {
        InstallOptions.Visible = false;
        SelectVersion.Visible = true;
        _selectedRelease = null;
        if (_dotnetFeature != null)
        {
            _dotnetFeature.QueueFree();
            _dotnetFeature = null;
        }

        if (_templatesFeature != null)
        {
            _templatesFeature.QueueFree();
            _templatesFeature = null;
        }

        if (_selfContained != null)
        {
            _selfContained.QueueFree();
            _selfContained = null;
        }
    }

    private void HandleInstall(ReleaseItem item)
    {
        if (item.Release == null) return;
        
        _selectedRelease = item.Release;
        SelectVersion.Visible = false;
        InstallOptions.Visible = true;
        
        InstallTitle.Text = $"Godot {_selectedRelease.Version}";
        RequiredSize.Text = _selectedRelease.StandardSize.FormatSize();
        
        if (_selectedRelease.DotnetUrl != "")
        {
            var csharp = _selectedRelease.IsGodot3() ? "Mono Support" : "Dotnet Support"; 
            _dotnetFeature = FeatureOption.Instantiate($"{csharp} Support", _selectedRelease.DotnetSize);
            _dotnetFeature.Install.Pressed += UpdateBytes;
            _dotnetFeature.HintTooltip = $"""
                                           Provides {csharp} C# Support for the Godot Engine.
                                           (Currently, this is a Separate Editor built specifically for
                                           Godot and Separate Templates).
                                           """;
            AvailableFeatures.AddChild(_dotnetFeature);
        }

        if (_selectedRelease.TemplateUrl != "")
        {
            _templatesFeature = FeatureOption.Instantiate("Template Support", _selectedRelease.TemplateSize);
            _templatesFeature.Install.Pressed += UpdateBytes;
            _templatesFeature.HintTooltip = """
                                            Provides the standard template support, for exporting Godot Projects
                                             to their final executable format for distribution.
                                            """;
            AvailableFeatures.AddChild(_templatesFeature);
        }

        _selfContained = FeatureOption.Instantiate("Self Contained Install", -1);
        _selfContained.Install.ButtonPressed = true;
        _selfContained.HintTooltip = """
                                     Installs the Godot editor with a self-contained folder for editor data.
                                     When this is not enabled, Godot will store to the normal global configuration location.
                                     """;
        AvailableFeatures.AddChild(_selfContained);
    }

    private void UpdateBytes()
    {
        if (_selectedRelease == null) return;
        var initBytes = (decimal)_selectedRelease.StandardSize;
        if (_dotnetFeature != null)
        {
            if (_dotnetFeature.Install.IsPressed())
                initBytes += _selectedRelease.DotnetSize;
            
            if (_templatesFeature != null)
            {
                var csharp = _selectedRelease.IsGodot3() ? "Mono" : "Dotnet";
                decimal templateSize = _selectedRelease.TemplateSize;
                decimal dotnetTemplateSize = _selectedRelease.DotnetTemplateSize;
                if (_dotnetFeature.Install.IsPressed())
                {
                    _templatesFeature.FeatureText = $"Template Support (w/{csharp} Support)";
                    _templatesFeature.FeatureSize = templateSize + dotnetTemplateSize;
                    _templatesFeature.HintTooltip = $"""
                                                    Provides the standard template support with {csharp}
                                                    support templates, for exporting Godot Projects to 
                                                    their final executable format for distribution.
                                                    """;
                }
                else
                {
                    _templatesFeature.FeatureText = "Template Support";
                    _templatesFeature.FeatureSize = templateSize;
                    _templatesFeature.HintTooltip = """
                                                    Provides the standard template support, for exporting Godot Projects
                                                     to their final executable format for distribution.
                                                    """;
                }
            }
        }

        if (_templatesFeature != null && _templatesFeature.Install.IsPressed())
        {
            initBytes += _selectedRelease.TemplateSize;
            if (_dotnetFeature != null && _dotnetFeature.Install.IsPressed())
                initBytes += _selectedRelease.DotnetTemplateSize;
        }
        
        RequiredSize.Text = initBytes.FormatSize();
    }
}
