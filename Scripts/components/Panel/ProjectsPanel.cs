using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using FPath = System.IO.Path;
using Dir = System.IO.Directory;
using SFile = System.IO.File;
using DateTime = System.DateTime;

public class ProjectsPanel : Panel
{
    #region Node Accessors
    [NodePath("VC/MC/HC/ActionButtons")]
    ActionButtons _actionButtons = null;
    [NodePath("VC/SC")]
    ScrollContainer _scrollContainer = null;
    [NodePath("VC/ProjectSort")]
    MarginContainer _projectSort = null;
    [NodePath("VC/ProjectSort/PC/HeaderButtons/ProjectName")]
    HeaderButton _projectName = null;
    [NodePath("VC/ProjectSort/PC/HeaderButtons/GodotVersion")]
    HeaderButton _godotVersion = null;
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
    enum View
    {
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

    private Array<string> Views = null;

    Dictionary<ProjectFile, ProjectLineEntry> pleCache;
    Dictionary<ProjectFile, ProjectIconEntry> pieCache;
    Dictionary<Category, CategoryList> catCache;
    Dictionary<CategoryList, Dictionary<ProjectFile, ProjectLineEntry>> cpleCache;

    bool dragging = false;
    float _topBorder = 0.0f;
    float _bottomBorder = 0.0f;
    float _borderSize = 50.0f;
    float _scrollSpeed = 0.0f;

    Timer _scrollTimer = null;
    Tween _scrollTween = null;
    #endregion

    Array<Container> _views;

    public override void _Ready()
    {
        this.OnReady();
        Views = new Array<string> {
            Tr("List View"),
            Tr("Icon View"),
            Tr("Category View")
        };

        _views = new Array<Container>();
        _views.Add(_listView);
        _views.Add(_gridView);
        _views.Add(_categoryView);

        _popupMenu = GD.Load<PackedScene>("res://components/ProjectPopup.tscn").Instance<ProjectPopup>();

        // Translations for Menu Items
        _popupMenu.UpdateTr(0, Tr("Open"));
        _popupMenu.UpdateTr(1, Tr("Run"));
        _popupMenu.UpdateTr(2, Tr("Show Project Files"));
        _popupMenu.UpdateTr(3, Tr("Show Data Folder"));
        _popupMenu.UpdateTr(4, Tr("Edit"));
        _popupMenu.UpdateTr(5, Tr("Remove"));
        AddChild(_popupMenu);
        _popupMenu.SetAsToplevel(true);

        AppDialogs.ImportProject.Connect("update_projects", this, "PopulateListing");
        AppDialogs.CreateCategory.Connect("update_categories", this, "PopulateListing");
        AppDialogs.RemoveCategory.Connect("update_categories", this, "PopulateListing");
        AppDialogs.EditProject.Connect("project_updated", this, "PopulateListing");
        AppDialogs.CreateProject.Connect("project_created", this, "OnProjectCreated");

        GetTree().Connect("files_dropped", this, "OnFilesDropped");

        _actionButtons.SetHidden(3);
        _actionButtons.SetHidden(4);
        _categoryList = new Dictionary<int, CategoryList>();
        _missingProjects = new Array<ProjectFile>();

        if (_viewSelector.SelectedView != -1)
        {
            if (CentralStore.Settings.DefaultView == Tr("Last View Used"))
            {
                int indx = Views.IndexOf(CentralStore.Settings.LastView);
                _viewSelector.SetView(indx);
                OnViewSelector_Clicked(indx);
            }
            else
            {
                int indx = Views.IndexOf(CentralStore.Settings.DefaultView);
                _viewSelector.SetView(indx);
                OnViewSelector_Clicked(indx);
            }
        }

        if (CentralStore.Settings.EnableAutoScan)
        {
            WaitForReady(ScanForProjects);
        }

        _topBorder = _scrollContainer.RectGlobalPosition.y + _borderSize;
        _bottomBorder = _scrollContainer.RectSize.y - _borderSize;

        _scrollTimer = new Timer();
        AddChild(_scrollTimer);
        _scrollTimer.Connect("timeout", this, "OnScrollTimer");

        _scrollTween = new Tween();
        AddChild(_scrollTween);

        PopulateListing();
    }

    private async void WaitForReady(System.Action runProc)
    {
        while (AppDialogs.BusyDialog.Header == null)
        {
            await this.IdleFrame();
        }

        runProc.Invoke();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseMotion iemmEvent)
        {
            if (!dragging)
                return;
            if (iemmEvent.Position.y <= _topBorder)
            {
                _scrollSpeed = Mathf.Clamp(iemmEvent.Position.y - _topBorder, -_borderSize, 0.0f);
                if (_scrollSpeed == -_borderSize)
                    _scrollSpeed *= 2;
            }
            else if (iemmEvent.Position.y >= _bottomBorder)
            {
                _scrollSpeed = Mathf.Clamp(iemmEvent.Position.y - _bottomBorder, 0.0f, _borderSize);
                if (_scrollSpeed == _borderSize)
                    _scrollSpeed *= 2;
            }
            else if (_scrollSpeed != 0)
            {
                _scrollSpeed = 0;
            }
        }
    }

    public void OnFilesDropped(string[] files, int screen)
    {
        var file = new System.IO.FileInfo(files[0]);
        if (file.Exists && file.Name.Equals("project.godot"))
            AppDialogs.ImportProject.ShowDialog(files[0]);
    }

    [SignalHandler("direction_changed", nameof(_projectName))]
    void OnDirChanged_ProjectName(HeaderButton.SortDirection @dir)
    {
        PopulateListing();
    }

    [SignalHandler("direction_changed", nameof(_godotVersion))]
    void OnDirChanged_GodotVersion(HeaderButton.SortDirection @dir)
    {
        PopulateListing();
    }

    void OnScrollTimer()
    {
        if (!dragging)
            return;
        if (_scrollSpeed == 0)
            return;
        if (_scrollContainer.ScrollVertical == 0 && _scrollSpeed < 0)
            return;
        if (_scrollContainer.ScrollVertical == _scrollContainer.GetVScrollbar().MaxValue && _scrollSpeed > 0)
            return;
        if (_scrollTween.IsActive())
            _scrollTween.StopAll();
        _scrollTween.InterpolateProperty(
            _scrollContainer,
            "scroll_vertical",
            _scrollContainer.ScrollVertical,
            _scrollContainer.ScrollVertical + _scrollSpeed,
            0.24f,
            Tween.TransitionType.Linear,
            Tween.EaseType.Out);
        _scrollTween.Start();
        //_scrollContainer.ScrollVertical += (int)_scrollSpeed;
    }

    public ProjectLineEntry NewPLE(ProjectFile pf)
    {
        ProjectLineEntry ple = _ProjectLineEntry.Instance<ProjectLineEntry>();
        UpdatePLE(ple, pf);
        return ple;
    }

    private void UpdatePLE(ProjectLineEntry ple, ProjectFile pf)
    {
        if (_missingProjects.Contains(pf))
            ple.MissingProject = true;
        else if (!ProjectFile.ProjectExists(pf.Location))
        {
            _missingProjects.Add(pf);
            ple.MissingProject = true;
        }
        else
        {
            ple.MissingProject = false;
        }
        ple.ProjectFile = pf;
    }

    public void UpdatePLE(ProjectFile pf)
    {
        if (pleCache.ContainsKey(pf))
            UpdatePLE(pleCache[pf], pf);

        foreach (var cat in cpleCache.Keys)
        {
            if (cpleCache[cat].ContainsKey(pf))
                UpdatePLE(cpleCache[cat][pf], pf);
        }
    }

    public ProjectIconEntry NewPIE(ProjectFile pf)
    {
        ProjectIconEntry pie = _ProjectIconEntry.Instance<ProjectIconEntry>();
        UpdatePIE(pie, pf);
        return pie;
    }

    private void UpdatePIE(ProjectIconEntry pie, ProjectFile pf)
    {
        if (_missingProjects.Contains(pf))
            pie.MissingProject = true;
        else if (!ProjectFile.ProjectExists(pf.Location))
        {
            _missingProjects.Add(pf);
            pie.MissingProject = true;
        }
        else
        {
            pie.MissingProject = false;
        }
        pie.ProjectFile = pf;
    }

    public void UpdatePIE(ProjectFile pf)
    {
        if (pieCache.ContainsKey(pf))
        {
            UpdatePIE(pieCache[pf], pf);
        }
    }

    public CategoryList NewCL(string name)
    {
        CategoryList clt = _CategoryList.Instance<CategoryList>();
        clt.Toggable = true;
        clt.CategoryName = name;
        return clt;
    }

    void ConnectHandlers(Node inode, bool isCategory = false)
    {
        if (inode is ProjectLineEntry ple)
        {
            ple.Connect("Clicked", this, "OnListEntry_Clicked");
            ple.Connect("DoubleClicked", this, "OnListEntry_DoubleClicked");
            ple.Connect("RightClicked", this, "OnListEntry_RightClicked");
            ple.Connect("RightDoubleClicked", this, "OnListEntry_RightDoubleClicked");
            ple.Connect("FavoriteUpdated", this, "OnListEntry_FavoriteUpdated");
            if (isCategory)
            {
                ple.Connect("DragStarted", this, "OnDragStarted");
                ple.Connect("DragEnded", this, "OnDragEnded");
            }
        }
        else if (inode is ProjectIconEntry pie)
        {
            pie.Connect("Clicked", this, "OnIconEntry_Clicked");
            pie.Connect("DoubleClicked", this, "OnIconEntry_DoubleClicked");
            pie.Connect("RightClicked", this, "OnIconEntry_RightClicked");
            pie.Connect("RightDoubleClicked", this, "OnIconEntry_RightDoubleClicked");
        }
    }

    Array<string> RecursiveScan(string path, string file)
    {
        var files = new Array<string>();
        var dirHandle = new Directory();
        if (dirHandle.Open(path) == Error.Ok)
        {
            dirHandle.ListDirBegin(true);
            var newPath = dirHandle.GetNext();
            while (!string.IsNullOrEmpty(newPath))
            {
                if (dirHandle.DirExists(newPath))
                {
                    var newFiles = RecursiveScan(newPath, file);
                    foreach (var x in newFiles) files.Add(x);
                }
                if (dirHandle.FileExists(newPath.Join(file)))
                    files.Add(path.Join(newPath, file));
                newPath = dirHandle.GetNext();
            }
            dirHandle.ListDirEnd();
        }

        dirHandle.Dispose();
        return files;
    }

    async Task<Array<string>> ScanDirectories(Array<string> scanDirs)
    {
        Array<string> projects = new Array<string>();

        await this.IdleFrame();

        foreach (string dir in scanDirs)
        {
            var projs = RecursiveScan(dir, "project.godot");
            foreach (var proj in projs)
                projects.Add(proj);
        }
        return projects;
    }

    private async Task<Array<string>> UpdateProjects(Array<string> projs)
    {
        Array<string> added = new Array<string>();
        await this.IdleFrame();

        foreach (string proj in projs)
        {
            AppDialogs.BusyDialog.UpdateByline($"Processing {projs.IndexOf(proj)}/{projs.Count}...");
            await this.IdleFrame();
            if (CentralStore.Instance.HasProject(proj.NormalizePath()))
            {
                await this.IdleFrame();
            }
            else
            {
                ProjectFile pf;
                try
                {
                    pf = ProjectFile.ReadFromFile(proj.NormalizePath());
                }
                catch
                {
                    pf = null;
                }

                if (pf == null) continue;
                pf.GodotVersion = CentralStore.Settings.DefaultEngine;
                CentralStore.Projects.Add(pf);
                added.Add(proj);
            }
        }
        return added;
    }

    private async void ScanForProjects()
    {
        Array<string> projects = new Array<string>();
        Array<string> scanDirs = CentralStore.Settings.ScanDirs.Duplicate();
        int i = 0;

        while (i < scanDirs.Count)
        {
            if (!Dir.Exists(scanDirs[i]))
                scanDirs.RemoveAt(i);
            else
                i++;
        }

        if (scanDirs.Count == 0)
        {
            var res = AppDialogs.YesNoDialog.ShowDialog(Tr("Scan Project Folders"),
                Tr("There are currently no valid Directories to scan, would you like to add one?"));
            while (!res.IsCompleted)
                await this.IdleFrame();

            if (res.Result)
            {
                AppDialogs.BrowseFolderDialog.CurrentFile = "";
                AppDialogs.BrowseFolderDialog.CurrentPath = CentralStore.Settings.ProjectPath;
                AppDialogs.BrowseFolderDialog.PopupCentered();
                AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnScanProjects_DirSelected", null, (int)ConnectFlags.Oneshot);
                AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnScanProjects_PopupHide", null, (int)ConnectFlags.Oneshot);
                return;
            }
            else
                return;
        }

        AppDialogs.BusyDialog.UpdateHeader(Tr("Scanning for Projects..."));
        AppDialogs.BusyDialog.UpdateByline(Tr("Scanning for Project files...."));
        AppDialogs.BusyDialog.ShowDialog();

        var projsTask = ScanDirectories(scanDirs);
        while (!projsTask.IsCompleted)
            await this.IdleFrame();

        AppDialogs.BusyDialog.UpdateByline(string.Format(Tr("Processing 0/{0}..."), projsTask.Result.Count));

        var addedTask = UpdateProjects(projsTask.Result);
        while (!addedTask.IsCompleted)
            await this.IdleFrame();

        AppDialogs.BusyDialog.HideDialog();
        if (addedTask.Result.Count == 0)
            AppDialogs.MessageDialog.ShowMessage(Tr("Scan Projects"), Tr("No new projects found."));
        else
        {
            AppDialogs.MessageDialog.ShowMessage(Tr("Scan Projects"),
                string.Format(Tr("Found {0} new projects, and added to database."), addedTask.Result.Count));
            CentralStore.Instance.SaveDatabase();
            PopulateListing();
        }
    }

    void OnScanProjects_DirSelected(string path)
    {
        CentralStore.Settings.ScanDirs.Clear();
        CentralStore.Settings.ScanDirs.Add(path);
        CentralStore.Instance.SaveDatabase();
        AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnScanProjects_DirSelected");
        ScanForProjects();
        PopulateListing();
    }

    void OnScanProjects_PopupHide()
    {
        if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnScanProjects_DirSelected"))
            AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnScanProjects_DirSelected");
    }

    // Optimizing PopulateListing() to utilize Cache of Nodes, adding and removing only as
    // CentralStore changes.  Sorting will happen AFTER all nodes have been generated, and will
    // be added / ordered from the cache to the display controls.
    // (At Least in ListView, Icon and Project no such sorting, but will ensure all updates will be
    // executed at a faster pace, then previous method of Freeing all, and Re-creating. )
    public void PopulateListing()
    {

        // Initialize if not initialized
        if (pleCache == null)
            pleCache = new Dictionary<ProjectFile, ProjectLineEntry>();
        if (pieCache == null)
            pieCache = new Dictionary<ProjectFile, ProjectIconEntry>();
        if (catCache == null)
            catCache = new Dictionary<Category, CategoryList>();
        if (cpleCache == null)
            cpleCache = new Dictionary<CategoryList, Dictionary<ProjectFile, ProjectLineEntry>>();

        ProjectLineEntry ple;
        ProjectIconEntry pie;
        CategoryList clt;

        // Create our Categories
        foreach (Category cat in CentralStore.Categories)
        {
            if (catCache.ContainsKey(cat))
                continue;
            clt = NewCL(cat.Name);
            clt.Pinned = CentralStore.Instance.IsCategoryPinned(cat);
            clt.SetMeta("ID", cat.Id);
            clt.Toggled = cat.IsExpanded;
            clt.Pinnable = true;
            _categoryList[cat.Id] = clt;
            catCache[cat] = clt;
            cpleCache[clt] = new Dictionary<ProjectFile, ProjectLineEntry>();
            _categoryView.AddChild(clt);
            clt.Connect("list_toggled", this, "OnCategoryListToggled", new Array { clt });
            clt.Connect("pin_toggled", this, "OnCategoryPinned", new Array() { clt });
            clt.Connect("drag_drop_completed", this, "OnDragDropCompleted");
        }

        if (clFavorites == null)
        {
            clFavorites = NewCL("Favorites");
            clFavorites.SetMeta("ID", -1);
            clFavorites.Toggled = CentralStore.Settings.FavoritesToggled;
            _categoryView.AddChild(clFavorites);
            clFavorites.Connect("list_toggled", this, "OnCategoryListToggled", new Array { clFavorites });
            clFavorites.Connect("drag_drop_completed", this, "OnDragDropCompleted");
            cpleCache[clFavorites] = new Dictionary<ProjectFile, ProjectLineEntry>();
        }

        if (clUncategorized == null)
        {
            clUncategorized = NewCL("Un-Categorized");
            clUncategorized.SetMeta("ID", -2);
            clUncategorized.Toggled = CentralStore.Settings.UncategorizedToggled;
            _categoryView.AddChild(clUncategorized);
            clUncategorized.Connect("list_toggled", this, "OnCategoryListToggled", new Array { clUncategorized });
            clUncategorized.Connect("drag_drop_completed", this, "OnDragDropCompleted");
            cpleCache[clUncategorized] = new Dictionary<ProjectFile, ProjectLineEntry>();
        }

        var updatedNullVersions = false;

        // Create our Project Entries
        foreach (ProjectFile pf in CentralStore.Projects)
        {
            if (string.IsNullOrEmpty(pf.GodotVersion))
            {
                if (string.IsNullOrEmpty(CentralStore.Settings.DefaultEngine))
                {
                    if (CentralStore.Versions.Count > 0)
                    {
                        var vers = CentralStore.Versions[0];
                        pf.GodotVersion = vers.Id;
                        updatedNullVersions = true;
                    }
                }
                else
                {
                    pf.GodotVersion = CentralStore.Settings.DefaultEngine;
                    updatedNullVersions = true;
                }
            }
            clt = null;
            if (!pleCache.ContainsKey(pf))
            {
                ple = NewPLE(pf);
                pleCache[pf] = ple;
                ConnectHandlers(ple);
            }

            if (!pieCache.ContainsKey(pf))
            {
                pie = NewPIE(pf);
                pieCache[pf] = pie;
                ConnectHandlers(pie);
            }

            if (_categoryList.ContainsKey(pf.CategoryId))
            {
                clt = _categoryList[pf.CategoryId];
            }

            if (clt == null && pf.Favorite)
                clt = clFavorites;

            if (clt == null)
                clt = clUncategorized;

            if (!cpleCache[clt].ContainsKey(pf))
            {
                ple = clt.AddProject(pf);
                cpleCache[clt][pf] = ple;
                ConnectHandlers(ple, true);
            }
        }

        if (updatedNullVersions)
            CentralStore.Instance.SaveDatabase();

        // Clean up of Project Entries
        foreach (ProjectFile pf in pleCache.Keys)
        {
            if (!CentralStore.Projects.Contains(pf))
            {
                pleCache[pf].QueueFree();
                pleCache.Remove(pf);
            }
        }

        foreach (ProjectFile pf in pieCache.Keys)
        {
            if (!CentralStore.Projects.Contains(pf))
            {
                pieCache[pf].QueueFree();
                pieCache.Remove(pf);
            }
        }

        // Cleanup of Categories
        foreach (CategoryList cclt in cpleCache.Keys)
        {
            foreach (ProjectFile pf in cpleCache[cclt].Keys)
            {
                if (!CentralStore.Projects.Contains(pf))
                {
                    cpleCache[cclt][pf].QueueFree();
                    cpleCache[cclt].Remove(pf);
                }
                else if (cclt == clFavorites && !pf.Favorite && cpleCache[cclt].Keys.Contains(pf))
                {
                    cpleCache[cclt][pf].QueueFree();
                    cpleCache[cclt].Remove(pf);
                }
            }

            if (cclt != clFavorites && cclt != clUncategorized)
            {
                int CatID = (int)cclt.GetMeta("ID");
                if (!CentralStore.Instance.HasCategoryId(CatID))
                {
                    Category cCat = null;
                    foreach (Category cat in catCache.Keys)
                    {
                        if (catCache[cat] == cclt)
                            cCat = cat;
                    }

                    if (cCat != null)
                    {
                        cpleCache.Remove(cclt);
                        catCache.Remove(cCat);
                        _categoryList.Remove(CatID);
                    }
                }
            }

            if (cclt == clUncategorized)
            {
                foreach (ProjectFile pf in cpleCache[cclt].Keys)
                {
                    if (pf.Favorite)
                    {
                        cpleCache[cclt][pf].QueueFree();
                        cpleCache[cclt].Remove(pf);
                    }
                }
            }

            cclt.SortListing();
        }

        PopulateSort();

        if (_missingProjects.Count == 0)
        {
            _actionButtons.SetHidden(6);
            _actionButtons.SetHidden(7);
        }
        else
        {
            _actionButtons.SetVisible(6);
            _actionButtons.SetVisible(7);
        }
    }

    public async void OnDragDropCompleted(CategoryList source, CategoryList destination, ProjectLineEntry ple)
    {
        if (cpleCache.ContainsKey(source) && cpleCache.ContainsKey(destination))
        {
            if (cpleCache[source].ContainsKey(ple.ProjectFile) && !cpleCache[destination].ContainsKey(ple.ProjectFile))
            {
                cpleCache[source].Remove(ple.ProjectFile);
                cpleCache[destination][ple.ProjectFile] = ple;

                if (destination == clUncategorized && ple.ProjectFile.Favorite)
                {
                    await this.IdleFrame();
                    ple.EmitSignal("FavoriteUpdated", ple);
                }
            }
        }
    }

    public void PopulateSort()
    {
        foreach (Node node in _listView.GetChildren())
            _listView.RemoveChild(node);

        foreach (Node node in _gridView.GetChildren())
            _gridView.RemoveChild(node);

        foreach (Node node in _categoryView.GetChildren())
            _categoryView.RemoveChild(node);

        foreach (Category cat in CentralStore.Instance.GetPinnedCategories())
            _categoryView.AddChild(catCache[cat]);

        foreach (Category cat in CentralStore.Instance.GetUnpinnedCategories())
            _categoryView.AddChild(catCache[cat]);

        _categoryView.AddChild(clFavorites);
        _categoryView.AddChild(clUncategorized);

        foreach (IOrderedEnumerable<ProjectFile> apf in SortListing())
        {
            foreach (ProjectFile pf in apf)
                _listView.AddChild(pleCache[pf]);
        }

        foreach (IOrderedEnumerable<ProjectFile> apf in SortListing(true))
        {
            foreach (ProjectFile pf in apf)
                _gridView.AddChild(pieCache[pf]);
        }
    }

    public void OnProjectCreated(ProjectFile pf)
    {
        PopulateListing();
        ExecuteEditorProject(pf);
    }

    private void UpdateListExcept(ProjectLineEntry ple)
    {
        if (_listView.GetChildren().Contains(ple))
        {
            foreach (ProjectLineEntry cple in _listView.GetChildren())
            {
                if (cple != ple)
                    cple.SelfModulate = new Color("00ffffff");
            }
        }
        else
        {
            foreach (CategoryList cl in _categoryView.GetChildren())
            {
                foreach (ProjectLineEntry cple in cl.List.GetChildren())
                {
                    if (cple != ple)
                        cple.SelfModulate = new Color("00ffffff");
                }
            }
        }
    }

    void OnCategoryListToggled(CategoryList clt)
    {
        int id = (int)clt.GetMeta("ID");
        if (id == -1 || id == -2)
        {
            if (id == -1)
                CentralStore.Settings.FavoritesToggled = clt.Toggled;
            else
                CentralStore.Settings.UncategorizedToggled = clt.Toggled;
            CentralStore.Instance.SaveDatabase();
            return;
        }
        Category cat = CentralStore.Categories.Where(x => x.Id == id).FirstOrDefault<Category>();
        if (cat == null)
            return;

        cat.IsExpanded = clt.Toggled;
        CentralStore.Instance.SaveDatabase();
    }

    void OnCategoryPinned(CategoryList clt)
    {
        int id = (int)clt.GetMeta("ID");
        Category cat = CentralStore.Categories.Where(x => x.Id == id).FirstOrDefault<Category>();
        if (cat == null)
            return;

        if (clt.Pinned)
        {
            CentralStore.Instance.PinCategory(cat);
        }
        else
        {
            CentralStore.Instance.UnpinCategory(cat);
        }

        PopulateSort();
        CentralStore.Instance.SaveDatabase();
    }

    void OnListEntry_Clicked(ProjectLineEntry ple)
    {
        UpdateListExcept(ple);
        _currentPLE = ple;
    }

    void OnListEntry_DoubleClicked(ProjectLineEntry ple)
    {
        if (ple.MissingProject)
            return;
        ple.ProjectFile.LastAccessed = DateTime.UtcNow;
        ExecuteEditorProject(ple.ProjectFile);
    }

    void OnListEntry_RightClicked(ProjectLineEntry ple)
    {
        _popupMenu.ProjectLineEntry = ple;
        _popupMenu.ProjectIconEntry = null;
        _popupMenu.Popup_(new Rect2(GetGlobalMousePosition(), _popupMenu.RectSize));
    }

    void OnListEntry_RightDoubleClicked(ProjectLineEntry ple)
    {

    }

    void OnListEntry_FavoriteUpdated(ProjectLineEntry ple)
    {
        PopulateListing();
    }

    void OnDragStarted(ProjectLineEntry ple)
    {
        dragging = true;
        _scrollTimer.Start(0.25f);
    }

    void OnDragEnded(ProjectLineEntry ple)
    {
        dragging = false;
        _scrollTimer.Stop();
    }

    private void OnIconEntry_Clicked(ProjectIconEntry pie)
    {
        UpdateIconsExcept(pie);
        _currentPIE = pie;
    }

    private void OnIconEntry_DoubleClicked(ProjectIconEntry pie)
    {
        if (pie.MissingProject)
            return;
        pie.ProjectFile.LastAccessed = DateTime.UtcNow;
        ExecuteEditorProject(pie.ProjectFile);
    }

    void OnIconEntry_RightClicked(ProjectIconEntry pie)
    {
        _popupMenu.ProjectLineEntry = null;
        _popupMenu.ProjectIconEntry = pie;
        _popupMenu.Popup_(new Rect2(GetGlobalMousePosition(), _popupMenu.RectSize));
    }

    void OnIconEntry_RightDoubleClicked(ProjectIconEntry pie)
    {

    }


    public async void _IdPressed(int id)
    {
        ProjectFile pf;
        if (_popupMenu.ProjectLineEntry != null)
        {
            pf = _popupMenu.ProjectLineEntry.ProjectFile;
            _currentPLE = _popupMenu.ProjectLineEntry;
        }
        else
        {
            pf = _popupMenu.ProjectIconEntry.ProjectFile;
            _currentPIE = _popupMenu.ProjectIconEntry;
        }
        switch (id)
        {
            case 0:     // Open Project
                ExecuteEditorProject(pf);
                break;
            case 1:     // Run Project
                ExecuteProject(pf);
                break;
            case 2:     // Show Project Files
                OS.ShellOpen("file://" + pf.Location.GetBaseDir());
                break;
            case 3:     // Show Project Data Folder
                string folder = GetProjectDataFolder(pf);
                if (Dir.Exists(folder))
                    OS.ShellOpen("file://" + folder);
                else
                    AppDialogs.MessageDialog.ShowMessage(Tr("Show Data Directory"),
                        string.Format(Tr("The data directory {0} does not exist!"), folder));
                break;
            case 4:     // Edit Project File
                AppDialogs.EditProject.Connect("project_updated", this, "OnProjectUpdated", new Array { pf });
                AppDialogs.EditProject.Connect("hide", this, "OnHide_EditProject");
                AppDialogs.EditProject.ShowDialog(pf);
                break;
            case 5:     // Remove Project
                await RemoveProject(pf);
                break;
        }
    }

    private void OnProjectUpdated(ProjectFile pf)
    {
        var ple = pleCache.Where(x => x.Key == pf).Select(x => x.Value).FirstOrDefault<ProjectLineEntry>();
        if (ple != null)
        {
            ple.ProjectFile = pf;
        }

        foreach (CategoryList cat in cpleCache.Keys)
        {
            ple = cpleCache[cat].Where(x => x.Key == pf).Select(x => x.Value).FirstOrDefault<ProjectLineEntry>();
            if (ple != null)
                ple.ProjectFile = pf;
        }

        var pie = pieCache.Where(x => x.Key == pf).Select(x => x.Value).FirstOrDefault<ProjectIconEntry>();
        if (pie != null)
            pie.ProjectFile = pf;
    }

    private void OnHide_EditProject()
    {
        AppDialogs.EditProject.Disconnect("project_updated", this, "OnProjectUpdated");
        AppDialogs.EditProject.Disconnect("hide", this, "OnHide_EditProject");
    }

    private void RemoveMissingProjects()
    {
        foreach (ProjectFile missing in _missingProjects)
        {
            CentralStore.Projects.Remove(missing);
        }
        CentralStore.Instance.SaveDatabase();
        _missingProjects.Clear();
        PopulateListing();
    }

    private string GetProjectDataFolder(ProjectFile pf)
    {
        ProjectConfig pc = new ProjectConfig();
        pc.Load(pf.Location);
        string folder = "";
        if (pc.HasSectionKey("application", "config/use_custom_user_dir"))
        {
            if (pc.GetValue("application", "config/use_custom_user_dir") == "true")
            {
                folder = OS.GetDataDir().Join(pc.GetValue("application", "config/custom_user_dir_name"));
            }
            else
            {
                folder = OS.GetDataDir().Join("godot", "app_userdata", pf.Name);
            }
        }
        else
        {
            folder = OS.GetDataDir().Join("Godot", "app_userdata", pf.Name);
        }
        return folder.NormalizePath();
    }

    private void ExecuteProject(ProjectFile pf)
    {
        GodotVersion gv = CentralStore.Instance.FindVersion(pf.GodotVersion);
        if (gv == null)
            return;

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = gv.GetExecutablePath().GetOSDir();
        psi.Arguments = $"--path \"{pf.Location.GetBaseDir()}\"";
        psi.WorkingDirectory = pf.Location.GetBaseDir().GetOSDir().NormalizePath();
        psi.UseShellExecute = !CentralStore.Settings.NoConsole;
        psi.CreateNoWindow = CentralStore.Settings.NoConsole;

        Process proc = Process.Start(psi);
    }

    private void UpdateIconsExcept(ProjectIconEntry pie)
    {
        foreach (ProjectIconEntry cpie in _gridView.GetChildren())
        {
            if (cpie != pie)
                cpie.SelfModulate = new Color("00FFFFFF");
        }
    }

    private async void ExecuteEditorProject(ProjectFile pf)
    {
        GodotVersion gv = CentralStore.Instance.FindVersion(pf.GodotVersion);
        if (gv == null)
        {
            var ynd = AppDialogs.YesNoDialog;
            var res = await ynd.ShowDialog("Missing Godot Version",
                string.Format(
                    Tr("Godot Version was not found for project {0}, do you wish to use the default engine {1}?"),
                    pf.Name,
                    CentralStore.Instance.FindVersion(CentralStore.Settings.DefaultEngine).Tag
                    )
            );
            if (res)
            {
                pf.GodotVersion = CentralStore.Settings.DefaultEngine;
                gv = CentralStore.Instance.FindVersion(CentralStore.Settings.DefaultEngine);
                CentralStore.Instance.SaveDatabase();
                if (gv == null)
                {
                    var ld = AppDialogs.MessageDialog;
                    ld.ShowMessage("Failed to Launch Project", "Default Godot Version cannot be found, please select a default version of Godot to use.");
                }
            }
            else
            {
                var md = AppDialogs.MessageDialog;
                md.ShowMessage("Failed to Launch Project", "Godot Version for Project does not exist, cannot open project in Editor!");
                return;
            }
        }

        if (!SFile.Exists(gv.GetExecutablePath().GetOSDir()))
        {
            OS.Alert(string.Format(Tr("Executable path does not exist!  Please check the Versions folder at: {0} for {1}."), gv.Location, gv.Tag), "Execution Error");
            return;
        }

        if (!string.IsNullOrEmpty(gv.SharedSettings))
        {
            var ssgv = CentralStore.Instance.FindVersion(gv.SharedSettings);
            if (ssgv is null)
            {
                gv.SharedSettings = string.Empty;
                var md = AppDialogs.MessageDialog;
                md.ShowMessage("Shared Settings Invalid", "Instance of Shared Settings that was setup for this version of Godot, no longer exists, and has been removed.");
                CentralStore.Instance.SaveDatabase();
            }
            else
            {
                var fromPath = ssgv.Location.Join("editor_data");
                var toPath = gv.Location.Join("editor_data");
                var copies = new Array<string>()
                {
                    fromPath.Join("feature_profiles"),
                    fromPath.Join("script_templates"),
                    fromPath.Join("text_editor_themes"),
                    (gv.IsGodot4() && ssgv.IsGodot4()) ? fromPath.Join("editor_settings-4.tres") :
                        fromPath.Join("editor_settings-3.tres")
                };
                foreach (var path in copies)
                    CopyRecursive(path, toPath);
            }
        }

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = gv.GetExecutablePath().GetOSDir();
        psi.Arguments = $"--path \"{pf.Location.GetBaseDir()}\" -e";
        psi.WorkingDirectory = pf.Location.GetBaseDir().GetOSDir().NormalizePath();
        psi.UseShellExecute = !CentralStore.Settings.NoConsole;
        psi.CreateNoWindow = CentralStore.Settings.NoConsole;

        //Process proc = Process.Start(psi);
        Process proc = Process.Start(psi);
        if (CentralStore.Settings.CloseManagerOnEdit)
        {
            GetTree().Quit(0);
        }
    }

    void CopyRecursive(string fromPath, string toPath)
    {
        if (!Dir.Exists(toPath))
            Dir.CreateDirectory(toPath);

        var files = new Array<string>();
        if (Dir.Exists(fromPath))
            files = GodotInstaller.RecurseDirectory(fromPath);
        else
            files.Add(fromPath);

        foreach (var file in files)
        {
            var newFile = file.Replace(fromPath, toPath);
            if (newFile == toPath)
                newFile = toPath.Join(FPath.GetFileName(file));
            if (Dir.Exists(file) && !Dir.Exists(newFile))
                Dir.CreateDirectory(newFile);
            else if (SFile.Exists(file))
            {
                if (SFile.Exists(newFile))
                    SFile.Delete(newFile);
                SFile.Copy(file, newFile);
            }
        }
    }

    [SignalHandler("clicked", nameof(_actionButtons))]
    async void OnActionButtons_Clicked(int index)
    {
        switch (index)
        {
            case 0: // New Project File
                AppDialogs.CreateProject.ShowDialog();
                break;
            case 1: // Import Project File
                AppDialogs.ImportProject.ShowDialog();
                break;
            case 2: // Scan Project Folder
                ScanForProjects();
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
                AppDialogs.BrowseFolderDialog.CurrentFile = "";
                AppDialogs.BrowseFolderDialog.CurrentDir = CentralStore.Settings.ProjectPath;
                AppDialogs.BrowseFolderDialog.CurrentPath = CentralStore.Settings.ProjectPath;
                AppDialogs.BrowseFolderDialog.Connect("dir_selected", this, "OnSearchProjects_DirSelected", null, (int)ConnectFlags.Oneshot);
                AppDialogs.BrowseFolderDialog.Connect("popup_hide", this, "OnSearchProjects_PopupHide", null, (int)ConnectFlags.Oneshot);
                AppDialogs.BrowseFolderDialog.PopupCentered(new Vector2(510, 390));
                // TODO: Implement Browse Folder to match missing projects.
                break;
            case 7:
                var res = AppDialogs.YesNoDialog.ShowDialog(Tr("Remove Missing Projects..."),
                    Tr("Are you sure you want to remove any missing projects?"));
                await res;
                if (res.Result)
                    RemoveMissingProjects();
                break;
        }
    }

    private async void OnSearchProjects_DirSelected(string path)
    {
        _missingProjects.Clear();
        foreach (var project in CentralStore.Projects)
        {
            if (!ProjectFile.ProjectExists(project.Location))
                _missingProjects.Add(project);
        }

        if (_missingProjects.Count == 0) return;
        var foundMissing = 0;
        var foundProjects = new Array<ProjectFile>();
        path = path.NormalizePath();

        foreach (var project in _missingProjects)
        {
            var location = project.Location;
            var found = false;
            var parts = location.Split(System.IO.Path.DirectorySeparatorChar).Reverse().ToList();
            var pathPart = "";
            foreach (var part in parts)
            {
                pathPart = System.IO.Path.Combine(part, pathPart);
                var testPath = System.IO.Path.Combine(path, pathPart).NormalizePath();
                if (ProjectFile.ProjectExists(testPath))
                {
                    found = true;
                    location = System.IO.Path.Combine(path, pathPart).NormalizePath();
                    break;
                }
            }
            if (found)
            {
                project.Location = location.NormalizePath();
                foundProjects.Add(project);
                foundMissing++;
            }
        }

        foreach (var project in foundProjects)
        {
            _missingProjects.Remove(project);
            UpdatePIE(project);
            UpdatePLE(project);
        }

        if (foundMissing > 0)
        {
            CentralStore.Instance.SaveDatabase();
            AppDialogs.MessageDialog.ShowMessage("Found Updated Location", $"Found {foundMissing} projects in {path}.");
            PopulateListing();
        }
    }

    private async void OnSearchProjects_PopupHide()
    {
        if (AppDialogs.BrowseFolderDialog.IsConnected("dir_selected", this, "OnSearchProjects_DirSelected"))
            AppDialogs.BrowseFolderDialog.Disconnect("dir_selected", this, "OnSearchProjects_DirSelected");
    }

    private async Task RemoveProject(ProjectFile pf)
    {
        var task = AppDialogs.YesNoCancelDialog.ShowDialog(Tr("Remove Project"),
                string.Format(Tr("You are about to remove Project {0}.\nDo you wish to remove the files as well?"), pf.Name),
            Tr("Project and Files"), Tr("Just Project"));
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
                AppDialogs.MessageDialog.ShowMessage(Tr("Remove Project"),
                    Tr("Remove Project has been cancelled."));
                break;
        }
    }

    void RemoveFolders(string path)
    {
        Directory dir = new Directory();
        if (dir.Open(path) == Error.Ok)
        {
            dir.ListDirBegin(true, false);
            var filename = dir.GetNext();
            while (filename != "")
            {
                if (dir.CurrentIsDir())
                {
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
    void OnViewSelector_Clicked(int page)
    {
        for (int i = 0; i < _views.Count; i++)
        {
            if (i == page)
                _views[i].Show();
            else
                _views[i].Hide();
        }
        if (page == 2)
        {
            _actionButtons.SetVisible(3);
            _actionButtons.SetVisible(4);
        }
        else
        {
            _actionButtons.SetHidden(3);
            _actionButtons.SetHidden(4);
        }
        if (page == 0)
        {
            _projectSort.Visible = true;
        }
        else
        {
            _projectSort.Visible = false;
        }
        _currentView = (View)page;
        CentralStore.Settings.LastView = Views[page];
    }

    public System.Collections.ArrayList SortListing(bool @default = false)
    {
        IOrderedEnumerable<ProjectFile> fav;
        IOrderedEnumerable<ProjectFile> non_fav;

        // Default Behavior
        if (_projectName.Direction == HeaderButton.SortDirection.Indeterminate &&
             _godotVersion.Direction == HeaderButton.SortDirection.Indeterminate ||
             @default)
        {
            fav = CentralStore.Projects.Where(pf => pf.Favorite)
                        .OrderByDescending(pf => pf.LastAccessed);

            non_fav = CentralStore.Projects.Where(pf => !pf.Favorite)
                        .OrderByDescending(pf => pf.LastAccessed);
            // Sort by Project Name
        }
        else if (_projectName.Direction != HeaderButton.SortDirection.Indeterminate &&
                    _godotVersion.Direction == HeaderButton.SortDirection.Indeterminate)
        {
            if (_projectName.Direction == HeaderButton.SortDirection.Up)
            {
                fav = CentralStore.Projects.OrderByDescending(pf => pf.Name);
                non_fav = null;
            }
            else
            {
                fav = CentralStore.Projects.OrderBy(pf => pf.Name);
                non_fav = null;
            }
            // Sort by Godot Version
        }
        else
        {
            if (_godotVersion.Direction == HeaderButton.SortDirection.Up)
            {
                fav = CentralStore.Projects.OrderBy(pf => CentralStore.Instance.GetVersion(pf.GodotVersion).Tag)
                        .ThenBy(pf => !CentralStore.Instance.GetVersion(pf.GodotVersion).IsMono);
                non_fav = null;
            }
            else
            {
                fav = CentralStore.Projects.OrderByDescending(pf => CentralStore.Instance.GetVersion(pf.GodotVersion).Tag)
                        .ThenByDescending(pf => !CentralStore.Instance.GetVersion(pf.GodotVersion).IsMono);
                non_fav = null;
            }
        }

        if (non_fav == null)
            return new System.Collections.ArrayList() { fav };
        else
            return new System.Collections.ArrayList() { fav, non_fav };
    }
}
