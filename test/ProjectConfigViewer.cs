using Godot;
using System;
using System.Linq;
using GodotManager.Library.Util;

[Tool, GlobalClass, SceneTree(root: "Nodes")]
public partial class ProjectConfigViewer : Control
{
    private GodotProjectFile _config;
    private GodotConfigParser _parser;
    private TreeItem _root;
    private Font _treeFont;
    private int _fontSize;

    [OnInstantiate]
    public void Initialize()
    {
        _config = null;
        SectionList.Disabled = true;
        SectionViewer.SetColumnCustomMinimumWidth(0, 20);
        SectionViewer.SetColumnCustomMinimumWidth(3, 20);
        SectionViewer.SetColumnExpand(0, false);
        SectionViewer.SetColumnExpand(1, false);
        SectionViewer.SetColumnClipContent(1, false);
        SectionViewer.SetColumnExpand(2, true);
        SectionViewer.SetColumnExpand(3, false);
        SectionViewer.SetColumnCustomMinimumWidth(1,120);
        SectionViewer.SetColumnTitle(1, "Key");
        SectionViewer.SetColumnTitle(2, "Value");
        _treeFont = SectionViewer.GetThemeFont("font");
        _fontSize = SectionViewer.GetThemeFontSize("font");
    }
    
    [GodotOverride]
    public void OnReady()
    {
        LoadConfig.Pressed += async () =>
        {
            var dlg = new FileDialog();
            dlg.UseNativeDialog = true;
            dlg.ForceNative = true;
            dlg.Title = "Open ProjectConfig";
            dlg.CurrentDir = ProjectSettings.GlobalizePath("res://test/test_project_configs");
            dlg.FileMode = FileDialog.FileModeEnum.OpenFile;
            dlg.Access = FileDialog.AccessEnum.Filesystem;
            dlg.Filters = new[]
            {
                "*.project.godot,*.godot;Godot Project;text/plain"
            };
            AddChild(dlg);
            dlg.PopupCentered(new Vector2I(400,300));
            dlg.FileSelected += file =>
            {
                GD.Print($"File selected: {file}");
                _parser = new GodotConfigParser();
                _parser.Parse(file);
                SectionList.Clear();
                SectionList.AddItem("Header");
                foreach (var section in _parser.Sections.Keys)
                    SectionList.AddItem(section);
                SectionList.EmitSignal(OptionButton.SignalName.ItemSelected, 0);
                SectionList.Disabled = false;
            };
            dlg.CloseRequested += () => dlg.QueueFree();
        };

        SectionList.ItemSelected += async index =>
        {
            var section = SectionList.GetItemText((int)index);
            SectionViewer.Clear();
            var keySize = new Vector2(120, 0);
            _root = SectionViewer.CreateItem();
            if (section == "Header")
            {
                foreach (var key in _parser.Global.Keys)
                {
                    var size = _treeFont.GetStringSize(key);
                    if (size.X > keySize.X) keySize = size;
                    var iter = _root.CreateChild();
                    iter.SetText(1, key);
                    iter.SetText(2, _parser.Global[key]);
                }
            }
            else
            {
                foreach (var key in _parser.Sections[section].Keys)
                {
                    var size = _treeFont.GetStringSize(key);
                    if (size.X > keySize.X) keySize = size;
                    var iter = _root.CreateChild();
                    iter.SetText(1, key);
                    iter.SetText(2, _parser.GetValue(key, section));
                }
            }

            SectionViewer.SetColumnCustomMinimumWidth(1, (int)keySize.X);
        };
    }

    public override partial void _Ready();
}
