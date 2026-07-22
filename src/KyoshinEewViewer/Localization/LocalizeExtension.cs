using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace KyoshinEewViewer.Localization;

public class LocalizeExtension(LocalizationKey key) : MarkupExtension
{
	public override object ProvideValue(IServiceProvider serviceProvider)
		=> new DynamicResourceExtension($"Localization.{key}").ProvideValue(serviceProvider);
}
