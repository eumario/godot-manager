#nullable enable
using System.Linq;
using Godot;
using GodotManager;
using GodotManager.Library.Database;
using GodotManager.Library.Managers;
using GodotManager.Library.Models;
using GodotManager.Library.UI;
using GodotManager.Library.Util;
using Octokit;

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
        BackButton.Pressed += () =>
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
        };

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
            AvailableFeatures.AddChild(_dotnetFeature);
        }

        if (_selectedRelease.TemplateUrl != "")
        {
            _templatesFeature = FeatureOption.Instantiate("Template Support", _selectedRelease.TemplateSize);
            _templatesFeature.Install.Pressed += UpdateBytes;
            AvailableFeatures.AddChild(_templatesFeature);
        }

        _selfContained = FeatureOption.Instantiate("Self Contained Install", -1);
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
                }
                else
                {
                    _templatesFeature.FeatureText = "Template Support";
                    _templatesFeature.FeatureSize = templateSize;
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
