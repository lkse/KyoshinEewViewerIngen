using KyoshinEewViewer.Series.KyoshinMonitor.Models;
using KyoshinEewViewer.Services.Workflows;
using KyoshinMonitorLib;
using System;
using KyoshinEewViewer.Localization;

namespace KyoshinEewViewer.Series.KyoshinMonitor.Workflow;

public class EewEvent(KyoshinMonitorSeries? series, EewEventType subType) : WorkflowEvent("Eew", series)
{
	[LocalizedDescription(LocalizationKey.WorkflowVarEewEventType)]
	public EewEventType EventSubType { get; init; } = subType;

	[LocalizedDescription(LocalizationKey.WorkflowVarEewOccurrenceTime)]
	public DateTime? OccurrenceAt { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewEventId)]
	public required string EewId { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewSource)]
	public required string EewSource { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewSerial)]
	public int SerialNo { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewIsDefinitelyCancelled)]
	public bool IsTrueCancelled { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewMaxIntensity)]
	public JmaIntensity Intensity { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewMaxIntensityLong)]
	public string IntensityLongName => Intensity.ToLongString();

	[LocalizedDescription(LocalizationKey.WorkflowVarEewIsIntensityOver)]
	public bool IsIntensityOver { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewEpicenterName)]
	public string? EpicenterPlaceName { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewEpicenterLocation)]
	public Location? EpicenterLocation { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewMagnitude)]
	public float? Magnitude { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewDepth)]
	public int? Depth { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewIsAssumedHypocenter)]
	public bool? IsTemporaryEpicenter { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewIsWarning)]
	public bool IsWarning { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewWarningAreaCodes)]
	public int[]? WarningAreaCodes { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewWarningAreaNames)]
	public string[]? WarningAreaNames { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewIsFinal)]
	public bool IsFinal { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewIsCancelled)]
	public bool IsCancelled { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEewIsReplay)]
	public bool IsReplay { get; init; }

	public static EewEvent FromEewModel(KyoshinMonitorSeries series, EewEventType type, Eew eew, bool isReplay)
		=> new(series, type)
		{
			OccurrenceAt = eew.Hypocenter?.OccurrenceTime,
			EewId = eew.Id,
			EewSource = eew.DisplaySource,
			SerialNo = eew.SerialNo,
			IsTrueCancelled = eew.IsTrueCancelled,
			Intensity = eew.MaxIntensity,
			IsIntensityOver = eew.IsIntensityOver,
			EpicenterPlaceName = eew.Hypocenter?.Place,
			EpicenterLocation = eew.Hypocenter?.Location,
			Magnitude = eew.Hypocenter?.Magnitude,
			Depth = eew.Hypocenter?.Depth,
			IsTemporaryEpicenter = eew.Hypocenter?.IsTemporary,
			IsWarning = eew.IsWarning,
			WarningAreaCodes = eew.WarningAreas?.Codes,
			WarningAreaNames = eew.WarningAreas?.Names,
			IsFinal = eew.IsFinal,
			IsCancelled = eew.IsCancelled,
			IsReplay = isReplay,
		};
}

public enum EewEventType
{
	New,
	UpdateNewSerial,
	UpdateWithMoreAccurate,
	Final,
	Cancel,
	NewWarning,
	UpdateWarning,
	CancelWarning,
	WarningLevelReached,
	IncreaseMaxIntensity,
	DecreaseMaxIntensity,
}
