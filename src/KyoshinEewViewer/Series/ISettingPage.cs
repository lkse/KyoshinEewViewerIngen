using Avalonia.Controls;
using KyoshinEewViewer.Localization;

namespace KyoshinEewViewer.Series;

public interface ISettingPage
{
	public bool IsVisible { get; }
	public string? Icon { get; }
	public string Title { get; }
	public LocalizationKey? TitleKey => null;
	public Control DisplayControl { get; }

	public ISettingPage[] SubPages { get; }
}
