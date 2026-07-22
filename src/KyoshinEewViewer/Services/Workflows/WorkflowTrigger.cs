using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Series.Earthquake.Workflow;
using KyoshinEewViewer.Series.KyoshinMonitor.Workflow;
using KyoshinEewViewer.Series.Qzss.Workflow;
using KyoshinEewViewer.Series.Tsunami.Workflow;
using KyoshinEewViewer.Services.Workflows.BuiltinTriggers;
using ReactiveUI;
using System;
using System.Text.Json.Serialization;

namespace KyoshinEewViewer.Services.Workflows;

/// <summary>
/// トリガーの一覧表示に使用する情報。表示名は言語切り替えに追従する。
/// </summary>
public class WorkflowTriggerInfo : ReactiveObject
{
	private readonly LocalizationKey _displayNameKey;
	private LocalizationService? _localizationService;

	public WorkflowTriggerInfo(Type type, LocalizationKey displayNameKey, Func<WorkflowTrigger> create)
	{
		Type = type;
		_displayNameKey = displayNameKey;
		Create = create;
	}

	public Type Type { get; }
	public Func<WorkflowTrigger> Create { get; }

	public string DisplayName => _localizationService?.Get(_displayNameKey) ?? _displayNameKey.ToString();

	public void AttachLocalization(LocalizationService localizationService)
	{
		if (_localizationService == localizationService)
			return;
		if (_localizationService != null)
			_localizationService.LanguageChanged -= OnLanguageChanged;

		_localizationService = localizationService;
		_localizationService.LanguageChanged += OnLanguageChanged;
		OnLanguageChanged(this, EventArgs.Empty);
	}

	private void OnLanguageChanged(object? sender, EventArgs e)
		=> this.RaisePropertyChanged(nameof(DisplayName));
}

[JsonDerivedType(typeof(DummyTrigger), typeDiscriminator: "Dummy")]
[JsonDerivedType(typeof(AllTrigger), typeDiscriminator: "All")]
[JsonDerivedType(typeof(ShakeDetectTrigger), typeDiscriminator: "KyoshinShakeDetected")]
[JsonDerivedType(typeof(EewTrigger), typeDiscriminator: "Eew")]
[JsonDerivedType(typeof(ApplicationStartupTrigger), typeDiscriminator: "ApplicationStartup")]
[JsonDerivedType(typeof(UpdateAvailableTrigger), typeDiscriminator: "UpdateAvailable")]
[JsonDerivedType(typeof(EarthquakeInformationTrigger), typeDiscriminator: "EarthquakeInformation")]
[JsonDerivedType(typeof(TsunamiInformationTrigger), typeDiscriminator: "TsunamiInformation")]
[JsonDerivedType(typeof(QzssTrigger), typeDiscriminator: "Qzss")]
public abstract class WorkflowTrigger : ReactiveObject
{
	/// <summary>
	/// このトリガーが発火するイベントの型
	/// </summary>
	[JsonIgnore]
	public abstract Type EventType { get; }

	static WorkflowTrigger()
	{
		WorkflowService.RegisterTrigger<DummyTrigger>(LocalizationKey.WorkflowTriggerNameNone);
		WorkflowService.RegisterTrigger<AllTrigger>(LocalizationKey.WorkflowTriggerNameAll);
		WorkflowService.RegisterTrigger<ApplicationStartupTrigger>(LocalizationKey.WorkflowTriggerNameAppStartup);
		WorkflowService.RegisterTrigger<UpdateAvailableTrigger>(LocalizationKey.WorkflowTriggerNameUpdateAvailable);
		WorkflowService.RegisterTrigger<ShakeDetectTrigger>(LocalizationKey.WorkflowTriggerNameShakeDetect);
		WorkflowService.RegisterTrigger<EewTrigger>(LocalizationKey.WorkflowTriggerNameEew);
		WorkflowService.RegisterTrigger<EarthquakeInformationTrigger>(LocalizationKey.WorkflowTriggerNameEarthquakeInfo);
		WorkflowService.RegisterTrigger<TsunamiInformationTrigger>(LocalizationKey.WorkflowTriggerNameTsunamiInfo);
		WorkflowService.RegisterTrigger<QzssTrigger>(LocalizationKey.WorkflowTriggerNameQzss);
	}

	[JsonIgnore]
	public abstract Control DisplayControl { get; }
	public abstract bool CheckTrigger(WorkflowEvent content);

	public abstract WorkflowEvent CreateTestEvent();
}

public class DummyTrigger : WorkflowTrigger
{
	public override Type EventType => typeof(TestEvent);
	[JsonIgnore]
	public override Control DisplayControl => LocalizedControls.TextBlock(LocalizationKey.WorkflowTriggerNoneDescription);

	public override bool CheckTrigger(WorkflowEvent content)
		=> content is TestEvent;

	public override WorkflowEvent CreateTestEvent()
		=> new TestEvent();
}

public class TestEvent : WorkflowEvent
{
	public TestEvent(): base("Test", null)
	{
		IsTest = true;
	}
	public DateTime Time { get; } = DateTime.Now;
}
