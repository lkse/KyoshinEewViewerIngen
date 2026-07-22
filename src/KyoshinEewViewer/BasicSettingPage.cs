using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Series;
using ReactiveUI;

namespace KyoshinEewViewer;
public class BasicSettingPage<T>(string? icon, LocalizationKey titleKey, ISettingPage[] subPages) : ReactiveObject, ISettingPage where T : Control, new()
{
	private bool _isVisible = true;
	public bool IsVisible
	{
		get => _isVisible;
		set => this.RaiseAndSetIfChanged(ref _isVisible, value);
	}

	public string? Icon => icon;
	public string Title => titleKey.ToString();
	public LocalizationKey? TitleKey => titleKey;
	public Control DisplayControl => new T();

	public ISettingPage[] SubPages => subPages;
}

public class BasicSettingPage(string? icon, LocalizationKey titleKey, ISettingPage[] subPages) : ReactiveObject, ISettingPage
{
	private bool _isVisible = true;
	public bool IsVisible
	{
		get => _isVisible;
		set => this.RaiseAndSetIfChanged(ref _isVisible, value);
	}

	public string? Icon => icon;
	public string Title => titleKey.ToString();
	public LocalizationKey? TitleKey => titleKey;
	public Control DisplayControl => new Panel();

	public ISettingPage[] SubPages => subPages;
}
