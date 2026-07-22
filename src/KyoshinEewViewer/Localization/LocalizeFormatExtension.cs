using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// ローカライズされた書式文字列に値を埋め込むマークアップ拡張
/// </summary>
public class LocalizeFormatExtension(LocalizationKey key) : MarkupExtension
{
	/// <summary>
	/// 書式の {0} に埋め込む値
	/// </summary>
	public BindingBase? Value { get; set; }

	/// <summary>
	/// 書式の {1} に埋め込む値
	/// </summary>
	public BindingBase? Value2 { get; set; }

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		var binding = new MultiBinding { Converter = FormatConverter.Default };
		binding.Bindings.Add(new DynamicResourceExtension($"Localization.{key}"));
		if (Value != null)
			binding.Bindings.Add(Value);
		if (Value2 != null)
			binding.Bindings.Add(Value2);
		return binding;
	}

	private sealed class FormatConverter : IMultiValueConverter
	{
		public static FormatConverter Default { get; } = new();

		public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
			=> values[0] is string format ? string.Format(culture, format, [.. values.Skip(1)]) : null;
	}
}
