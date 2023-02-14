using Godot;
using Godot.Sharp.Extras;

// namespace

namespace GodotManager.Library.Components.Dialogs;

public partial class NewVersion : AcceptDialog
{
	#region Signals
	#endregion
	
	#region Quick Create
	public static NewVersion FromScene()
	{
		var scene = GD.Load<PackedScene>("res://Library/Components/Dialogs/NewVersion.tscn");
		return scene.Instantiate<NewVersion>();
	}
	#endregion
	
	#region Node Paths

	[NodePath] private Label _version;
	[NodePath] private Label _release;
	[NodePath] private Label _releasedBy;
	[NodePath] private CheckBox _downloadMono;
	#endregion
	
	#region Private Variables

	private string _versionString;
	private string _releaseString;
	private string _releasedByString;

	private string _versionFormat;
	private string _releaseFormat;
	private string _releasedByFormat;
	#endregion
	
	#region Public Variables

	public string Version
	{
		get => _versionString;
		set
		{
			_versionString = value;
			if (_version != null)
				_version.Text = string.Format(_versionFormat, value);
		}
	}

	public string Release
	{
		get => _releaseString;
		set
		{
			_releaseString = value;
			if (_release != null)
				_release.Text = string.Format(_releaseFormat, value);
		}
	}

	public string ReleasedBy
	{
		get => _releasedByString;
		set
		{
			_releasedByString = value;
			if (_releasedBy != null)
				_releasedBy.Text = string.Format(_releasedByFormat, value);
		}
	}

	public bool DownloadMono => _downloadMono.ButtonPressed;
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		_versionFormat = _version.Text;
		_releaseFormat = _release.Text;
		_releasedByFormat = _releasedBy.Text;

		Version = _versionString;
		Release = _releaseString;
		ReleasedBy = _releasedByString;
		
		// Rest of Initialization Functions
		AddCancelButton("Cancel");
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions
	#endregion
}