using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Series;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;

namespace KyoshinEewViewer.ViewModels;

public class SettingPageViewModel : ReactiveObject, ISettingPage
{
	private readonly ISettingPage _page;
	private readonly LocalizationService _localizationService;

	public bool IsVisible => _page.IsVisible;
	public bool IsSelectable => _page is not BasicSettingPage;
	public string? Icon => _page.Icon;
	public string Title => _page.TitleKey is { } key ? _localizationService.Get(key) : _page.Title;
	public Control DisplayControl => _page.DisplayControl;
	public ISettingPage[] SubPages { get; }

	public SettingPageViewModel(ISettingPage page, LocalizationService localizationService)
	{
		_page = page;
		_localizationService = localizationService;
		SubPages = page.SubPages.Select(p => new SettingPageViewModel(p, localizationService)).Cast<ISettingPage>().ToArray();

		_localizationService.LanguageChanged += OnLanguageChanged;
		if (_page is INotifyPropertyChanged notifyPropertyChanged)
			notifyPropertyChanged.PropertyChanged += OnPagePropertyChanged;
	}

	private void OnLanguageChanged(object? sender, EventArgs e)
		=> this.RaisePropertyChanged(nameof(Title));

	private void OnPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(IsVisible))
			this.RaisePropertyChanged(nameof(IsVisible));
	}
}
