using Avalonia.Controls;
using Avalonia.Interactivity;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Views.Components;
using Splat;

namespace KyoshinEewViewer.Services.Workflows.BuiltinActions;
public partial class LogOutputActionControl : UserControl
{
	public LogOutputActionControl()
	{
		InitializeComponent();
	}

	private async void EditTemplateButton_Click(object? sender, RoutedEventArgs e)
	{
		if (DataContext is not LogOutputAction action)
			return;

		var (success, templateText) = await TemplateEditorDialog.ShowAsync(
			Locator.Current.GetService<LocalizationService>()?.Get(LocalizationKey.WorkflowActionLogOutputEditorTitle) ?? "ログ出力テンプレート編集",
			action.TemplateText,
			action.FindEventType());

		if (success && templateText != null)
			action.TemplateText = templateText;
	}
}
