using Godot;
using System;
using GodotManager.Library.Util;

namespace GodotManager.Library.UI;

[SceneTree(root: "Nodes")]
public partial class FeatureOption : PanelContainer
{
	[Notify] public partial string FeatureText { get; set; }
	[Notify] public partial decimal FeatureSize { get; set; }
	[Notify] public partial string HintTooltip { get; set; }
	
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
		TooltipText = HintTooltip;
		Feature.TooltipText = HintTooltip;
		ArchiveSize.TooltipText = HintTooltip;
		Install.TooltipText = HintTooltip;

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
		HintTooltipChanged += () =>
		{
			if (FeatureText == null || ArchiveSize == null || Install == null)
				return;
			TooltipText = HintTooltip;
			Feature.TooltipText = HintTooltip;
			ArchiveSize.TooltipText = HintTooltip;
			Install.TooltipText = HintTooltip;
		};
	}
}
