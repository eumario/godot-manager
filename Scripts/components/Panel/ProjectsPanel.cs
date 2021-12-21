using Godot;
using Godot.Collections;
using GodotSharpExtras;
using System.Linq;

public class ProjectsPanel : Panel
{
#region Node Accessors
    [NodePath("VC/MC/HC/ActionButtons")]
    ActionButtons _actionButtons = null;
    [NodePath("VC/SC/MarginContainer/ProjectList/ListView")]
    VBoxContainer _listView = null;
    [NodePath("VC/SC/MarginContainer/ProjectList/GridView")]
    GridContainer _gridView = null;
    [NodePath("VC/SC/MarginContainer/ProjectList/CategoryView")]
    VBoxContainer _categoryView = null;
    [NodePath("VC/MC/HC/ViewToggleButtons")]
    ViewToggleButtons _viewSelector = null;
#endregion

#region Template Scenes
    PackedScene _ProjectLineEntry = GD.Load<PackedScene>("res://components/ProjectLineEntry.tscn");
    PackedScene _ProjectIconEntry = GD.Load<PackedScene>("res://components/ProjectIconEntry.tscn");
    PackedScene _CategoryListToggle = GD.Load<PackedScene>("res://components/CategoryListToggle.tscn");
#endregion

    Array<Container> _views;

    public override void _Ready()
    {
        this.OnReady();

        _views = new Array<Container>();
        _views.Add(_listView);
        _views.Add(_gridView);
        _views.Add(_categoryView);

        _viewSelector.Connect("Clicked", this, "OnViewSelector_Clicked");
        _actionButtons.Connect("clicked", this, "OnActionButtons_Clicked");
        AppDialogs.Instance.ImportProject.Connect("update_projects", this, "PopulateListing");

        CentralStore.Instance.LoadDatabase();
        PopulateListing();
    }


    public ProjectLineEntry NewPLE(ProjectFile pf) {
        ProjectLineEntry ple = _ProjectLineEntry.Instance<ProjectLineEntry>();
        ple.Name = pf.Name;
        ple.Description = pf.Description;
        ple.Icon = pf.Location.GetResourceBase(pf.Icon);
        ple.Location = pf.Location;
        return ple;
    }

    public ProjectIconEntry NewPIE(ProjectFile pf) {
        ProjectIconEntry pie = _ProjectIconEntry.Instance<ProjectIconEntry>();
        pie.ProjectName = pf.Name;
        pie.Icon = pf.Location.GetResourceBase(pf.Icon);
        pie.ProjectLocation = pf.Location;
        //pie.GodotVersion = pf.GodotVersion;
        return pie;
    }
    
    public CategoryListToggle NewCLT(string name) {
        CategoryListToggle clt = _CategoryListToggle.Instance<CategoryListToggle>();
        clt.CategoryName = name;
        return clt;
    }

    public void PopulateListing() {
        ProjectLineEntry ple;
        ProjectIconEntry pie;
        CategoryListToggle clt;

        foreach(Node child in _listView.GetChildren()) {
            child.QueueFree();
        }
        foreach(Node child in _gridView.GetChildren()) {
            child.QueueFree();
        }
        foreach(CategoryListToggle child in _categoryView.GetChildren()) {
            foreach(Node cchild in child.CategoryList.GetChildren()) {
                cchild.QueueFree();
            }
            child.QueueFree();
        }

        foreach(Category cat in CentralStore.Instance.Categories) {
            clt = NewCLT(cat.Name);
            clt.Set("ID",cat.Id);
            _categoryView.AddChild(clt);
        }

        clt = NewCLT("Un-Categorized");
        clt.Set("ID",-1);
        _categoryView.AddChild(clt);

        foreach(ProjectFile pf in CentralStore.Instance.Projects) {
            ple = NewPLE(pf);
            pie = NewPIE(pf);
            _listView.AddChild(ple);
            ple.Connect("Clicked", this, "OnListEntry_Clicked");
            ple.Connect("DoubleClicked", this, "OnListEntry_DoubleClicked");
            _gridView.AddChild(pie);
            if (pf.CategoryId == -1) {
                clt = _categoryView.GetChild<CategoryListToggle>(_categoryView.GetChildCount()-1);
            } else {
                clt = _categoryView.GetChild<CategoryListToggle>(pf.CategoryId);
            }
            clt.AddProject(pf);
        }
    }

    private void UpdateListExcept(ProjectLineEntry ple) {
        foreach (ProjectLineEntry cple in _listView.GetChildren()) {
            if (cple != ple)
                cple.SelfModulate = new Color("00ffffff");
        }
    }

    public void OnListEntry_Clicked(ProjectLineEntry ple) {
        UpdateListExcept(ple);
    }

    public void OnListEntry_DoubleClicked(ProjectLineEntry ple) {
        GD.Print(ple.Location);
        // OS.Execute(@"C:\Users\eumar\AppData\Local\Programs\Microsoft VS Code\bin\code.cmd", new string[] { $"{ple.Location}"}, false);
    }

    public void OnActionButtons_Clicked(int index) {
        switch (index) {
            case 0: // New Project File
                AppDialogs.Instance.CreateProject.ShowDialog();
                break;
            case 1: // Import Project File
                AppDialogs.Instance.ImportProject.Visible = true;
                break;
            case 2: // Scan Project Folder
                break;
            case 3: // Remove Project (May be removed completely)
                break;
        }
    }

    public void OnViewSelector_Clicked(int page) {
        for (int i = 0; i < _views.Count; i++) {
            if (i == page)
                _views[i].Show();
            else
                _views[i].Hide();
        }
    }

    public void AddTestProjects() {
        ProjectFile pf = ProjectFile.ReadFromFile(@"E:\Projects\Godot\godot-manager-mono\project.godot");
        CentralStore.Instance.Projects.Add(pf);
        pf = ProjectFile.ReadFromFile(@"E:\Projects\Godot\3D Platformer Demo\project.godot");
        CentralStore.Instance.Projects.Add(pf);
        pf = ProjectFile.ReadFromFile(@"E:\Projects\Godot\EditorPlugins\project.godot");
        CentralStore.Instance.Projects.Add(pf);
        pf = ProjectFile.ReadFromFile(@"E:\Projects\Godot\Godot-3D-Space-Shooter-main\project.godot");
        CentralStore.Instance.Projects.Add(pf);
        pf = ProjectFile.ReadFromFile(@"E:\Projects\Godot\Third Person Shooter Demo\project.godot");
        CentralStore.Instance.Projects.Add(pf);
        pf = ProjectFile.ReadFromFile(@"E:\Projects\src\mad-productivity\project.godot");
        CentralStore.Instance.Projects.Add(pf);
        CentralStore.Instance.SaveDatabase();
        PopulateListing();
    }

    public Array<ProjectFile> TestSortListing() {
        Array<ProjectFile> projectFiles = new Array<ProjectFile>();
        var pfolder = CentralStore.Instance.Projects.OrderByDescending(pf => pf.LastAccessed);
        foreach (ProjectFile pf in pfolder)
            projectFiles.Add(pf);
        return projectFiles;
    }

    public Array<ProjectFile> TestFavSortListing() {
        Array<ProjectFile> projectFiles = new Array<ProjectFile>();
        var fav = CentralStore.Instance.Projects.Where(pf => pf.Favorite == true).OrderByDescending(pf => pf.LastAccessed);
        var non_fav = CentralStore.Instance.Projects.Where(pf => pf.Favorite != true).OrderByDescending(pf => pf.LastAccessed);

        foreach(ProjectFile pf in fav)
            projectFiles.Add(pf);
        
        foreach(ProjectFile pf in non_fav)
            projectFiles.Add(pf);
        
        return projectFiles;
    }

    public void OnButton_Pressed() {
        //AddTestProjects();
        
        Array<ProjectFile> projectFiles = TestFavSortListing(); //TestSortListing();

        foreach(ProjectFile pf in projectFiles) {
            GD.Print($"Project: {pf.Name}, Last Accessed: {pf.LastAccessed}");
        }
        
    }
}   