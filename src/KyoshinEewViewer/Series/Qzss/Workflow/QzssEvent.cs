using KyoshinEewViewer.Series.Qzss.Models;
using KyoshinEewViewer.Services.Workflows;
using KyoshinEewViewer.Localization;

namespace KyoshinEewViewer.Series.Qzss.Workflow;

public class QzssEvent(QzssSeries? series, QzssEventType subType, string sentence, DisasterCrisisInformation information) : WorkflowEvent("Qzss", series)
{
	[LocalizedDescription(LocalizationKey.WorkflowVarQzssEventType)]
	public QzssEventType EventSubType { get; init; } = subType;

	[LocalizedDescription(LocalizationKey.WorkflowVarQzssRawSentence)]
	public string Sentence { get; init; } = sentence;

	[LocalizedDescription(LocalizationKey.WorkflowVarQzssParsedReport)]
	public DisasterCrisisInformation Information { get; init; } = information;
}

public enum QzssEventType
{
	NewSentenceReceived,

	ReportGroupCreated,
	ReportGroupUpdated,

	NankaiTroughReportCompleted,
}
