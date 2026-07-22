using KyoshinEewViewer.Core;
using KyoshinEewViewer.Services.Workflows;
using KyoshinMonitorLib;
using System;
using KyoshinEewViewer.Localization;

namespace KyoshinEewViewer.Series.Earthquake.Workflow;

public class EarthquakeInformationEvent(EarthquakeSeries? series) : WorkflowEvent("EarthquakeInformation", series)
{
	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoUpdatedTime)]
	public DateTime UpdatedAt { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoTypeName)]
	public required string LatestInformationName { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoEventId)]
	public required string EarthquakeId { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoIsTraining)]
	public bool IsTrainingOrTest { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoIsVolcano)]
	public bool IsVolcano { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoVolcanoName)]
	public string? VolcanoName { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoOccurrenceTime)]
	public DateTime? DetectedAt { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoMaxIntensity)]
	public JmaIntensity MaxIntensity { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoMaxIntensityLong)]
	public string MaxIntensityLongName => MaxIntensity.ToLongString();

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoPreviousMaxIntensity)]
	public JmaIntensity? PreviousMaxIntensity { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoMaxLpgm)]
	public LpgmIntensity? MaxLpgmIntensity { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoIsCancelled)]
	public bool IsCancelled { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoIsHypocenterOnly)]
	public bool IsHypocenterOnly { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoIsDetailedIntensity)]
	public bool IsDetailIntensityApplied { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoHypocenter)]
	public EarthquakeInformationEventHypocenter? Hypocenter { get; init; }

	// TODO: 実装したいがけっこう構造弄らないといけないかも
	// public List<ObservationIntensityGroup> Intensities { get; init; } = [];

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoComment)]
	public string? Comment { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarEqInfoFreeComment)]
	public string? FreeFormComment { get; init; }
}

public record EarthquakeInformationEventHypocenter(
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoOccurrenceAt)] DateTime OccurrenceAt,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoPlaceName)] string? PlaceName,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoLocation)] Location? Location,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoMagnitude)] float Magnitude,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoMagnitudeAltText)] string? MagnitudeAlternativeText,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoDepth)] int Depth,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoIsNoDepthData)] bool IsNoDepthData,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoIsVeryShallow)] bool IsVeryShallow,
	[property: LocalizedDescription(LocalizationKey.WorkflowVarHypoIsForeign)] bool IsForeign
);

