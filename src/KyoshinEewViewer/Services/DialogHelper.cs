using FluentAvalonia.UI.Controls;
using KyoshinEewViewer.Localization;
using Splat;
using System.Threading.Tasks;

namespace KyoshinEewViewer.Services;

public static class DialogHelper
{
	/// <summary>
	/// 確認ダイアログを表示します
	/// </summary>
	/// <param name="title">ダイアログのタイトル</param>
	/// <param name="message">確認メッセージ</param>
	/// <returns>「はい」が選択された場合true、それ以外はfalse</returns>
	public static async Task<bool> ShowSettingWindowConfirmationDialogAsync(string title, string message)
	{
		var windowService = Locator.Current.GetService<ISubWindowsService>();
		if (windowService == null)
			return false;

		var localizationService = Locator.Current.GetService<LocalizationService>();
		var dialog = new FAContentDialog()
		{
			Title = title,
			Content = message,
			PrimaryButtonText = localizationService?.Get(LocalizationKey.CommonYes) ?? "はい",
			SecondaryButtonText = localizationService?.Get(LocalizationKey.CommonNo) ?? "いいえ",
			DefaultButton = FAContentDialogButton.Secondary
		};

		var result = await dialog.ShowAsync(windowService.SettingWindow);
		return result == FAContentDialogResult.Primary;
	}
}