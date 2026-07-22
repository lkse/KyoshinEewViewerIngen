using Avalonia.Controls;
using Avalonia.Interactivity;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Views.Components;
using Splat;

namespace KyoshinEewViewer.Services.Workflows.BuiltinActions;
public partial class VoicevoxSpeechActionControl : UserControl
{
	public VoicevoxSpeechActionControl()
	{
		InitializeComponent();
	}

	private async void EditTemplateButton_Click(object? sender, RoutedEventArgs e)
	{
		if (DataContext is not VoicevoxSpeechAction action)
			return;

		var (success, templateText) = await TemplateEditorDialog.ShowAsync(
			Locator.Current.GetService<LocalizationService>()?.Get(LocalizationKey.WorkflowActionVoicevoxEditorTitle) ?? "読み上げ内容テンプレート編集",
			action.TemplateText,
			action.FindEventType());

		if (success && templateText != null)
			action.TemplateText = templateText;
	}
}
