using Avalonia.Data.Converters;
using KyoshinEewViewer.Core;
using KyoshinEewViewer.Localization;
using System;
using System.Globalization;

namespace KyoshinEewViewer.Converters;

/// <summary>
/// <see cref="ThemeMeta"/> をローカライズした表示名に変換する。
/// </summary>
public class ThemeMetaDisplayNameConverter : IValueConverter
{
	public static readonly ThemeMetaDisplayNameConverter Default = new();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is ThemeMeta meta ? LocalizedThemeName.Get(meta) : value?.ToString();

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}
