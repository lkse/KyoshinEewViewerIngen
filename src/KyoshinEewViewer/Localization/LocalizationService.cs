using Avalonia;
using Avalonia.Controls;
using KyoshinEewViewer.Core.Models;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace KyoshinEewViewer.Localization;

public class LocalizationService : ReactiveObject
{
	private const string JapaneseLanguageCode = "ja";
	private const string EnglishLanguageCode = "en";

	public IReadOnlyList<LanguageOption> SupportedLanguages { get; } =
	[
		new(JapaneseLanguageCode, "日本語"),
		new(EnglishLanguageCode, "English"),
	];

	public event EventHandler? LanguageChanged;

	private readonly KyoshinEewViewerConfiguration _config;
	private Application? _application;
	private ResourceDictionary? _currentCatalog;
	private LanguageOption _selectedLanguage;

	public LanguageOption SelectedLanguage
	{
		get => _selectedLanguage;
		set
		{
			if (FindLanguage(value.Code) != value)
				throw new ArgumentException($"対応していない言語が指定されました: {value.Code}", nameof(value));
			if (_selectedLanguage == value)
				return;

			this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
			_config.Language = value.Code;
			ApplyLanguage();
		}
	}

	public LocalizationService(KyoshinEewViewerConfiguration config)
	{
		SplatRegistrations.RegisterLazySingleton<LocalizationService>();

		_config = config;
		_selectedLanguage = FindLanguage(config.Language);
	}

	public void Initialize(Application application)
	{
		_application = application;
		ApplyLanguage();
	}

	public string Get(LocalizationKey key)
	{
		if (_application?.Resources.TryGetResource($"Localization.{key}", null, out var value) == true && value is string text)
			return text;

		throw new InvalidOperationException($"ローカライズリソースが見つかりません: {key}");
	}

	private LanguageOption FindLanguage(string languageCode)
	{
		foreach (var language in SupportedLanguages)
			if (language.Code == languageCode)
				return language;

		return SupportedLanguages[0];
	}

	private void ApplyLanguage()
	{
		if (_application == null)
			return;

		var culture = SelectedLanguage.Code switch
		{
			JapaneseLanguageCode => new CultureInfo("ja-JP"),
			EnglishLanguageCode => new CultureInfo("en-US"),
			_ => throw new InvalidOperationException($"対応していない言語が指定されました: {SelectedLanguage.Code}"),
		};
		CultureInfo.CurrentCulture = culture;
		CultureInfo.CurrentUICulture = culture;
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		if (_currentCatalog != null)
			_application.Resources.MergedDictionaries.Remove(_currentCatalog);

		_currentCatalog = LoadCatalog(SelectedLanguage.Code);
		_application.Resources.MergedDictionaries.Add(_currentCatalog);
		LanguageChanged?.Invoke(this, EventArgs.Empty);
	}

	private static ResourceDictionary LoadCatalog(string languageCode)
	{
		var resourceName = $"KyoshinEewViewer.Localization.Localization.{languageCode}.json";
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
			?? throw new InvalidOperationException($"ローカライズカタログが見つかりません: {languageCode}");
		var values = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
			?? throw new InvalidDataException($"ローカライズカタログを読み込めません: {languageCode}");
		var catalog = new ResourceDictionary();

		foreach (var key in Enum.GetValues<LocalizationKey>())
		{
			if (!values.Remove(key.ToString(), out var value))
				throw new InvalidDataException($"ローカライズキーが見つかりません: {languageCode}/{key}");
			catalog.Add($"Localization.{key}", value);
		}
		if (values.Count != 0)
			throw new InvalidDataException($"未定義のローカライズキーがあります: {languageCode}/{string.Join(", ", values.Keys)}");

		return catalog;
	}
}
