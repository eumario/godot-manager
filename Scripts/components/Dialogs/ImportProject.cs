using Godot;
using GodotSharpExtras;
using Godot.Collections;


public class ImportProject : ReferenceRect
{
    [Signal]
    public delegate void update_projects();

#region Node Paths
    [NodePath("PC/CC/P/VB/MCContent/VB/HBoxContainer/LocationValue")]
    LineEdit _locationValue = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/HBoxContainer/LocationBrowse")]
    Button _locationBrowse = null;

    [NodePath("PC/CC/P/VB/MCContent/VB/GodotVersions")]
    OptionButton _godotVersions = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/AddBtn")]
    Button _addBtn = null;

    [NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")]
    Button _cancelBtn = null;
#endregion

#region Private Variables
    GodotVersion gvDefault;
    int iDefault;
#endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		this.OnReady();

		UpdateGodotVersions();
		_locationValue.Text = "";

		_addBtn.Connect("pressed", this, "OnAddBtnPressed");
		_cancelBtn.Connect("pressed", this, "OnCancelBtnPressed");
		_locationBrowse.Connect("pressed", this, "OnLocationBrowsePressed");
	}

	private void UpdateGodotVersions()
	{
        _godotVersions.Clear();
		foreach (GodotVersion gdVers in CentralStore.Versions)
		{
			string gdTag = gdVers.GetDisplayName() + " (Default)";
			int id = CentralStore.Versions.IndexOf(gdVers);
			_godotVersions.AddItem(gdTag, id);
			_godotVersions.SetItemMetadata(id, gdVers.Id);
			if (CentralStore.Settings.DefaultEngine == gdVers.Id)
			{
				gvDefault = gdVers;
				iDefault = id;
			}
		}
	}

	public void ShowDialog() {
        UpdateGodotVersions();
        _locationValue.Text = "";
        _godotVersions.Selected = iDefault;
        Visible = true;
    }

    public void OnAddBtnPressed() {
        if (_locationValue.Text == "") {
            AppDialogs.MessageDialog.ShowMessage("No Project Selected", "You need to select a project before it can be added.");
            return;
        }
        if (_godotVersions.Selected == -1) {
            AppDialogs.MessageDialog.ShowMessage("No Godot Version Selected", "You need to select a Godot Version to use with this Project.");
            return;
        }
        int id = _godotVersions.GetItemId(_godotVersions.Selected);
        GodotVersion gdVers = CentralStore.Instance.FindVersion(_godotVersions.GetItemMetadata(id) as string);
        ProjectFile pf = ProjectFile.ReadFromFile(_locationValue.Text);
        if (gdVers != null)
            pf.GodotVersion = gdVers.Id;
        CentralStore.Projects.Add(pf);
        CentralStore.Instance.SaveDatabase();
        EmitSignal("update_projects");
        Visible = false;
    }

    public void OnCancelBtnPressed() {
        Visible = false;
    }

    public void OnLocationBrowsePressed() {
        AppDialogs.ImportFileDialog.CurrentFile = "";
        AppDialogs.ImportFileDialog.CurrentPath = "";
        AppDialogs.ImportFileDialog.PopupCentered(new Vector2(510, 390));
        AppDialogs.ImportFileDialog.Connect("file_selected", this, "OnFileSelected");
    }

    public void OnFileSelected(string file_path) {
        _locationValue.Text = file_path;
    }
}
