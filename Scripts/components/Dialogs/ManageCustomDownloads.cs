using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot.Sharp.Extras;

public class ManageCustomDownloads : ReferenceRect
{
    [Signal]
    public delegate void update_list();
    
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
        if (items.Length <= 0)
        {
            AppDialogs.MessageDialog.ShowMessage("Edit Custom Download",
                "You need to select a Download entry to edit it.");
            return;
        }
        var ced = CustomVersionList.GetItemMetadata(items[0]);
        _currentCed = ced as CustomEngineDownload;
        DisplayStruct();
    }

    [SignalHandler("pressed", nameof(RemoveCustomVersion))]
    async Task OnPressed_RemoveCustomVersion()
    {
        var items = CustomVersionList.GetSelectedItems();
        if (items.Length <= 0)
        {
            AppDialogs.MessageDialog.ShowMessage("Remove Custom Download",
                "You must select a Download entry to remove it.");
            return;
        }
        var delCed = CustomVersionList.GetItemMetadata(items[0]) as CustomEngineDownload;
        bool installed = false;
        GodotVersion installedGv = null;
        foreach (GodotVersion gv in CentralStore.Versions)
        {
            if (gv.CustomEngine == delCed)
            {
                installed = true;
                installedGv = gv;
                break;
            }
        }

        if (delCed == _currentCed)
            _currentCed = null;

        if (installed)
        {
            var res = await AppDialogs.YesNoDialog.ShowDialog(Tr("Remove Custom Engine Download"),
                Tr("There is a version of this engine installed, do you wish to uninstall it?"));
            if (res)
            {
                var installer = GodotInstaller.FromVersion(installedGv);
                installer.Uninstall();
                CentralStore.Versions.Remove(installedGv);
            }

            CentralStore.CustomEngines.Remove(delCed);
            CentralStore.Instance.SaveDatabase();
            UpdateList();
        }
        else
        {
            var res = await AppDialogs.YesNoDialog.ShowDialog(Tr("Remove Custom Engine Donwload"),
                string.Format(Tr("Are you sure you want to delete '{0}' from your list of downloads?"),delCed.Name));
            if (res)
            {
                CentralStore.CustomEngines.Remove(delCed);
                CentralStore.Instance.SaveDatabase();
                UpdateList();
            }
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

    private async void UpdateStruct()
    {
        if (_currentCed == null)
        {
            _currentCed = new CustomEngineDownload();
            CentralStore.CustomEngines.Add(_currentCed);
            int id = CentralStore.CustomEngines.Count;
            while (CentralStore.Instance.HasCustomEngineId(id))
                id++;

            _currentCed.Id = id;
        }

        if (_currentCed.Url != DownloadUrl.Text)
        {
            // Need to Update our download size.
            var client = new GDCSHTTPClient();
            var uri = new Uri(DownloadUrl.Text);
            var res = await client.StartClient(uri.Host, uri.Port, uri.Scheme == "https");
            if (res == HTTPClient.Status.Connected)
            {
                var headers = await client.HeadRequest(uri.PathAndQuery);
                if (headers.Headers.Contains("Transfer-Encoding"))
                    _currentCed.DownloadSize = 0;
                else
                {
                    int size = 0;
                    if (int.TryParse((string)headers.Headers["Content-Length"], out size))
                    {
                        _currentCed.DownloadSize = size;
                    }
                }
            }
            client.QueueFree();
        }

        _currentCed.Name = DownloadName.Text;
        _currentCed.Url = DownloadUrl.Text;
        _currentCed.Interval = IntervalValues[DownloadInterval.Selected];
        _currentCed.TagName = TagName.Text;
        bool found = false;
        for (int i = 0; i < CustomVersionList.GetItemCount(); i++)
        {
            if (CustomVersionList.GetItemMetadata(i) == _currentCed)
            {
                CustomVersionList.SetItemText(i, DownloadName.Text);
                found = true;
            }
        }
        CentralStore.Instance.SaveDatabase();

        if (!found)
            UpdateList();

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
        
        EmitSignal("update_list");
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
