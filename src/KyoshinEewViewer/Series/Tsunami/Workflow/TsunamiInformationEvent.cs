using KyoshinEewViewer.Series.Tsunami.Models;
using KyoshinEewViewer.Services.Workflows;
using KyoshinEewViewer.Localization;

namespace KyoshinEewViewer.Series.Tsunami.Workflow;

public class TsunamiInformationEvent(TsunamiSeries? series) : WorkflowEvent("TsunamiInformation", series)
{
	[LocalizedDescription(LocalizationKey.WorkflowVarTsunamiBody)]
	public required TsunamiInfo? TsunamiInfo { get; init; }

	[LocalizedDescription(LocalizationKey.WorkflowVarTsunamiLevel)]
	public TsunamiLevel Level => TsunamiInfo?.Level ?? TsunamiLevel.None;

	[LocalizedDescription(LocalizationKey.WorkflowVarTsunamiPreviousLevel)]
	public required TsunamiLevel PreviousLevel { get; init; }
}
