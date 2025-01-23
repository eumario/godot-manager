using Godot;
using Godot.Sharp.Extras;

// namespace
namespace GodotManager.Library.Components.Dialogs;

[SceneNode("res://Library/Components/Dialogs/BusyDialog.tscn")]
public partial class BusyDialog : Window
{
	#region Signals
	#endregion
	
	#region Node Paths

	[NodePath] private Label _header;
	[NodePath] private Label _byline;
	#endregion
	
	#region Private Variables

	private string _headerText = "Fetching information from Github...";
	private string _bylineText = "Downloading xxx bytes...";
	#endregion
	
	#region Public Variables

	public string HeaderText
	{
		get => _headerText;
		set
		{
			_headerText = value;
			if (_header != null)
				_header.Text = value;
		}
	}

	public string BylineText
	{
		get => _bylineText;
		set
		{
			_bylineText = value;
			if (_byline != null)
				_byline.Text = _bylineText;
		}
	}
	#endregion

	#region Godot Overrides
	public override void _Ready()
	{
		this.OnReady();
		
		HeaderText = _headerText;
		BylineText = _bylineText;
	}
	#endregion
	
	#region Event Handlers
	#endregion
	
	#region Private Support Functions
	#endregion
	
	#region Public Support Functions

	public void UpdateHeader(string msg) => HeaderText = msg;
	public void UpdateByline(string msg) => BylineText = msg;

	#endregion
}
