using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reflection;
using System.Text.Json.Serialization;
using KyoshinEewViewer.Localization;

namespace KyoshinEewViewer.Services.Workflows.BuiltinTriggers;

public class UpdateAvailableTrigger : WorkflowTrigger
{
	public override Type EventType => typeof(UpdateAvailableEvent);
	[JsonIgnore]
	public override Control DisplayControl => new UpdateAvailableTriggerControl { DataContext = this };

	private bool _isContinuous = false;
	public bool IsContinuous
	{
		get => _isContinuous;
		set => this.RaiseAndSetIfChanged(ref _isContinuous, value);
	}
	
	public override bool CheckTrigger(WorkflowEvent content)
		=> content is UpdateAvailableEvent updateAvailableEvent && updateAvailableEvent.IsContinuous == IsContinuous;

	public override WorkflowEvent CreateTestEvent()
	{
		var random = new Random();
		return new UpdateAvailableEvent(IsContinuous && random.Next(0, 2) == 0, Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "0.0.0");
	}
}

public class UpdateAvailableEvent(bool isContinuous, string latestVersion) : WorkflowEvent("UpdateAvailable", null)
{
	[LocalizedDescription(LocalizationKey.WorkflowVarUpdateIsContinued)]
	public bool IsContinuous { get; } = isContinuous;

	[LocalizedDescription(LocalizationKey.WorkflowVarUpdateLatestVersion)]
	public string LatestVersion { get; } = latestVersion;
}
