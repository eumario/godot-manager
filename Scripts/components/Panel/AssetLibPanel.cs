using Godot;
using Godot.Collections;
using GodotSharpExtras;

public class AssetLibPanel : Panel
{
#region Nodes Path
    [NodePath("VC/HC/SearchField")]
    LineEdit _searchField = null;

    [NodePath("VC/HC/Import")]
    Button _import = null;

    [NodePath("VC/HC2/SortBy")]
    OptionButton _sortBy = null;

    [NodePath("VC/HC2/Category")]
    OptionButton _category = null;

    [NodePath("VC/HC2/MirrorSite")]
    OptionButton _mirrorSite = null;

    [NodePath("VC/HC2/Support")]
    Button _support = null;

    [NodePath("VC/PaginatedListing")]
    PaginatedListing _paginatedListing = null;
#endregion

    public override void _Ready()
    {
        this.OnReady();
        GetParent<TabContainer>().Connect("tab_changed", this, "OnPageChanged");
    }

    private int downloadedBytes = 0;

    async void OnPageChanged(int page) {
        if (GetParent<TabContainer>().GetCurrentTabControl() == this) {
            AppDialogs.Instance.BusyDialog.UpdateHeader("Gathering information from GodotEngine Assetlib...");
            AppDialogs.Instance.BusyDialog.UpdateByline("Connecting...");
            AppDialogs.Instance.BusyDialog.ShowDialog();
            
            AssetLib.AssetLib.Instance.Connect("chunk_received", this, "OnChunkReceived");
            downloadedBytes = 0;
            var task = AssetLib.AssetLib.Instance.Configure();
            while (!task.IsCompleted) {
                await this.IdleFrame();
            }

            AssetLib.AssetLib.Instance.Disconnect("chunk_received", this, "OnChunkReceived");

            AppDialogs.Instance.BusyDialog.UpdateHeader("Processing Data from GodotEngine Assetlib...");
            AppDialogs.Instance.BusyDialog.UpdateByline("Processing...");

            _category.Clear();
            Dictionary<string, Array<Dictionary<string, string>>> categories = task.Result;

            foreach (Dictionary<string, string> category in categories["categories"]) {
                _category.AddItem(category["name"], category["id"].ToInt());
                _category.SetItemMetadata(category["id"].ToInt(), category["type"]);
            }
            
            AppDialogs.Instance.BusyDialog.Visible = false;
        }
    }
}
