using Godot;
using System;
using System.Linq;
using GodotManager.Library.Util;

[Tool, GlobalClass, SceneTree(root: "Nodes")]
public partial class ProjectConfigViewer : Control
{
    private GodotProjectFile _config;
    private GodotConfigParser _parser;

    [OnInstantiate]
    public void Initialize()
    {
        _config = null;
        SectionList.Disabled = true;
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
                // var cfg = new GodotProjectFile(file);
                // cfg.Load();
                // SectionList.Clear();
                // foreach(var section in cfg.Sections)
                //     SectionList.AddItem(section);
                // _config = cfg;
                SectionList.Disabled = false;
            };
            await ToSignal(dlg, Window.SignalName.CloseRequested);
            dlg.QueueFree();
            GD.Print("Dialog freed.");
        };

        ItemEntry.KeySizeChangedEventHandler itemOnKeySizeChanged(ItemEntry itemEntry)
        {
            return (width) =>
            {
                if (KeyHeader.CustomMinimumSize.X < width)
                {
                    var newSize = new Vector2(width, 0);
                    KeyHeader.CustomMinimumSize = newSize;
                    KeyHeader.GetParent<HBoxContainer>().QueueSort();
                    KeyHeader.QueueRedraw();
                    foreach (var citem in Items.GetChildren().OfType<ItemEntry>())
                    {
                        if (citem == itemEntry) continue;
                        citem.CustomMinimumSize = newSize;
                        citem.Size = newSize;
                        citem.QueueRedraw();
                    }

                    Items.QueueSort();
                    Items.QueueRedraw();
                }
            };
        }

        SectionList.ItemSelected += async index =>
        {
            foreach (var child in Items.GetChildren()) child.QueueFree();
            Items.AddChild(new HSeparator());
            var section = SectionList.GetItemText((int)index);
            // foreach (var key in _config.GetKeys(section))
            // {
            //     var item = ItemEntry.Instantiate(key, _config[section, key]);
            //     Items.AddChild(item);
            // }
            if (section == "Header")
            {
                foreach (var key in _parser.Global.Keys)
                {
                    var item = ItemEntry.Instantiate(key, _parser.GetValue(key));
                    Items.AddChild(item);
                    Items.AddChild(new HSeparator());
                    item.KeySizeChanged += itemOnKeySizeChanged(item);
                }
            }
            else
            {
                foreach (var key in _parser.Sections[section].Keys)
                {
                    var item = ItemEntry.Instantiate(key, _parser.GetValue(key, section));
                    Items.AddChild(item);
                    item.KeySizeChanged += itemOnKeySizeChanged(item);
                }
            }
        };
    }

    public override partial void _Ready();
}
