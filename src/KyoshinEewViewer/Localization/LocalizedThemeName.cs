using KyoshinEewViewer.Core;
using Splat;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// <see cref="ThemeMeta"/> の表示名を言語設定に応じてローカライズする。
/// ThemeMeta は KyoshinEewViewer.Core にあり LocalizationService を参照できないため、
/// 表示側のこのヘルパーで解決する。
/// </summary>
public static class LocalizedThemeName
{
	public static string Get(ThemeMeta meta)
	{
		var loc = Locator.Current.GetService<LocalizationService>();
		return meta.Type switch
		{
			ThemeType.BuiltIn => loc?.Get(LocalizationKey.ThemeBuiltIn) ?? meta.DisplayName,
			ThemeType.ExternalFile => string.Format(loc?.Get(LocalizationKey.ThemeExternal) ?? "外部テーマ: {0}", meta.Identifier),
			ThemeType.Temporary => loc?.Get(LocalizationKey.ThemeEditEditingTheme) ?? meta.DisplayName,
			_ => meta.DisplayName,
		};
	}
}
