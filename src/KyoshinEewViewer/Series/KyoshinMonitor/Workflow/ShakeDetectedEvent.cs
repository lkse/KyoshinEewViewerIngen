using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Series.KyoshinMonitor.Models;
using KyoshinEewViewer.Services.Workflows;
using System;
using System.Linq;
using KyoshinEewViewer.Localization;

namespace KyoshinEewViewer.Series.KyoshinMonitor.Workflow;

public class ShakeDetectedEvent(
	KyoshinMonitorSeries? series,
	DateTime time,
	KyoshinEvent evt,
	bool isReplay,
	bool isRegionExpanded,
	bool isSubRegionExpanded,
	ShakeDetectedRegion[] regionDetails
) : WorkflowEvent("KyoshinShakeDetected", series)
{
	[LocalizedDescription(LocalizationKey.WorkflowVarShakeDetectedTime)]
	public DateTime EventedAt { get; } = time;

	[LocalizedDescription(LocalizationKey.WorkflowVarShakeFirstDetectedTime)]
	public DateTime FirstEventedAt { get; } = evt.CreatedAt;

	[LocalizedDescription(LocalizationKey.WorkflowVarShakeLevel)]
	public KyoshinEventLevel Level { get; } = evt.Level;

	[LocalizedDescription(LocalizationKey.WorkflowVarShakeEventId)]
	public Guid KyoshinEventId { get; } = evt.Id;

	[LocalizedDescription(LocalizationKey.WorkflowVarShakeRegionNames)]
	public string[] Regions { get; } = evt.Points.Select(p => p.Region).Distinct().ToArray();

	[LocalizedDescription(LocalizationKey.WorkflowVarShakePeakRegions)]
	public ShakeDetectedRegion[] RegionDetails { get; } = regionDetails;

	[LocalizedDescription(LocalizationKey.WorkflowVarShakeIsReplay)]
	public bool IsReplay { get; } = isReplay;

	[LocalizedDescription(LocalizationKey.WorkflowVarShakeIsRegionExpanded)]
	public bool IsRegionExpanded { get; } = isRegionExpanded;

	[LocalizedDescription(LocalizationKey.WorkflowVarShakeIsSubRegionExpanded)]
	public bool IsSubRegionExpanded { get; } = isSubRegionExpanded;
}
