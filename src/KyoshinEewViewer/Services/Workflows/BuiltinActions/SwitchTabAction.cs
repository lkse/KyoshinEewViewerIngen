using Avalonia.Controls;
using KyoshinEewViewer.Events;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Series;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KyoshinEewViewer.Services.Workflows.BuiltinActions;

public class SwitchTabAction : WorkflowAction
{
	[JsonIgnore]
	public override Control DisplayControl => LocalizedControls.TextBlock(LocalizationKey.WorkflowActionSwitchTabDescription);

	public override Task ExecuteAsync(WorkflowEvent content)
	{
		if (content.EventedSeries is not SeriesBase series)
			return Task.CompletedTask;
		ActiveRequest.Send(series);
		return Task.CompletedTask;
	}
}
