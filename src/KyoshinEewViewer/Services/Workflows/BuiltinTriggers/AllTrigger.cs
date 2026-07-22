using System;
using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using System.Text.Json.Serialization;

namespace KyoshinEewViewer.Services.Workflows.BuiltinTriggers;

public class AllTrigger : WorkflowTrigger
{
	public override Type EventType => typeof(WorkflowEvent);
	[JsonIgnore]
	public override Control DisplayControl => LocalizedControls.TextBlock(LocalizationKey.WorkflowTriggerAllDescription);

	public override bool CheckTrigger(WorkflowEvent content) => true;
	public override WorkflowEvent CreateTestEvent() => new TestEvent();
}
