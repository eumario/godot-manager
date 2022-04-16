using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using Newtonsoft.Json;
using System.IO.Compression;
using SFile = System.IO.File;

public class AddonInstaller : ReferenceRect
{
#region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/VB/DetailLabel")]
    Label _detailLabel = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/SC/VBoxContainer/AddonTree")]
    Tree _addonTree = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/OkButton")]
    Button _okButton = null;
#endregion

#region Private Variables
    private PluginInstaller _installer;
    private TreeItem _root;
    private bool _updating = false;
    private Dictionary<string, TreeItem> _statusMap;
#endregion

#region Public Variables
#endregion

#region Icon Registry
    Texture ift_image = GD.Load<Texture>("res://Assets/Icons/icon_ft_image.svg");
    Texture ift_audio = GD.Load<Texture>("res://Assets/Icons/icon_ft_audio.svg");
    Texture ift_packedscene = GD.Load<Texture>("res://Assets/Icons/icon_ft_packed_scene.svg");
    Texture ift_shader = GD.Load<Texture>("res://Assets/Icons/icon_ft_shader.svg");
    Texture ift_gdscript = GD.Load<Texture>("res://Assets/Icons/icon_ft_gdscript.svg");
    Texture ift_csharp = GD.Load<Texture>("res://Assets/Icons/icon_ft_csharp.svg");
    Texture ift_visualscript = GD.Load<Texture>("res://Assets/Icons/icon_ft_visualscript.svg");
    Texture ift_resource = GD.Load<Texture>("res://Assets/Icons/icon_ft_resource.svg");
    Texture ift_atlastexture = GD.Load<Texture>("res://Assets/Icons/icon_ft_atlas_texture.svg");
    Texture ift_mesh = GD.Load<Texture>("res://Assets/Icons/icon_ft_mesh.svg");
    Texture ift_text = GD.Load<Texture>("res://Assets/Icons/icon_ft_text.svg");
    Texture ift_font = GD.Load<Texture>("res://Assets/Icons/icon_ft_font.svg");
    Texture ift_object = GD.Load<Texture>("res://Assets/Icons/icon_ft_object.svg");
    Texture ift_file = GD.Load<Texture>("res://Assets/Icons/icon_ft_file.svg");
    Texture ift_folder = GD.Load<Texture>("res://Assets/Icons/icon_ft_folder.svg");

    Dictionary<string, Texture> IconRegistry = null;

    Array<string> IgnoreFiles = null;
#endregion

    void AddRegistry(string[] exts, Texture icon) {
        foreach (string ext in exts)
            IconRegistry.Add(ext, icon);
    }

    void InitRegistry() {
        IconRegistry = new Dictionary<string, Texture>();
        // Image Formats
        AddRegistry(new string[] {".bmp",".dds",".exr",".hdr",".jpg",".jpeg",".png",".svg",".svgz",".tga",".webp"}, ift_image);
        // Audio Formats
        AddRegistry(new string[] {".wav",".mp3",".ogg"}, ift_audio);
        // Packed Scene Formats
        AddRegistry(new string[] {".scn",".tscn",".escn",".dae",".gltf",".glb"}, ift_packedscene);
        // Shader Formats
        AddRegistry(new string[] {".gdshader",".shader"}, ift_shader);
        // Script Formats
        AddRegistry(new string[] {".gd"}, ift_gdscript);
        AddRegistry(new string[] {".cs"}, ift_csharp);
        AddRegistry(new string[] {".vs"}, ift_visualscript);
        // Atlas Texture Format
        AddRegistry(new string[] {".atlastex"}, ift_atlastexture);
        // Mesh Texture Format
        AddRegistry(new string[] {".obj"}, ift_mesh);
        // Text File Formats
        AddRegistry(new string[] {".txt",".md",".rst",".json",".yml",".yaml",".toml",".cfg",".ini"}, ift_text);
        // Font Formats
        AddRegistry(new string[] {".ttf",".otf",".woff",".fnt"}, ift_font);
        // No Extension
        AddRegistry(new string[] {"::noext::"}, ift_object);
        // Unknown Extension
        AddRegistry(new string[] {"::unknown::"}, ift_file);
        AddRegistry(new string[] {"::folder::"}, ift_folder);
    }

    void InitIgnoreFiles() {
        IgnoreFiles = new Array<string>();
        IgnoreFiles.Add("res://icon.png");
        IgnoreFiles.Add("res://icon.png.import");
        IgnoreFiles.Add("res://project.godot");
        IgnoreFiles.Add("res://default_env.tres");
        IgnoreFiles.Add("res://.gitignore");
        IgnoreFiles.Add("res://README.md");
        IgnoreFiles.Add("res://LICENSE");
        IgnoreFiles.Add("res://LICENSE.md");
    }

    public override void _Ready()
    {
        this.OnReady();
        InitRegistry();
        InitIgnoreFiles();
        _statusMap = new Dictionary<string, TreeItem>();
    }

    public void ShowDialog(AssetPlugin asset) {
        _installer = new PluginInstaller(asset);
        _detailLabel.Text = $"Contents of asset {asset.Asset.Title}\nSelect files to Install:";
        PopulateTree();
        Visible = true;
    }

    void UpdateSubitems(TreeItem item, bool check, bool first = false) {
        // Code "Copied" from Godot editor_asset_installer.cpp
        item.SetChecked(0, check);

        if (item.GetChildren() != null) {
            UpdateSubitems(item.GetChildren(), check);
        }

        if (!first && item.GetNext() != null) {
            UpdateSubitems(item.GetNext(), check);
        }
    }

    void UncheckParent(TreeItem item) {
        // Code "Copied" from Godot editor_asset_installer.cpp
        if (item == null)
            return;
        
        bool any_checked = false;
        TreeItem citem = item.GetChildren();
        while (citem != null) {
            if (citem.IsChecked(0)) {
                any_checked = true;
                break;
            }
            citem = citem.GetNext();
        }

        if (!any_checked) {
            item.SetChecked(0,false);
            UncheckParent(item.GetParent());
        }
    }

    [SignalHandler("pressed", nameof(_okButton))]
    void OnPressed_OkButton() {
        Array<string> installFiles = new Array<string>();

        foreach(string key in _statusMap.Keys) {
            if (_statusMap[key] != null) {
                if (_statusMap[key].IsChecked(0))
                    installFiles.Add(key);
            }
        }
        _installer.AssetPlugin.InstallFiles = installFiles;
        
        CentralStore.Instance.SaveDatabase();
        Visible = false;
    }

    [SignalHandler("item_edited", nameof(_addonTree))]
    async void OnItemEdited() {
        // Code "Copied" from Godot editor_asset_installer.cpp
        if (_updating)
            return;
        
        TreeItem item = _addonTree.GetEdited();
        if (item == null)
            return;

        _updating = true;

        string path = item.GetMetadata(0) as string;

        if (item.GetCustomColor(0) == new Color(1,0,0)) {
            if (item.IsChecked(0)) {
                var res = await AppDialogs.YesNoDialog.ShowDialog("Addon Installer - Ignored File","The file you have selected, is known to be a file that is part of your project structure, and can cause corruption if installed, do you wish to continue?");
                item.SetChecked(0,res);
                _updating = false;
                if (!res)
                    return;
            }
        }

        if (path == string.Empty || item == _root) {
            UpdateSubitems(item, item.IsChecked(0), true);
        }

        if (item.IsChecked(0)) {
            while (item != null) {
                item.SetChecked(0, true);
                item = item.GetParent();
            }
        } else {
            UncheckParent(item.GetParent());
        }
        _updating = false;
    }


    void PopulateTree() {
        // Original code inspired by editor_asset_installer.cpp
        _updating = true;
        _addonTree.Clear();
        _root = _addonTree.CreateItem(null, -1);
        _root.SetCellMode(0,TreeItem.TreeCellMode.Check);
        _root.SetChecked(0,true);
        _root.SetText(0,"res://");
        _root.SetIcon(0,IconRegistry["::folder::"]);
        _root.SetEditable(0,true);
        Dictionary<string, TreeItem> folders = new Dictionary<string, TreeItem>();

        int indx = -1;
        Array<string> _zipContents = _installer.GetZipContents();
        foreach(string entry in _installer.GetFileList()) {
            string path = entry;
            bool isdir = false;
            indx++;

            if (path.IndexOf("/") > 0)
                path = path.Substr(path.IndexOf("/")+1,path.Length);

            if (path.EndsWith("/")) {
                path = path.Substr(0,path.Length-1);
                isdir = true;
            }

            if (path == "")
                continue;

            int pp = path.FindLast("/");

            TreeItem parent;
            if (pp == -1) {
                parent = _root;
            } else {
                string ppath = path.Substr(0,pp);
                if (folders.ContainsKey(ppath))
                    parent = folders[ppath];
                else
                    parent = _root;
            }

            TreeItem ti = _addonTree.CreateItem(parent);
            ti.SetCellMode(0, TreeItem.TreeCellMode.Check);
            ti.SetChecked(0,true);
            ti.SetEditable(0,true);
            if (isdir) {
                folders[path] = ti;
                ti.SetText(0, path.GetFile() + "/");
                ti.SetIcon(0, IconRegistry["::folder::"]);
                ti.SetMetadata(0,"");
            } else {
                string file = path.GetFile();
                string ext = file.GetExtension().ToLower();
                if (IconRegistry.ContainsKey(ext)) {
                    ti.SetIcon(0, IconRegistry[ext]);
                } else if (ext == string.Empty) {
                    ti.SetIcon(0, IconRegistry["::noext::"]);
                } else {
                    ti.SetIcon(0, IconRegistry["::unknown::"]);
                }
                ti.SetText(0,file);

                ti.SetMetadata(0,"res://".Join(path));
                
                if (IgnoreFiles.Contains("res://".Join(path))) {
                    ti.SetChecked(0,false);
                    ti.SetCustomColor(0,new Color(1,0,0));
                }
            }

            _statusMap[_zipContents[indx]] = ti;
        }
        _updating = false;
    }
}
