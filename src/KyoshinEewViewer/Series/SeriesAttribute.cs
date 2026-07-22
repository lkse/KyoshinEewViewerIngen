using FluentAvalonia.UI.Controls;
using KyoshinEewViewer.Localization;
using ReactiveUI;
using System;

namespace KyoshinEewViewer.Series;

public class SeriesMeta(Type type, string key, LocalizationKey nameKey, FAIconSource icon, bool isDefaultEnabled, LocalizationKey? detailKey = null) : ReactiveObject
{
	private LocalizationService? _localizationService;
	public Type Type { get; } = type;

	/// <summary>
	/// 設定ファイルなどで使用するキー名
	/// </summary>
	public string Key { get; } = key;

	/// <summary>
	/// 表示名
	/// </summary>
	public string Name => GetLocalizedString(nameKey);

	/// <summary>
	/// アイコン
	/// </summary>
	public FAIconSource Icon { get; } = icon;

	/// <summary>
	/// デフォルトで有効な状態にするか
	/// </summary>
	public bool IsDefaultEnabled { get; } = isDefaultEnabled;

	/// <summary>
	/// 機能についての詳細
	/// </summary>
	public string Detail => detailKey is { } key ? GetLocalizedString(key) : "";

	public void AttachLocalization(LocalizationService localizationService)
	{
		if (_localizationService == localizationService)
			return;
		if (_localizationService != null)
			_localizationService.LanguageChanged -= OnLanguageChanged;

		_localizationService = localizationService;
		_localizationService.LanguageChanged += OnLanguageChanged;
		OnLanguageChanged(this, EventArgs.Empty);
	}

	private string GetLocalizedString(LocalizationKey key)
		=> _localizationService?.Get(key)
			?? throw new InvalidOperationException("SeriesMeta にローカライズサービスが設定されていません");

	private void OnLanguageChanged(object? sender, EventArgs e)
	{
		this.RaisePropertyChanged(nameof(Name));
		this.RaisePropertyChanged(nameof(Detail));
	}
}
