using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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
    PackedScene _CategoryList = GD.Load<PackedScene>("res://components/CategoryList.tscn");
#endregion

#region Enumerations
    enum View {
        ListView,
        GridView,
        CategoryView
    }
#endregion

#region Private Variables
    CategoryList clFavorites = null;
    CategoryList clUncategorized = null;

    ProjectLineEntry _currentPLE = null;
    ProjectIconEntry _currentPIE = null;

    View _currentView = View.ListView;
    Dictionary<int, CategoryList> _categoryList;
    ProjectPopup _popupMenu = null;
    Array<ProjectFile> _missingProjects = null;

    Array<string> Views = new Array<string> {
        "List View",
        "Icon View",
        "Category View"
    };
#endregion

    Array<Container> _views;

    public override void _Ready()
    {
        this.OnReady();

        _views = new Array<Container>();
        _views.Add(_listView);
        _views.Add(_gridView);
        _views.Add(_categoryView);

        _popupMenu = GD.Load<PackedScene>("res://components/ProjectPopup.tscn").Instance<ProjectPopup>();
        AddChild(_popupMenu);
        _popupMenu.SetAsToplevel(true);

        AppDialogs.ImportProject.Connect("update_projects", this, "PopulateListing");
        AppDialogs.CreateCategory.Connect("update_categories", this, "PopulateListing");
        AppDialogs.RemoveCategory.Connect("update_categories", this, "PopulateListing");
        AppDialogs.EditProject.Connect("project_updated", this, "PopulateListing");
        AppDialogs.CreateProject.Connect("project_created", this, "OnProjectCreated");

        _actionButtons.SetHidden(3);
        _actionButtons.SetHidden(4);
        _categoryList = new Dictionary<int, CategoryList>();
        _missingProjects = new Array<ProjectFile>();

        if (_viewSelector.SelectedView != -1) {
            if (CentralStore.Settings.DefaultView == "Last View Used") {
                int indx = Views.IndexOf(CentralStore.Settings.LastView);
                _viewSelector.SetView(indx);
                OnViewSelector_Clicked(indx);
            } else {
                int indx = Views.IndexOf(CentralStore.Settings.DefaultView);
                _viewSelector.SetView(indx);
                OnViewSelector_Clicked(indx);
            }
        }

        PopulateListing();
    }


    public ProjectLineEntry NewPLE(ProjectFile pf) {
        ProjectLineEntry ple = _ProjectLineEntry.Instance<ProjectLineEntry>();
        if (_missingProjects.Contains(pf))
            ple.MissingProject = true;
        else if (!ProjectFile.ProjectExists(pf.Location)) {
            _missingProjects.Add(pf);
            ple.MissingProject = true;
        }
        ple.ProjectFile = pf;
        return ple;
    }

    public ProjectIconEntry NewPIE(ProjectFile pf) {
        ProjectIconEntry pie = _ProjectIconEntry.Instance<ProjectIconEntry>();
        if (_missingProjects.Contains(pf))
            pie.MissingProject = true;
        else if (!ProjectFile.ProjectExists(pf.Location)) {
            _missingProjects.Add(pf);
            pie.MissingProject = true;
        }
        pie.ProjectFile = pf;
        return pie;
    }
    
    public CategoryList NewCL(string name) {
        CategoryList clt = _CategoryList.Instance<CategoryList>();
        clt.Toggable = true;
        clt.CategoryName = name;
        return clt;
    }

    void ConnectHandlers(Node inode) {
        if (inode is ProjectLineEntry ple) {
            ple.Connect("Clicked", this, "OnListEntry_Clicked");
            ple.Connect("DoubleClicked", this, "OnListEntry_DoubleClicked");
            ple.Connect("RightClicked", this, "OnListEntry_RightClicked");
            ple.Connect("RightDoubleClicked", this, "OnListEntry_RightDoubleClicked");
        } else if (inode is ProjectIconEntry pie) {
            pie.Connect("Clicked", this, "OnIconEntry_Clicked");
            pie.Connect("DoubleClicked", this, "OnIconEntry_DoubleClicked");
            pie.Connect("RightClicked", this, "OnIconEntry_RightClicked");
            pie.Connect("RightDoubleClicked", this, "OnIconEntry_RightDoubleClicked");
        }
    }

    public void PopulateListing() {
        ProjectLineEntry ple;
        ProjectIconEntry pie;
        CategoryList clt;

        foreach(Node child in _listView.GetChildren()) {
            child.QueueFree();
        }
        foreach(Node child in _gridView.GetChildren()) {
            child.QueueFree();
        }
        foreach(CategoryList child in _categoryView.GetChildren()) {
            foreach(Node cchild in child.List.GetChildren()) {
                cchild.QueueFree();
            }
            child.QueueFree();
        }

        _categoryList.Clear();

        foreach(Category cat in CentralStore.Categories) {
            clt = NewCL(cat.Name);
            clt.SetMeta("ID",cat.Id);
            _categoryList[cat.Id] = clt;
            _categoryView.AddChild(clt);
        }

        clFavorites = NewCL("Favorites");
        clFavorites.SetMeta("ID", -1);
        _categoryView.AddChild(clFavorites);

        clUncategorized = NewCL("Un-Categorized");
        clUncategorized.SetMeta("ID",-2);
        _categoryView.AddChild(clUncategorized);

        foreach(ProjectFile pf in CentralStore.Projects) {
            ple = NewPLE(pf);
            pie = NewPIE(pf);
            _listView.AddChild(ple);
            _gridView.AddChild(pie);

            ConnectHandlers(ple);
            ConnectHandlers(pie);
            if (pf.CategoryId == -1) {
                clt = clUncategorized;
            } else {
                if (_categoryList.ContainsKey(pf.CategoryId))
                    clt = _categoryList[pf.CategoryId];
                else
                    clt = clUncategorized;
            }
            ple = clt.AddProject(pf);
            ConnectHandlers(ple);
        }
        if (_missingProjects.Count == 0)
            _actionButtons.SetHidden(6);
        else
            _actionButtons.SetVisible(6);
    }

    public void OnProjectCreated(ProjectFile pf) {
        PopulateListing();
        ExecuteEditorProject(pf.GodotVersion, pf.Location);
    }

    private void UpdateListExcept(ProjectLineEntry ple) {
        if (_listView.GetChildren().Contains(ple)) {
            foreach (ProjectLineEntry cple in _listView.GetChildren()) {
                if (cple != ple)
                    cple.SelfModulate = new Color("00ffffff");
            }
        } else {
            foreach (CategoryList cl in _categoryView.GetChildren()) {
                foreach(ProjectLineEntry cple in cl.List.GetChildren()) {
                    if (cple != ple)
                        cple.SelfModulate = new Color("00ffffff");
                }
            }
        }
    }

    void OnListEntry_Clicked(ProjectLineEntry ple) {
        UpdateListExcept(ple);
        _currentPLE = ple;
    }

    void OnListEntry_DoubleClicked(ProjectLineEntry ple) {
        if (ple.MissingProject)
            return;
        ExecuteEditorProject(ple.GodotVersion, ple.Location.GetBaseDir());
    }

    void OnListEntry_RightClicked(ProjectLineEntry ple) {
        _popupMenu.ProjectLineEntry = ple;
        _popupMenu.ProjectIconEntry = null;
        _popupMenu.Popup_(new Rect2(GetGlobalMousePosition(), _popupMenu.RectSize));
    }

    void OnListEntry_RightDoubleClicked(ProjectLineEntry ple) {

    }

    private void OnIconEntry_Clicked(ProjectIconEntry pie) {
        UpdateIconsExcept(pie);
        _currentPIE = pie;
    }

    private void OnIconEntry_DoubleClicked(ProjectIconEntry pie)
	{
        if (pie.MissingProject)
            return;
		ExecuteEditorProject(pie.GodotVersion, pie.Location.GetBaseDir());
	}

    void OnIconEntry_RightClicked(ProjectIconEntry pie) {
        _popupMenu.ProjectLineEntry = null;
        _popupMenu.ProjectIconEntry = pie;
        _popupMenu.Popup_(new Rect2(GetGlobalMousePosition(), _popupMenu.RectSize));
    }

    void OnIconEntry_RightDoubleClicked(ProjectIconEntry pie) {

    }


    public async void _IdPressed(int id) {
        if (_popupMenu.ProjectLineEntry != null) {
            ProjectLineEntry ple = _popupMenu.ProjectLineEntry;
            switch(id) {
                case 0:         // Open Project
                    ExecuteEditorProject(ple.GodotVersion, ple.ProjectFile.Location.GetBaseDir());
                    break;
                case 1:         // Run Project
                    ExecuteProject(ple.GodotVersion, ple.ProjectFile.Location.GetBaseDir());
                    break;
                case 2:         // Show Project Files
                    OS.ShellOpen(ple.ProjectFile.Location.GetBaseDir());
                    break;
				case 3:         // Show Project Data Folder
					string folder = GetProjectDataFolder(ple.ProjectFile);
                    OS.ShellOpen(folder);
					break;
				case 4:         // Edit Project file
                    // Handle Editing Certain Settings for ProjectFile
                    AppDialogs.EditProject.ShowDialog(ple.ProjectFile);
                    break;
                case 5:         // Remove Project
                    await RemoveProject(ple.ProjectFile);
                    break;
            }
        } else {
            ProjectIconEntry pie = _popupMenu.ProjectIconEntry;
            switch(id) {
                case 0:         // Open Project
                    ExecuteEditorProject(pie.GodotVersion, pie.ProjectFile.Location.GetBaseDir());
                    break;
                case 1:         // Run Project
                    ExecuteProject(pie.GodotVersion, pie.ProjectFile.Location.GetBaseDir());
                    break;
                case 2:         // Show Project Files
                    OS.ShellOpen(pie.ProjectFile.Location.GetBaseDir());
                    break;
                case 3:         // Show Project Data Folder
                    string folder = GetProjectDataFolder(pie.ProjectFile);
                    OS.ShellOpen(folder);
                    break;
                case 4:         // Edit Project file
                    // Handle Editing Certain Settings for ProjectFile
                    AppDialogs.EditProject.ShowDialog(pie.ProjectFile);
                    break;
                case 5:         // Remove Project
                    await RemoveProject(pie.ProjectFile);
                    break;
            }
        }
    }

    private void RemoveMissingProjects() {
        foreach (ProjectFile missing in _missingProjects) {
            CentralStore.Projects.Remove(missing);
        }
        CentralStore.Instance.SaveDatabase();
        _missingProjects.Clear();
        PopulateListing();
    }

	private string GetProjectDataFolder(ProjectFile pf)
	{
		ConfigFile cf = new ConfigFile();
		cf.Load(pf.Location);
		string folder = "";
		if (cf.HasSectionKey("application", "config/use_custom_user_dir"))
		{
			if ((bool)cf.GetValue("application", "config/use_custom_user_dir") == true)
			{
#if GODOT_WINDOWS || GODOT_UWP
				folder = OS.GetEnvironment("APPDATA");
#elif GODOT_LINUXBSD || GODOT_X11
                            folder = "~/.local/share";
#elif GODOT_MACOS || GODOT_OSX
                            folder = "~/Library/Application Support";
#endif
				folder = folder.PlusFile((string)cf.GetValue("application", "config/custom_user_dir_name"));
			} else {
#if GODOT_WINDOWS || GODOT_UWP
                folder = OS.GetEnvironment("APPDATA").PlusFile("Godot").PlusFile("app_userdata");
#elif GODOT_LINUXBSD || GODOT_X11
                folder = "~/local/share/godot/app_userdata";
#elif GODOT_MACOS || GODOT_OSX
                folder = "~/Library/Application Support/Godot/app_userdata";
#endif
			    folder = folder.PlusFile(pf.Name);                
            }
		}
		else
		{
#if GODOT_WINDOWS || GODOT_UWP
			folder = OS.GetEnvironment("APPDATA").PlusFile("Godot").PlusFile("app_userdata");
#elif GODOT_LINUXBSD || GODOT_X11
            folder = "~/local/share/godot/app_userdata";
#elif GODOT_MACOS || GODOT_OSX
            folder = "~/Library/Application Support/Godot/app_userdata";
#endif
			folder = folder.PlusFile(pf.Name);
		}
        return folder;
	}

	private void ExecuteProject(string godotVersion, string location)
	{
		GodotVersion gv = CentralStore.Instance.FindVersion(godotVersion);
        if (gv == null)
            return;
        
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = gv.GetExecutablePath().GetOSDir();
        psi.Arguments = $"--path {location}";
        psi.WorkingDirectory = location;
        psi.UseShellExecute = !CentralStore.Settings.NoConsole;
        psi.CreateNoWindow = CentralStore.Settings.NoConsole;

        Process proc = Process.Start(psi);
	}

	private void UpdateIconsExcept(ProjectIconEntry pie) {
        foreach(ProjectIconEntry cpie in _gridView.GetChildren()) {
            if (cpie != pie)
                cpie.SelfModulate = new Color("00FFFFFF");
        }
    }

	private void ExecuteEditorProject(string godotVersion, string location)
	{
		GodotVersion gv = CentralStore.Instance.FindVersion(godotVersion);
		if (gv == null)
			return;
        
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = gv.GetExecutablePath().GetOSDir();
        psi.Arguments = $"--path {location} -e";
        psi.WorkingDirectory = location;
        psi.UseShellExecute = !CentralStore.Settings.NoConsole;
        psi.CreateNoWindow = CentralStore.Settings.NoConsole;
        
        Process proc = Process.Start(psi);
        if (CentralStore.Settings.CloseManagerOnEdit) {
            GetTree().Quit(0);
        }
	}

    [SignalHandler("clicked", nameof(_actionButtons))]
	async void OnActionButtons_Clicked(int index) {
        switch (index) {
            case 0: // New Project File
                AppDialogs.CreateProject.ShowDialog();
                break;
            case 1: // Import Project File
                AppDialogs.ImportProject.ShowDialog();
                break;
            case 2: // Scan Project Folder
                break;
            case 3: // Add Category
                AppDialogs.CreateCategory.ShowDialog();
                break;
            case 4: // Remove Category
                AppDialogs.RemoveCategory.ShowDialog();
                break;
			case 5: // Remove Project (May be removed completely)
				ProjectFile pf = null;
				if (_currentView == View.GridView)
				{
					if (_currentPIE != null)
						pf = _currentPIE.ProjectFile;
				}
				else
				{
					if (_currentPLE != null)
						pf = _currentPLE.ProjectFile;
				}

				if (pf == null)
					return;

				await RemoveProject(pf);
				break;
            case 6:
                var res = AppDialogs.YesNoDialog.ShowDialog("Remove Missing Projects...", "Are you usre you want to remove any missing projects?");
                await res;
                if (res.Result)
                    RemoveMissingProjects();
                break;
		}
    }

	private async Task RemoveProject(ProjectFile pf)
	{
		var task = AppDialogs.YesNoCancelDialog.ShowDialog("Remove Project", $"You are about to remove Project {pf.Name}.\nDo you wish to remove the files as well?",
			"Project and Files", "Just Project");
		while (!task.IsCompleted)
			await this.IdleFrame();
		switch (task.Result)
		{
			case YesNoCancelDialog.ActionResult.FirstAction:
				string path = pf.Location.GetBaseDir();
				RemoveFolders(path);
				CentralStore.Projects.Remove(pf);
				CentralStore.Instance.SaveDatabase();
				PopulateListing();
				break;
			case YesNoCancelDialog.ActionResult.SecondAction:
				CentralStore.Projects.Remove(pf);
				CentralStore.Instance.SaveDatabase();
				PopulateListing();
				break;
			case YesNoCancelDialog.ActionResult.CancelAction:
				AppDialogs.MessageDialog.ShowMessage("Remove Project", "Remove Project has been cancelled.");
				break;
		}
	}

	void RemoveFolders(string path) {
        Directory dir = new Directory();
        if (dir.Open(path) == Error.Ok) {
            dir.ListDirBegin(true, false);
            var filename = dir.GetNext();
            while (filename != "") {
                if (dir.CurrentIsDir()) {
                    RemoveFolders(path.PlusFile(filename).NormalizePath());
                }
                dir.Remove(filename);
                filename = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        dir.Open(path.GetBaseDir());
        dir.Remove(path.GetFile());
    }

    [SignalHandler("Clicked", nameof(_viewSelector))]
    void OnViewSelector_Clicked(int page) {
        for (int i = 0; i < _views.Count; i++) {
            if (i == page)
                _views[i].Show();
            else
                _views[i].Hide();
        }
        if (page == 2) {
            _actionButtons.SetVisible(3);
            _actionButtons.SetVisible(4);
        } else {
            _actionButtons.SetHidden(3);
            _actionButtons.SetHidden(4);
        }
        _currentView = (View)page;
        CentralStore.Settings.LastView = Views[page];
    }

    public Array<ProjectFile> TestSortListing() {
        Array<ProjectFile> projectFiles = new Array<ProjectFile>();
        var pfolder = CentralStore.Projects.OrderByDescending(pf => pf.LastAccessed);
        foreach (ProjectFile pf in pfolder)
            projectFiles.Add(pf);
        return projectFiles;
    }

    public Array<ProjectFile> TestFavSortListing() {
        Array<ProjectFile> projectFiles = new Array<ProjectFile>();
        var fav = CentralStore.Projects.Where(pf => pf.Favorite == true).OrderByDescending(pf => pf.LastAccessed);
        var non_fav = CentralStore.Projects.Where(pf => pf.Favorite != true).OrderByDescending(pf => pf.LastAccessed);

        foreach(ProjectFile pf in fav)
            projectFiles.Add(pf);
        
        foreach(ProjectFile pf in non_fav)
            projectFiles.Add(pf);
        
        return projectFiles;
    }

    void OnButton_Pressed() {
        //AddTestProjects();
        
        Array<ProjectFile> projectFiles = TestFavSortListing(); //TestSortListing();

        foreach(ProjectFile pf in projectFiles) {
            GD.Print($"Project: {pf.Name}, Last Accessed: {pf.LastAccessed}");
        }
        
    }
}   