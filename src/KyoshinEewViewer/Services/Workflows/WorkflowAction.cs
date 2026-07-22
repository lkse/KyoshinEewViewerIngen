using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Services.Workflows.BuiltinActions;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KyoshinEewViewer.Services.Workflows;

/// <summary>
/// アクションの一覧表示に使用する情報。表示名は言語切り替えに追従する。
/// </summary>
public class WorkflowActionInfo : ReactiveObject
{
	private readonly LocalizationKey _displayNameKey;
	private LocalizationService? _localizationService;

	public WorkflowActionInfo(Type type, LocalizationKey displayNameKey, Func<WorkflowAction> create)
	{
		Type = type;
		_displayNameKey = displayNameKey;
		Create = create;
	}

	public Type Type { get; }
	public Func<WorkflowAction> Create { get; }

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

[JsonDerivedType(typeof(DummyAction), typeDiscriminator: "Dummy")]
[JsonDerivedType(typeof(MultipleAction), typeDiscriminator: "Multiple")]
[JsonDerivedType(typeof(SendNotificationAction), typeDiscriminator: "SendNotification")]
[JsonDerivedType(typeof(PlaySoundAction), typeDiscriminator: "PlaySound")]
[JsonDerivedType(typeof(WindowActivateAction), typeDiscriminator: "WindowActivate")]
[JsonDerivedType(typeof(WaitAction), typeDiscriminator: "Wait")]
[JsonDerivedType(typeof(LogOutputAction), typeDiscriminator: "LogOutput")]
[JsonDerivedType(typeof(WebhookAction), typeDiscriminator: "Webhook")]
[JsonDerivedType(typeof(ExecuteFileAction), typeDiscriminator: "ExecuteFile")]
[JsonDerivedType(typeof(VoicevoxSpeechAction), typeDiscriminator: "VoicevoxSpeech")]
[JsonDerivedType(typeof(SwitchTabAction), typeDiscriminator: "SwitchTab")]
public abstract class WorkflowAction : ReactiveObject
{
	static WorkflowAction()
	{
		WorkflowService.RegisterAction<DummyAction>(LocalizationKey.WorkflowActionNameNone);
		WorkflowService.RegisterAction<MultipleAction>(LocalizationKey.WorkflowActionNameMultiple);
		WorkflowService.RegisterAction<SendNotificationAction>(LocalizationKey.WorkflowActionNameSendNotification);
		WorkflowService.RegisterAction<PlaySoundAction>(LocalizationKey.WorkflowActionNamePlaySound);
		WorkflowService.RegisterAction<VoicevoxSpeechAction>(LocalizationKey.WorkflowActionNameVoicevox);
		WorkflowService.RegisterAction<WindowActivateAction>(LocalizationKey.WorkflowActionNameWindowActivate);
		WorkflowService.RegisterAction<SwitchTabAction>(LocalizationKey.WorkflowActionNameSwitchTab);
		WorkflowService.RegisterAction<WaitAction>(LocalizationKey.WorkflowActionNameWait);
		WorkflowService.RegisterAction<LogOutputAction>(LocalizationKey.WorkflowActionNameLogOutput);
		WorkflowService.RegisterAction<WebhookAction>(LocalizationKey.WorkflowActionNameWebhook);
		WorkflowService.RegisterAction<ExecuteFileAction>(LocalizationKey.WorkflowActionNameExecuteFile);
	}

	[JsonIgnore]
	public abstract Control DisplayControl { get; }

	public virtual Task PrepareAsync(WorkflowEvent content) => Task.CompletedTask;
	public abstract Task ExecuteAsync(WorkflowEvent content);

	/// <summary>
	/// このアクションが所属するワークフローのトリガーからイベント型を取得する
	/// </summary>
	public Type? FindEventType()
	{
		var workflowService = Locator.Current.GetService<WorkflowService>();
		if (workflowService == null)
			return null;
		var workflow = workflowService.Workflows.Concat(workflowService.SystemWorkflows)
			.FirstOrDefault(w => w.Actions == this || w.Actions.ChildActions.Any(c => c.Action == this));
		return workflow?.Trigger?.EventType;
	}
}

public class DummyAction : WorkflowAction
{
	[JsonIgnore]
	public override Control DisplayControl => LocalizedControls.TextBlock(LocalizationKey.WorkflowActionNoneDescription);
	public override Task ExecuteAsync(WorkflowEvent content)
		=> Task.CompletedTask;
}
