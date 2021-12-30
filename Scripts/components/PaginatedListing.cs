using Godot;
using GodotSharpExtras;
using Godot.Collections;
using System.Threading.Tasks;

public class PaginatedListing : ScrollContainer
{
#region Signals
    [Signal]
    public delegate void page_changed(int page);
#endregion

#region Node Paths
    [NodePath("VBoxContainer/TopPageCount")]
    PaginationNav _topPageCount = null;

    [NodePath("VBoxContainer/Listing")]
    GridContainer _listing = null;

    [NodePath("VBoxContainer/BottomPageCount")]
    PaginationNav _bottomPageCount = null;
#endregion

#region Templates
    PackedScene tAssetLibEntry = GD.Load<PackedScene>("res://components/AssetLibEntry.tscn");
#endregion

#region Private Variables
    AssetLib.QueryResult alqrLastResult = null;
    DownloadQueue dlq = null;
#endregion

    public override void _Ready()
    {
        this.OnReady();
        _topPageCount.Connect("page_changed", this, "OnPageChanged");
        _bottomPageCount.Connect("page_changed", this, "OnPageChanged");
        dlq = new DownloadQueue();
        dlq.Name = "DownloadQueue";
        dlq.Connect("download_completed", this, "OnImageDownloaded");
        AddChild(dlq);
    }

    public void UpdateResults(AssetLib.QueryResult result) {
        foreach(AssetLibEntry ale in _listing.GetChildren()) {
            ale.QueueFree();
        }
        if (alqrLastResult != null) {
            if (alqrLastResult.Pages != result.Pages) {
                _topPageCount.UpdateConfig(result.Pages);
                _bottomPageCount.UpdateConfig(result.Pages);
            }
        } else {
            _topPageCount.UpdateConfig(result.Pages);
            _bottomPageCount.UpdateConfig(result.Pages);
        }
        alqrLastResult = result;
        _topPageCount.SetPage(result.Page);
        _bottomPageCount.SetPage(result.Page);
        foreach(AssetLib.AssetResult asset in result.Result) {
            AssetLibEntry ale = tAssetLibEntry.Instance<AssetLibEntry>();
            ale.Title = asset.Title;
            ale.Category = asset.Category;
            ale.Author = asset.Author;
            ale.License = asset.Cost;
            ale.AssetId = asset.AssetId;
            _listing.AddChild(ale);
            System.Uri uri = new System.Uri(asset.IconUrl);
            string iconPath = $"user://cache/images/{asset.AssetId}{uri.AbsolutePath.GetExtension()}";
            ale.SetMeta("iconPath", iconPath);
            if (!System.IO.File.Exists(iconPath.GetOSDir().NormalizePath())) {
                // Implement Image Downloader through Download Queue
                ImageDownloader dld = new ImageDownloader(asset.IconUrl, iconPath);
                dlq.Push(dld);
                ale.SetMeta("dld", dld);
            } else {
                Texture icon = Util.LoadImage(iconPath);
                if (icon == null)
                    ale.Icon = Util.LoadImage("res://Assets/Icons/missing_icon.svg");
                else
                    ale.Icon = icon;
            }
        }
        dlq.StartDownload();
    }

    public void OnImageDownloaded(ImageDownloader dld) {
        foreach(AssetLibEntry ale in _listing.GetChildren()) {
            if (ale.HasMeta("dld")) {
                if ((ale.GetMeta("dld") as ImageDownloader) == dld) {
                    ale.RemoveMeta("dld");
                    string iconPath = ale.GetMeta("iconPath") as string;
                    if (System.IO.File.Exists(iconPath.GetOSDir().NormalizePath())) {
                        Texture icon = Util.LoadImage(iconPath);
                        if (icon == null)
                            icon = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
                        ale.Icon = icon;
                    } else {
                        ale.Icon = GD.Load<Texture>("res://Assets/Icons/missing_icon.svg");
                    }
                    return;
                }
            }
        }
    }

    public void OnPageChanged(int page) {
        if (alqrLastResult != null && page != alqrLastResult.Page) {
            EmitSignal("page_changed", page);
        }
    }
}
