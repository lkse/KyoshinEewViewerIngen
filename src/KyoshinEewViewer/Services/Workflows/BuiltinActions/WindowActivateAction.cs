using Avalonia.Controls;
using KyoshinEewViewer.Core.Models.Events;
using KyoshinEewViewer.Localization;
using ReactiveUI;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KyoshinEewViewer.Services.Workflows.BuiltinActions;

public class WindowActivateAction : WorkflowAction
{
	[JsonIgnore]
	public override Control DisplayControl => LocalizedControls.TextBlock(LocalizationKey.WorkflowActionWindowActivateDescription);

	public override Task ExecuteAsync(WorkflowEvent content)
	{
		MessageBus.Current.SendMessage(new ShowMainWindowRequested());
		return Task.CompletedTask;
	}
}
