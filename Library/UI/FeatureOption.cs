using Godot;
using System;
using GodotManager.Library.Util;

namespace GodotManager.Library.UI;

[SceneTree(root: "_")]
public partial class FeatureOption : PanelContainer
{
	[Notify] public partial string FeatureText { get; set; }
	[Notify] public partial decimal FeatureSize { get; set; }
	
	[OnInstantiate]
	public void Init(string feature, decimal size)
	{
		FeatureText = feature;
		FeatureSize = size;
	}
	
	public override partial void _Ready();

	[GodotOverride]
	public void OnReady()
	{
		Feature.Text = FeatureText;
		ArchiveSize.Text = FeatureSize == -1 ? "" : FeatureSize.FormatSize();

		FeatureTextChanged += () =>
		{
			if (Feature == null) return;
			Feature.Text = FeatureText;
		};
		FeatureSizeChanged += () =>
		{
			if (ArchiveSize == null) return;
			ArchiveSize.Text = FeatureSize == -1 ? "" : FeatureSize.FormatSize();
		};
	}
}
