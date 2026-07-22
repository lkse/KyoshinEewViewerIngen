using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using KyoshinEewViewer.Core;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Localization;
using Splat;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using Path = System.IO.Path;

namespace KyoshinEewViewer.Views;

public partial class IntensityThemeEditWindow : Window
{
	private IDisposable? _themeSubscription;

	private static LocalizationService? Loc => Locator.Current.GetService<LocalizationService>();

	public IntensityThemeEditWindow()
	{
		InitializeComponent();

		rollbackButton.Click += async (_, _) =>
		{
			if (IntensityTheme == null)
				return;
			var result = await new FAContentDialog
			{
				Title = Loc?.Get(LocalizationKey.ThemeRestoreTitle) ?? "復元する",
				Content = IntensityTheme.Meta.Type == ThemeType.ExternalFile
					? Loc?.Get(LocalizationKey.ThemeReloadFromFile) ?? "ファイルからテーマを読み込み直しますか？"
					: Loc?.Get(LocalizationKey.ThemeDiscardEditingConfirm) ?? "編集中のテーマの変更を破棄しますか？",
				PrimaryButtonText = Loc?.Get(LocalizationKey.CommonYes) ?? "はい",
				SecondaryButtonText = Loc?.Get(LocalizationKey.CommonNo) ?? "いいえ",
			}.ShowAsync(this);

			if (result == FAContentDialogResult.Primary)
			{
				AssignTheme(IntensityTheme);
				IsSaved = true;
			}
		};

		saveButton.Click += async (_, _) =>
		{
			if (IntensityTheme == null || DataContext is not IntensityTheme theme || KyoshinEewViewerApp.Selector?.IntensityThemes is not { } intensityThemes)
				return;

			if (IntensityTheme.Meta.Type == ThemeType.BuiltIn)
			{
				var result = await new FAContentDialog
				{
					Title = Loc?.Get(LocalizationKey.ThemeSaveBuiltInTitle) ?? "組み込みテーマの保存",
					Content = string.Format(
						Loc?.Get(LocalizationKey.ThemeSaveBuiltInContent) ?? "組み込みテーマは変更できないため、外部テーマとして保存します。\n{0}.json として保存します。ファイル名に使用できない文字が含まれていないか確認してください。",
						theme.Name),
					PrimaryButtonText = Loc?.Get(LocalizationKey.CommonYes) ?? "はい",
					SecondaryButtonText = Loc?.Get(LocalizationKey.CommonNo) ?? "いいえ",
				}.ShowAsync(this);

				if (result != FAContentDialogResult.Primary)
					return;

				try
				{
					var intensityThemesDir = Path.Combine(PlatformDirectories.ApplicationData, "IntensityThemes");
					PlatformDirectories.EnsureDirectoryExists(intensityThemesDir);
					var path = Path.Combine(intensityThemesDir, $"{theme.Name}.json");
					File.WriteAllText(path, JsonSerializer.Serialize(theme));
					IntensityTheme = new ThemeSelector.IntensityTheme(new(ThemeType.ExternalFile, Path.GetFileName(path)), theme, theme.CreateResourceDictionary());
					intensityThemes.Add(IntensityTheme);
				}
				catch (Exception ex)
				{
					await new FAContentDialog
					{
						Title = Loc?.Get(LocalizationKey.ThemeSaveFailedTitle) ?? "保存に失敗",
						Content = string.Format(Loc?.Get(LocalizationKey.ThemeSaveFailedContent) ?? "テーマの保存に失敗しました: {0}", ex.Message),
						PrimaryButtonText = "OK",
					}.ShowAsync(this);
				}
				return;
			}

			if (IntensityTheme.Meta.Type == ThemeType.ExternalFile)
			{
				var result = await new FAContentDialog
				{
					Title = Loc?.Get(LocalizationKey.ThemeSaveExternalTitle) ?? "外部テーマの保存",
					Content = string.Format(Loc?.Get(LocalizationKey.ThemeSaveExternalContent) ?? "{0} にテーマを上書き保存しますか？", IntensityTheme.Meta.Identifier),
					PrimaryButtonText = Loc?.Get(LocalizationKey.CommonYes) ?? "はい",
					SecondaryButtonText = Loc?.Get(LocalizationKey.CommonNo) ?? "いいえ",
				}.ShowAsync(this);
				if (result != FAContentDialogResult.Primary)
					return;
				try
				{
					var intensityThemesDir = Path.Combine(PlatformDirectories.ApplicationData, "IntensityThemes");
					PlatformDirectories.EnsureDirectoryExists(intensityThemesDir);
					File.WriteAllText(Path.Combine(intensityThemesDir, IntensityTheme.Meta.Identifier), JsonSerializer.Serialize(theme));
					var newTheme = new ThemeSelector.IntensityTheme(new(ThemeType.ExternalFile, IntensityTheme.Meta.Identifier), theme, theme.CreateResourceDictionary());
					var index = intensityThemes.IndexOf(IntensityTheme);
					intensityThemes.RemoveAt(index);
					intensityThemes.Insert(index, newTheme);
					IntensityTheme = newTheme;
				}
				catch (Exception ex)
				{
					await new FAContentDialog
					{
						Title = Loc?.Get(LocalizationKey.ThemeSaveFailedTitle) ?? "保存に失敗",
						Content = string.Format(Loc?.Get(LocalizationKey.ThemeSaveFailedContent) ?? "テーマの保存に失敗しました: {0}", ex.Message),
						PrimaryButtonText = "OK",
					}.ShowAsync(this);
				}
				return;
			}
		};
	}


	public static readonly DirectProperty<IntensityThemeEditWindow, ThemeSelector.IntensityTheme?> IntensityThemeProperty =
		AvaloniaProperty.RegisterDirect<IntensityThemeEditWindow, ThemeSelector.IntensityTheme?>(nameof(IntensityTheme),
			o => o.IntensityTheme,
			(o, v) => o.IntensityTheme = v);

	private ThemeSelector.IntensityTheme? _intensityTheme = null;
	public ThemeSelector.IntensityTheme? IntensityTheme
	{
		get => _intensityTheme;
		set {
			SetAndRaise(IntensityThemeProperty, ref _intensityTheme, value);
			if (value == null)
				return;
			AssignTheme(value);
			themeDetailText.Text = LocalizedThemeName.Get(value.Meta);
		}
	}

	private void AssignTheme(ThemeSelector.IntensityTheme theme)
	{
		_themeSubscription?.Dispose();
		var cloned = theme.Theme.Clone();
		DataContext = cloned;
		_themeSubscription = cloned.WhenAnyPropertyChanged()
			.Throttle(TimeSpan.FromMilliseconds(100))
			.Subscribe(c =>
			{
				UpdateTheme(c);
				IsSaved = false;
			});
		UpdateTheme(cloned);
		IsSaved = true;
	}

	private void UpdateTheme(IntensityTheme? c)
	{
		if (c == null || KyoshinEewViewerApp.Selector == null)
			return;

		Dispatcher.UIThread.Post(() =>
		{
			c = c.Clone();
			KyoshinEewViewerApp.Selector.SelectedIntensityTheme = new(new(ThemeType.Temporary, "編集中のテーマ"), c, c.CreateResourceDictionary());
		});
	}

	private bool IsSaved { get; set; }
	protected override async void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);

		if (IntensityTheme == null || KyoshinEewViewerApp.Selector == null)
			return;

		if (IsSaved)
		{
			Dispatcher.UIThread.Post(() =>
			{
				KyoshinEewViewerApp.Selector.SelectedIntensityTheme = IntensityTheme;
			});
			return;
		}

		e.Cancel = true;
		var result = await new FAContentDialog
		{
			Title = Loc?.Get(LocalizationKey.ThemeDiscardCloseTitle) ?? "テーマの変更を破棄",
			Content = Loc?.Get(LocalizationKey.ThemeDiscardCloseContent) ?? "ウィンドウを閉じて編集中のテーマの変更を破棄しますか？",
			PrimaryButtonText = Loc?.Get(LocalizationKey.CommonYes) ?? "はい",
			SecondaryButtonText = Loc?.Get(LocalizationKey.CommonNo) ?? "いいえ",
		}.ShowAsync(this);
		if (result == FAContentDialogResult.Primary)
		{
			IsSaved = true;
			Close();
		}
	}
}
