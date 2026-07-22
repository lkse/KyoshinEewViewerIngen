using Avalonia.Controls;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// コードビハインドでローカライズ済みコントロールを生成するためのヘルパー
/// </summary>
public static class LocalizedControls
{
	/// <summary>
	/// 指定したローカライズキーの文字列を表示し、言語切り替えに追従する <see cref="TextBlock"/> を生成する
	/// </summary>
	public static TextBlock TextBlock(LocalizationKey key)
	{
		var textBlock = new TextBlock();
		textBlock.Bind(Avalonia.Controls.TextBlock.TextProperty, textBlock.GetResourceObservable($"Localization.{key}"));
		return textBlock;
	}
}
