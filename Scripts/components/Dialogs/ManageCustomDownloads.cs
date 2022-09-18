using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot.Sharp.Extras;

public class ManageCustomDownloads : ReferenceRect
{
    #region NodePaths

    [NodePath] private TextureButton AddCustomVersion = null;
    [NodePath] private TextureButton EditCustomVersion = null;
    [NodePath] private TextureButton RemoveCustomVersion = null;

    [NodePath] private ItemList CustomVersionList = null;

    [NodePath] private LineEdit DownloadName = null;
    [NodePath] private LineEdit DownloadUrl = null;
    [NodePath] private OptionButton DownloadInterval = null;
    [NodePath] private LineEdit TagName = null;

    [NodePath] private Button SaveUpdates = null;
    
    [NodePath("PC/CC/P/VB/MCButtons/HB/OkBtn")] private Button OkButton = null;
    [NodePath("PC/CC/P/VB/MCButtons/HB/CancelBtn")] private Button CancelButton = null;
    #endregion
    
    #region Private Variables
    private CustomEngineDownload _currentCed = null;
    #endregion
    
    #region Static Readonly Varaibles

    private static readonly List<TimeSpan> IntervalValues = new List<TimeSpan>
    {
        TimeSpan.FromMinutes(60), TimeSpan.FromHours(24), TimeSpan.FromDays(7),
        TimeSpan.FromDays(14), TimeSpan.FromDays(30), TimeSpan.Zero
    };
    #endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
    }

    #region Signal Handlers
    [SignalHandler("pressed", nameof(AddCustomVersion))]
    void OnPressed_AddCustomVersion()
    {
        // int id = CentralStore.Categories.Count;
        // while (CentralStore.Instance.HasCategoryId(id)) {
        //     id++;
        // }
        // c.Id = id;
        if (_currentCed != null)
        {
            UpdateStruct();
            CentralStore.Instance.SaveDatabase();
        }

        _currentCed = new CustomEngineDownload();
        CentralStore.CustomEngines.Add(_currentCed);

        int id = CentralStore.CustomEngines.Count;
        while (CentralStore.Instance.HasCustomEngineId(id))
            id++;

        _currentCed.Id = id;
        CustomVersionList.AddItem("<no name>");
        CustomVersionList.SetItemMetadata(CustomVersionList.GetItemCount()-1, _currentCed);
        DisplayStruct();
    }

    [SignalHandler("pressed", nameof(EditCustomVersion))]
    void OnPressed_EditCustomVersion()
    {
        if (_currentCed != null)
        {
            UpdateStruct();
            CentralStore.Instance.SaveDatabase();
        }

        var items = CustomVersionList.GetSelectedItems();
        var ced = CustomVersionList.GetItemMetadata(items[0]);
        _currentCed = ced as CustomEngineDownload;
        DisplayStruct();
    }

    [SignalHandler("pressed", nameof(RemoveCustomVersion))]
    async Task OnPressed_RemoveCustomVersion()
    {
        var delCed = CustomVersionList.GetItemMetadata(CustomVersionList.GetSelectedItems()[0]) as CustomEngineDownload;
        //AppDialogs.MessageDialog.ShowMessage("Remove Custom Engine Donwload", $"Are you sure you want to delete '{delCed.Name}' from your list of downloads?");
        var res = await AppDialogs.YesNoDialog.ShowDialog("Remove Custom Engine Donwload",
            $"Are you sure you want to delete '{delCed.Name}' from your list of downloads?");
        if (res)
        {
            CentralStore.CustomEngines.Remove(delCed);
            CentralStore.Instance.SaveDatabase();
            UpdateList();
        }
    }

    [SignalHandler("item_selected", nameof(CustomVersionList))]
    void OnItemSelected_CustomVersionList(int index)
    {
        _currentCed = (CustomEngineDownload)CustomVersionList.GetItemMetadata(index);
        DisplayStruct();
    }

    [SignalHandler("pressed", nameof(SaveUpdates))]
    void OnPressed_SaveUpdates()
    {
        UpdateStruct();
        CentralStore.Instance.SaveDatabase();
    }

    [SignalHandler("pressed", nameof(OkButton))]
    void OnPressed_OkButton()
    {
        CentralStore.Instance.SaveDatabase();
        Visible = false;
    }

    [SignalHandler("pressed", nameof(CancelButton))]
    void OnPressed_CancelButton()
    {
        Visible = false;
    }
    #endregion
    
    #region Private Functions

    private void UpdateStruct()
    {
        _currentCed.Name = DownloadName.Text;
        _currentCed.Url = DownloadUrl.Text;
        _currentCed.Interval = IntervalValues[DownloadInterval.Selected];
        _currentCed.TagName = TagName.Text;
        for (int i = 0; i < CustomVersionList.GetItemCount(); i++)
        {
            if (CustomVersionList.GetItemMetadata(i) == _currentCed)
            {
                CustomVersionList.SetItemText(i, DownloadName.Text);
            }
        }

        ClearFields();
    }

    private void DisplayStruct()
    {
        DownloadName.Text = _currentCed.Name;
        DownloadUrl.Text = _currentCed.Url;
        DownloadInterval.Selected = IntervalValues.IndexOf(_currentCed.Interval);
        TagName.Text = _currentCed.TagName;
    }

    private void UpdateList()
    {
        CustomVersionList.Clear();

        foreach (CustomEngineDownload ced in CentralStore.CustomEngines)
        {
            CustomVersionList.AddItem(ced.Name == "" ? "<no name>" : ced.Name);
            CustomVersionList.SetItemMetadata(CustomVersionList.GetItemCount()-1, ced);
        }
    }

    private void ClearFields()
    {
        DownloadName.Text = "";
        DownloadUrl.Text = "";
        DownloadInterval.Selected = IntervalValues.IndexOf(TimeSpan.Zero);
        TagName.Text = "";
    }
    #endregion

    public void ShowDialog()
    {
        ClearFields();
        UpdateList();
        Visible = true;
    }
}
